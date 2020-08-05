using System.Text.Json;
using Microsoft.Extensions.Logging;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.MetricsGlue
{
    public class CustomMetricSet01 : IGlueMetric
    {
        public string GlueMetricPropName { get; } = "/App/customSystem/customMetricSet01";
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
                promMyCounter_ = LinkedPromMetrics[0];
                promMyGauge_ = LinkedPromMetrics[1];
            }
        }

        private IPromMetric promMyCounter_;
        private IPromMetric promMyGauge_;


        public CustomMetricSet01()
        {
            PromMetricDefs = new PromMetricDef[]
            {
                new PromMetricDef()
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_custom_my_counter",
                    Help = "Sample custom counter",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = true,
                },
                new PromMetricDef()
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_custom_my_gauge",
                    Help = "Sample custom gauge",
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
                long myCounter = 0;
                long myGauge = 0;

                var datapointsEl = metricEl.GetProperty("datapoints");
                int nDatapoints = datapointsEl.GetArrayLength();
                for (int i = 0; i < nDatapoints; ++i)
                {
                    JsonElement valueSetEl = datapointsEl[i].GetProperty("value").GetProperty("value");
                    myCounter = valueSetEl.GetProperty("myCounter").GetProperty("value").GetInt32();
                    myGauge = valueSetEl.GetProperty("myGauge").GetProperty("value").GetInt32();
                }

                using (Registry.AcquireMetricValueUpdateLock())
                {
                    promMyCounter_.Set(myCounter, commonLabels);
                    promMyGauge_.Set(myGauge, commonLabels);
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
