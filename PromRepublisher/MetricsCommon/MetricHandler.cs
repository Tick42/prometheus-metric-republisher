using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PromRepublisher.MetricsCommon
{
    public class MetricHandler
    {
        private readonly IMetricRegistry metricRegistry_;

        const string metricsProp = "metrics";

        public MetricHandler(IMetricRegistry metricRegistry)
        {
            metricRegistry_ = metricRegistry;
        }

        public void OnMetric(ref JsonElement jel, ILogger logger)
        {
            CommonLabelsInfo commonLabels = IGlueMetric.GetCommonLabelValues(ref jel);

            // find the "metrics" object
            if (!jel.TryGetProperty(metricsProp, out JsonElement metricsEl))
            {
                //metricRegistry_.ReportUnhandledMetric(commonLabels.Concat(new string[] { "" }).ToArray());
                return;
            }
            if (metricsEl.ValueKind == JsonValueKind.Null)
            {
                //metricRegistry_.ReportUnhandledMetric(commonLabels.Concat(new string[] { "" }).ToArray());
                return;
            }

            // enumerate the properties of the "metrics" object
            using (var objEnum = metricsEl.EnumerateObject())
            {
                foreach (var item in objEnum)
                {
                    string propName = item.Name;
                    IGlueMetric glueMetric = metricRegistry_.GetMetricByPropName(propName);
                    if(glueMetric is null)
                    {
                        metricRegistry_.ReportUnhandledMetric(commonLabels.ConcatLabels(propName));
                        continue;
                    }

                    JsonElement metricEl = item.Value;
                    if(!glueMetric.ParseJson(ref metricEl, ref jel, commonLabels, logger))
                    {
                        metricRegistry_.ReportUnhandledMetric(commonLabels.ConcatLabels(propName));
                    }
                }
            }

            return;
        }
    }
}
