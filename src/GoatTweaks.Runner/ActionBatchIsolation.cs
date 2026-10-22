namespace GoatTweaks.Runner;

public static class ActionBatchIsolation
{
    public static async Task<int> RunAsync<T>(
        IEnumerable<T> actions,
        Func<T, Task<int>> execute,
        Func<T, Exception, Task> logFailure)
    {
        var failures = 0;
        foreach (var action in actions)
        {
            try
            {
                if (await execute(action) != 0)
                    failures++;
            }
            catch (Exception exception)
            {
                failures++;
                await logFailure(action, exception);
            }
        }
        return failures;
    }
}
