using System;
using System.Threading;
using ClickBar_Logging;
using Microsoft.Data.Sqlite;

public static class RetryHelper
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
            catch (SqliteException ex)
            {
                Log.Debug($"RetryHelper -> ExecuteWithRetry -> Ceka se da se baza otkljuca: {ex.SqliteErrorCode}");
                //Console.WriteLine($"Caught SqliteException with error code: {ex.SqliteErrorCode}");

                if (ex.SqliteErrorCode == 5) // Kod 5 je za zaključanu bazu
                {
                    if (retryCount >= maxRetryCount)
                    {
                        throw;
                    }

                    retryCount++;
                    Thread.Sleep(delayMilliseconds);
                    delayMilliseconds *= 2; // Eksponencijalni rast čekanja
                }
                else
                {
                    throw; // Ponovno bacanje izuzetka koji nije povezano sa zaključavanjem baze
                }
            }
            catch (Exception ex)
            {
                Log.Error($"RetryHelper -> ExecuteWithRetry -> desila se greska: ", ex);
                throw;
            }
        }
    }
}