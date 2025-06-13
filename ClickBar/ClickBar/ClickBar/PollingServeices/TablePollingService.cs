using ClickBar_Logging;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;

namespace ClickBar.PollingServeices
{
    public sealed class TablePollingService
    {
        #region Fields Singleton
        private static readonly Lazy<TablePollingService> _instance = new Lazy<TablePollingService>(() => new TablePollingService());
        #endregion Fields Singleton

        #region Fields


        private CancellationTokenSource _cancellationTokenSource;
        private Task _pollingTask;
        //public Timer Timer1 { get; private set; }
        public static TablePollingService Instance => _instance.Value;

        #endregion Fields

        #region Constructors
        private TablePollingService() { }
        #endregion Constructors

        #region Public methods
        //public void StartPolling(Action<object, ElapsedEventArgs> checkDatabase)
        //{
        //    if (Timer != null)
        //    {
        //        Timer.Stop();
        //        Timer.Dispose();
        //    }
        //    Timer = new Timer(5000); // Proverava svakih 5 sekundi
        //    Timer.Elapsed += new ElapsedEventHandler(checkDatabase);
        //    Timer.AutoReset = true;
        //    Timer.Enabled = true;
        //}
        public void StartPollingStatusStolova(Func<object, ElapsedEventArgs, Task> checkDatabaseStatusStolovaAsync)
        {
            StopPolling(); // Zaustavi prethodni polling ako postoji

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            // Pokreni asinhroni task za polling
            _pollingTask = Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await checkDatabaseStatusStolovaAsync(null, null);
                    }
                    catch (Exception ex)
                    {
                        // Loguj grešku ako je potrebno
                        Log.Error("TablePollingService -> StartPollingStatusStolova -> Greška prilikom izvršavanja polling-a", ex);
                        Console.WriteLine($"Greška prilikom izvršavanja polling-a: {ex.Message}");
                    }

                    await Task.Delay(10000, cancellationToken); // Pauza od 5 sekundi između poziva
                }
            }, cancellationToken);
        }
        //public void StartPollingStatusStolova(Action<object, ElapsedEventArgs> checkDatabaseStatusStolova)
        //{
        //    if (Timer1 != null)
        //    {
        //        Timer1.Stop();
        //        Timer1.Dispose();
        //    }
        //    Timer1 = new Timer(5000); // Proverava svakih 5 sekundi
        //    Timer1.Elapsed += new ElapsedEventHandler(checkDatabaseStatusStolova);
        //    Timer1.AutoReset = true;
        //    Timer1.Enabled = true;
        //}
        #endregion Public methods

        #region Private methods
        private void StopPolling()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _pollingTask?.Wait(); // Sačekaj da se zadatak završi
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }
        #endregion Private methods
    }
}

