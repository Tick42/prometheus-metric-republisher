using System;
using System.Threading;

namespace PromRepublisher.MetricsCommon.Locks
{
    public class RegistryUpdateLock : IMetricRegistryLock
    {
        private readonly ReaderWriterLockSlim lock_;
        public RegistryUpdateLock(ReaderWriterLockSlim lockInstance)
        {
            lockInstance.EnterWriteLock();
            lock_ = lockInstance;
        }
        public void Dispose()
        {
            lock_?.ExitWriteLock();
            GC.SuppressFinalize(this);
        }
    }
    public class MetricValueUpdateLock : IMetricRegistryLock
    {
        private readonly ReaderWriterLockSlim lock_;
        public MetricValueUpdateLock(ReaderWriterLockSlim lockInstance)
        {
            lockInstance.EnterReadLock();
            lock_ = lockInstance;
        }
        public void Dispose()
        {
            lock_?.ExitReadLock();
            GC.SuppressFinalize(this);
        }
    }
}
