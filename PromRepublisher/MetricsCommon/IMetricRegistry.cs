using System.Threading.Tasks;
using PromRepublisher.MetricsCommon.Locks;

namespace PromRepublisher.MetricsCommon
{
    public interface IMetricRegistry
    {
        public void AddGlueMetric(IGlueMetric glueMetric);
        public IGlueMetric GetMetricByPropName(string metricPropName);
        public void ReportUnhandledMetric(string[] labels);
        public Task<string> CollectAndSerializeAsync();
        public IMetricRegistryLock AcquireMetricValueUpdateLock();
        public IMetricRegistryLock AcquireRegistryUpdateLock();
    }
}
