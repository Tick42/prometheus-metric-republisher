using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using PromRepublisher.MetricsCommon;

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
        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, string[] commonLabels)
        {
            // nothing: this metric is used internally
            return true;
        }
    }
}
