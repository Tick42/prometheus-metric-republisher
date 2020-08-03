using System.Text.Json;
using Microsoft.Extensions.Logging;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.MetricsGlue
{
    public class PerfMemoryMetric : IGlueMetric
    {
        public string GlueMetricPropName { get; } = "/App/performance/memory";
        public PromMetricDef[] PromMetricDefs { get; }
        public IMetricRegistry Registry { get; set; }

        private IPromMetric[] linkedPromMetrics_;
        public IPromMetric[] LinkedPromMetrics
        {
            get
            {
                return linkedPromMetrics_;
            }
            set
            {
                linkedPromMetrics_ = value;
                promTotalJSHeapSize_ = LinkedPromMetrics[0];
                promUsedJSHeapSize_ = LinkedPromMetrics[1];
            }
        }

        private IPromMetric promTotalJSHeapSize_;
        private IPromMetric promUsedJSHeapSize_;
        

        public PerfMemoryMetric()
        {
            PromMetricDefs = new PromMetricDef[]
            {
                new PromMetricDef()
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_js_heapsize_total",
                    Help = "Javascript Heapsize Total",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = true,
                },
                new PromMetricDef()
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_js_heapsize_used",
                    Help = "Javascript Heapsize Used",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = true,
                },
            };
        }

        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, CommonLabelsInfo commonLabels, ILogger logger)
        {
            try
            {
                long totalJSHeapSize = 0;
                long usedJSHeapSize = 0;

                var datapointsEl = metricEl.GetProperty("datapoints");
                int nDatapoints = datapointsEl.GetArrayLength();
                for (int i = 0; i < nDatapoints; ++i)
                {
                    JsonElement datapointEl = datapointsEl[i];
                    string value = datapointEl.GetProperty("value").GetProperty("value").ToString();
                    using (JsonDocument doc = JsonDocument.Parse(value))
                    {
                        totalJSHeapSize = doc.RootElement.GetProperty("totalJSHeapSize").GetInt64();
                        usedJSHeapSize = doc.RootElement.GetProperty("usedJSHeapSize").GetInt64();
                    }
                }

                using (Registry.AcquireMetricValueUpdateLock())
                {
                    promTotalJSHeapSize_.Set(totalJSHeapSize, commonLabels);
                    promUsedJSHeapSize_.Set(usedJSHeapSize, commonLabels);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {

            }
        }
    }
}
