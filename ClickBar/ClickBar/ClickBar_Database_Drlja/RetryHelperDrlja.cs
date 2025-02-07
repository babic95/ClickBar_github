using System;
using System.Threading;
using ClickBar_Logging;
using Microsoft.Data.Sqlite; // Dodajte za SqliteException

public static class RetryHelperDrlja
{
    public static void ExecuteWithRetry(Action action, int maxRetryCount = 10, int initialDelayMilliseconds = 200)
    {
        int retryCount = 0;
        int delayMilliseconds = initialDelayMilliseconds;

        while (true)
        {
            try
            {
                action();
                break;
            }
            catch (SqliteException ex) when (ex.SqliteErrorCode == 5) // Kod 5 je za zaključanu bazu
            {
                if (retryCount >= maxRetryCount)
                {
                    throw;
                }

                Log.Debug($"RetryHelperDrlja -> ExecuteWithRetry -> Ceka se na cuvanje delayMilliseconds -> {delayMilliseconds}");
                retryCount++;
                Thread.Sleep(delayMilliseconds);
                delayMilliseconds *= 2; // Eksponencijalni rast čekanja
            }
        }
    }
}