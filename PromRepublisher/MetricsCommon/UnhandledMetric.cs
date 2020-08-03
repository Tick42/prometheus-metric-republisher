using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PromRepublisher.MetricsCommon
{
    public class UnhandledMetric : IGlueMetric
    {
        public string GlueMetricPropName { get; } = "!unhandled!";
        public PromMetricDef[] PromMetricDefs { get; }
        public IPromMetric[] LinkedPromMetrics { get; set; }
        public IMetricRegistry Registry { get; set; }

        public UnhandledMetric()
        {
            PromMetricDefs = new PromMetricDef[]
            {
                new PromMetricDef()
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_unhandled_metric_counter",
                    Help = "Unhandled Metrics Count",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[]{"metricPropName"},
                },
            };
        }
        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, CommonLabelsInfo commonLabels, ILogger logger)
        {
            // nothing: this metric is used internally
            return true;
        }
    }
}
