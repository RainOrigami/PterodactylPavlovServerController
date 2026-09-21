namespace PterodactylPavlovServerController.Services;

/// <summary>
/// Pavlov's RCON silently drops commands when it is busy, so anything that must actually land
/// is sent through here: the command reply is checked and the command is repeated a bounded
/// number of times. The cap is what keeps a disconnected player from being retried forever.
/// </summary>
public static class RconRetry
{
    public const int DefaultAttempts = 3;

    public static async Task<bool> Ensure(string description, Func<Task<bool>> command, int maxAttempts = RconRetry.DefaultAttempts, int retryDelayMilliseconds = 150)
    {
        for (int attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (await command())
                {
                    if (attempt > 1)
                    {
                        await Console.Out.WriteLineAsync($"{description} succeeded on attempt {attempt}/{maxAttempts}");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync($"{description} threw on attempt {attempt}/{maxAttempts}: {ex.Message}");
            }

            if (attempt < maxAttempts)
            {
                await Task.Delay(retryDelayMilliseconds * attempt);
            }
        }

        await Console.Out.WriteLineAsync($"{description} failed after {maxAttempts} attempts");
        return false;
    }
}
