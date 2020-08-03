using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PromRepublisher.MetricsProm;
using PromRepublisher.MetricsCommon.Locks;

namespace PromRepublisher.MetricsCommon
{
    public class MetricRegistry : IMetricRegistry
    {
        private readonly Dictionary<string, IGlueMetric> glueMetrics_ = new Dictionary<string, IGlueMetric>();
        private readonly Dictionary<string, IPromMetric> promMetrics_ = new Dictionary<string, IPromMetric>();
        private readonly IPromMetric unhandled_;

        private readonly ReaderWriterLockSlim lock_ = new ReaderWriterLockSlim();

        public MetricRegistry()
        {
            IGlueMetric unhandledMetric = new UnhandledMetric();
            AddGlueMetric(unhandledMetric);
            unhandled_ = unhandledMetric.LinkedPromMetrics[0];
        }
        public void AddGlueMetric(IGlueMetric glueMetric)
        {
            List<IPromMetric> promMetrics = new List<IPromMetric>();
            foreach (PromMetricDef def in glueMetric.PromMetricDefs)
            {
                IPromMetric promMetric;
                string[] labels = def.CommonLabels.Concat(def.SpecificLabels).ToArray();
                switch (def.MetricType)
                {
                    case PromMetricType.Counter:
                        promMetric = new PromCounterMetric(def.Name, def.Help, labels);
                        break;
                    case PromMetricType.Gauge:
                        promMetric = new PromGaugeMetric(def.Name, def.Help, labels);
                        break;
                    default:
                        throw new Exception("Invalid metric type");
                }
                promMetric.UnpublishOnCollect = def.UnpublishOnCollect;
                promMetrics.Add(promMetric);
            }
            glueMetric.LinkedPromMetrics = promMetrics.ToArray();
            glueMetric.Registry = this;
            glueMetrics_.Add(glueMetric.GlueMetricPropName, glueMetric);
            foreach (var promMetric in promMetrics)
            {
                promMetrics_.Add(promMetric.Name, promMetric);
            }

            return;
        }

        public IGlueMetric GetMetricByPropName(string metricPropName)
        {
            return glueMetrics_.TryGetValue(metricPropName, out IGlueMetric glueMetric) ? glueMetric: null;
        }

        public void ReportUnhandledMetric(string[] labels)
        {
            unhandled_.Inc(1, labels);
        }

        public IMetricRegistryLock AcquireMetricValueUpdateLock()
        {
            return new MetricValueUpdateLock(lock_);
        }
        public IMetricRegistryLock AcquireRegistryUpdateLock()
        {
            return new RegistryUpdateLock(lock_);
        }

        public async Task<string> CollectAndSerializeAsync()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using(AcquireRegistryUpdateLock())
                {
                    await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                    UnpublishMetricsOnCollect();
                }
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    return await sr.ReadToEndAsync(); ;
                }
            }
        }

        private void UnpublishMetricsOnCollect()
        {
            foreach(var keyValue in promMetrics_)
            {
                IPromMetric met = keyValue.Value;
                if (met.UnpublishOnCollect)
                {
                    met.Unpublish();
                }
            }
        }
    }
}
