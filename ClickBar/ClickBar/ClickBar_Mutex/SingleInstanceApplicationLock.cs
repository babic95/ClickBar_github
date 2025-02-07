using System.Security.AccessControl;
using System.Security.Principal;

namespace ClickBar_Mutex
{
    public sealed class SingleInstanceApplicationLock : IDisposable
    {
        ~SingleInstanceApplicationLock()
        {
            Dispose(false);
        }

        void IDisposable.Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool TryAcquireExclusiveLock()
        {
            try
            {
                if (!_mutex.WaitOne(1000, false))
                    return false;
            }
            catch (AbandonedMutexException)
            {
                // Abandoned mutex, just log? Multiple instances
                // may be executed in this condition...
            }

            return _hasAcquiredExclusiveLock = true;
        }
#if CRNO
        private const string MutexId = @"Local\{00D9FC47-6D29-44D0-9ED7-AD54D8F52AD3}"; //crno
#else
        private const string MutexId = @"Local\{56CFA2F6-8FFE-4FD0-A719-2C051B42080D}"; //belo
#endif
        private readonly System.Threading.Mutex _mutex = CreateMutex();
        private bool _hasAcquiredExclusiveLock, _disposed;

        private void Dispose(bool disposing)
        {
            if (disposing && !_disposed && _mutex != null)
            {
                try
                {
                    if (_hasAcquiredExclusiveLock)
                        _mutex.ReleaseMutex();

                    _mutex.Dispose();
                }
                finally
                {
                    _disposed = true;
                }
            }
        }

        private static System.Threading.Mutex CreateMutex()
        {
            var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
            var allowEveryoneRule = new MutexAccessRule(sid,
                MutexRights.FullControl, AccessControlType.Allow);

            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            var mutex = new System.Threading.Mutex(false, MutexId);
            mutex.SetAccessControl(securitySettings);

            return mutex;
        }
    }
}
