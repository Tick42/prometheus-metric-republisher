using System.Text.Json;

namespace PromRepublisher.MetricsCommon
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
                promTotalJSHeapSize = LinkedPromMetrics[0];
                promUsedJSHeapSize = LinkedPromMetrics[1];
            }
        }

        private IPromMetric promTotalJSHeapSize;
        private IPromMetric promUsedJSHeapSize;
        

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

        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, string[] commonLabels)
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
                    promTotalJSHeapSize.Set(totalJSHeapSize, commonLabels);
                    promUsedJSHeapSize.Set(usedJSHeapSize, commonLabels);
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
