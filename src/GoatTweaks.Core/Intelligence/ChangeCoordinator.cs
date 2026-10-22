namespace GoatTweaks.Core.Intelligence;

public interface ISettingsGateway
{
    Task<MachineFacts> ReadMachineAsync(CancellationToken token);
    Task<SettingValue> ReadAsync(string settingId, CancellationToken token);
    Task WriteAsync(string settingId, SettingValue value, CancellationToken token);
}

/// <summary>Only three explicit low-risk targets are automatic. The legacy script executor is never called here.</summary>
public sealed class ChangeCoordinator(ISettingsGateway settings, ISessionJournal journal)
{
    public static void Approve(ServiceSession session, IEnumerable<string> ids)
    {
        if (session.Profile == OptimizationProfile.Diagnostic) throw new InvalidOperationException("O perfil Diagnóstico não autoriza alterações.");
        if (session.Before is null) throw new InvalidOperationException("Execute o diagnóstico inicial antes do plano.");
        if (session.Changes.Any(c => c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária"))
            throw new InvalidOperationException("O atendimento já contém mudanças. Valide ou restaure antes de criar outro plano em um novo atendimento.");
        var selected = ids.Distinct(StringComparer.Ordinal).OrderBy(id => id).ToList();
        if (selected.Count == 0) throw new InvalidOperationException("Selecione pelo menos um ajuste executável.");
        foreach (var id in selected)
        {
            var recommendation = session.Recommendations.SingleOrDefault(r => r.Id == id);
            if (recommendation is null || !recommendation.CanAuthorize) throw new InvalidOperationException("O plano contém uma operação não autorizável automaticamente.");
            ValidateChanges(recommendation.Changes);
        }
        session.AuthorizedIds = selected;
        session.ApprovedAt = DateTimeOffset.UtcNow;
        session.ApprovedPlanHash = session.PlanHash();
        session.Stage = ServiceStage.Plan;
    }

    public async Task ExecuteAsync(ServiceSession session, IProgress<string>? progress, CancellationToken token)
    {
        using var mutation = journal.AcquireMutationLock();
        if (session.Profile == OptimizationProfile.Diagnostic || session.Before is null || session.ApprovedAt is null ||
            DateTimeOffset.UtcNow - session.ApprovedAt > TimeSpan.FromMinutes(30) ||
            session.AuthorizedIds.Count == 0 || session.ApprovedPlanHash != session.PlanHash())
            throw new InvalidOperationException("Plano ausente, alterado ou expirado. Revise a prévia e autorize novamente.");
        var machine = await settings.ReadMachineAsync(token);
        if (!machine.Supported || machine.Fingerprint != session.Before.Machine.Fingerprint)
            throw new InvalidOperationException("Máquina, usuário, sistema ou driver mudou desde o diagnóstico. Refaça a análise.");
        session.Stage = ServiceStage.Execution;
        await journal.SaveAsync(session, token);
        foreach (var id in session.AuthorizedIds.ToArray())
        {
            token.ThrowIfCancellationRequested();
            var recommendation = session.Recommendations.Single(r => r.Id == id);
            if (!recommendation.CanAuthorize) throw new InvalidOperationException("A recomendação deixou de ser executável.");
            ValidateChanges(recommendation.Changes);
            var record = new ChangeRecord { RecommendationId = id, Title = recommendation.Title,
                Changes = recommendation.Changes.ToList(), RestartRequired = recommendation.RestartRequired };
            session.Changes.Add(record);
            progress?.Report("Preparando backup · " + recommendation.Title);
            var stale = false;
            foreach (var change in record.Changes)
                if (await settings.ReadAsync(change.SettingId, token) != change.Before) stale = true;
            if (id == IntelligenceSettings.PowerPlan && (machine.Notebook != false || machine.OnAcPower != true ||
                !machine.AvailablePowerPlans.Contains(IntelligenceSettings.BalancedGuid, StringComparer.OrdinalIgnoreCase))) stale = true;
            if (stale)
            {
                record.State = "Ignorado · estado ou compatibilidade mudou";
                await journal.SaveAsync(session, token);
                continue;
            }
            // If this durable save fails, execution stops before any setting is changed.
            await journal.SaveAsync(session, token);
            try
            {
                foreach (var change in record.Changes)
                {
                    token.ThrowIfCancellationRequested();
                    await settings.WriteAsync(change.SettingId, change.Desired, token);
                    if (await settings.ReadAsync(change.SettingId, token) != change.Desired)
                        throw new IOException("A leitura após a aplicação não corresponde ao estado pretendido.");
                }
                record.State = "Aplicado e verificado";
                progress?.Report("Configuração verificada · " + record.Title + ". Benefício no jogo ainda não medido.");
            }
            catch (Exception exception)
            {
                record.Error = exception.Message;
                try
                {
                    await RestoreValuesAsync(record, CancellationToken.None);
                    record.State = "Falhou · estado anterior restaurado";
                }
                catch (Exception rollback) { record.State = "Revisão necessária"; record.Error += " | Restauração: " + rollback.Message; }
                progress?.Report(record.State + " · " + record.Title);
            }
            // Failure to persist stops further writes. The on-disk prepared record still contains the backup.
            await journal.SaveAsync(session, CancellationToken.None);
        }
        session.ApprovedPlanHash = null;
        session.Stage = session.Changes.Any(c => c.State == "Aplicado e verificado" && c.RestartRequired) ? ServiceStage.Restart : ServiceStage.Validation;
        await journal.SaveAsync(session, CancellationToken.None);
    }

    public async Task RestoreAsync(ServiceSession session, IProgress<string>? progress, CancellationToken token)
    {
        if(session.Profile==OptimizationProfile.Diagnostic) throw new InvalidOperationException("O perfil Diagnóstico não executa restaurações.");
        using var mutation = journal.AcquireMutationLock();
        var machine = await settings.ReadMachineAsync(token);
        if (session.Before is null || machine.MachineId != session.Before.Machine.MachineId || machine.UserId != session.Before.Machine.UserId)
            throw new InvalidOperationException("O backup pertence a outra máquina ou usuário.");
        foreach (var record in session.Changes.AsEnumerable().Reverse().Where(c => c.State is "Aplicado e verificado" or "Preparado" or "Revisão necessária"))
        {
            token.ThrowIfCancellationRequested();
            ValidateChanges(record.Changes);
            await journal.SaveAsync(session, token);
            try { await RestoreValuesAsync(record, token); record.State = "Restaurado e verificado"; }
            catch (Exception ex) { record.State = "Revisão necessária"; record.Error = ex.Message; }
            progress?.Report(record.State + " · " + record.Title);
            await journal.SaveAsync(session, CancellationToken.None);
        }
        session.ApprovedPlanHash = null;
        session.Stage = ServiceStage.Validation;
        await journal.SaveAsync(session, CancellationToken.None);
    }

    private async Task RestoreValuesAsync(ChangeRecord record, CancellationToken token)
    {
        foreach (var change in record.Changes.AsEnumerable().Reverse())
        {
            var current = await settings.ReadAsync(change.SettingId, token);
            if (current == change.Before) continue;
            if (current != change.Desired) throw new InvalidOperationException("A configuração foi alterada por outra origem. A reversão automática não sobrescreveu esse estado.");
            await settings.WriteAsync(change.SettingId, change.Before, token);
            if (await settings.ReadAsync(change.SettingId, token) != change.Before) throw new IOException("A restauração não foi confirmada pela leitura.");
        }
    }

    public static void ValidateChanges(IReadOnlyList<ProposedChange> changes)
    {
        if (changes.Count != 1) throw new InvalidDataException("Cada ajuste gerenciado deve conter exatamente uma alteração isolada.");
        foreach (var change in changes)
        {
            var allowed = change.SettingId switch
            {
                IntelligenceSettings.GameMode => change.Before == SettingValue.Dword(0) && change.Desired == SettingValue.Dword(1),
                IntelligenceSettings.BackgroundCapture => change.Before == SettingValue.Dword(1) && change.Desired == SettingValue.Dword(0),
                IntelligenceSettings.PowerPlan => change.Before == SettingValue.PowerPlan(IntelligenceSettings.PowerSaverGuid) && change.Desired == SettingValue.PowerPlan(IntelligenceSettings.BalancedGuid),
                _ => false
            };
            if (!allowed) throw new InvalidDataException("Alvo ou transição fora das operações gerenciadas permitidas.");
        }
    }
}
