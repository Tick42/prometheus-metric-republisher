using System.Text.Json;
using Microsoft.Extensions.Logging;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.MetricsGlue
{
    public class PerfEntriesMetric : IGlueMetric
    {
        public string GlueMetricPropName { get; } = "/App/performance/entries";
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
                promResourceCount_ = LinkedPromMetrics[0];
                promResourceSize_ = LinkedPromMetrics[1];
                promResourceDuration_ = LinkedPromMetrics[2];
                promNavigationCount_ = LinkedPromMetrics[3];
                promNavigationDuration_ = LinkedPromMetrics[4];
                promNavigationRenderDuration_ = LinkedPromMetrics[5];
            }
        }

        private IPromMetric promResourceCount_;
        private IPromMetric promResourceSize_;
        private IPromMetric promResourceDuration_;
        private IPromMetric promNavigationCount_;
        private IPromMetric promNavigationDuration_;
        private IPromMetric promNavigationRenderDuration_;

        public PerfEntriesMetric()
        {
            PromMetricDefs = new PromMetricDef[]
            {
                new PromMetricDef() // index 0
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_resource_count",
                    Help = "Number of web resources loaded",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 1
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_resource_size_total",
                    Help = "Size of all loaded web resources",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 2
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_resource_duration_total",
                    Help = "Total load duration of all web resources",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 3
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_navigation_count",
                    Help = "Number of reported navigation entries",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 4
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_navigation_duration_total",
                    Help = "Total duration of navigation entries",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 5
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_navigation_render_duration_total",
                    Help = "Total rendering duration",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
            };
        }

        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, CommonLabelsInfo commonLabels, ILogger logger)
        {
            try
            {
                long resourceCount = 0;
                long resourceSize = 0;
                double resourceDuration = 0;
                long navigationCount = 0;
                double navigationDuration = 0;
                double navigationRenderDuration = 0;
                //long paintCount = 0;
                //double paintDuration = 0;

                var datapointsEl = metricEl.GetProperty("datapoints");
                int nDatapoints = datapointsEl.GetArrayLength();
                for (int i = 0; i < nDatapoints; ++i)
                {
                    JsonElement datapointEl = datapointsEl[i];
                    string value = datapointEl.GetProperty("value").GetProperty("value").ToString();
                    using (JsonDocument doc = JsonDocument.Parse(value))
                    {
                        var entriesEl = doc.RootElement;
                        int nEntries = entriesEl.GetArrayLength();
                        for (int j = 0; j < nEntries; ++j)
                        {
                            JsonElement entryEl = entriesEl[j];
                            try
                            {
                                string entryType = entryEl.GetProperty("entryType").ToString();
                                if (entryType == "resource")
                                {
                                    long transferSize = entryEl.GetProperty("transferSize").GetInt64();
                                    double duration = entryEl.GetProperty("duration").GetDouble();
                                    resourceCount++;
                                    resourceSize += transferSize;
                                    resourceDuration += duration;
                                    continue;
                                }
                                if (entryType == "navigation")
                                {
                                    double duration = entryEl.GetProperty("duration").GetDouble();
                                    double responseEnd = entryEl.GetProperty("responseEnd").GetDouble();
                                    double domComplete = entryEl.GetProperty("domComplete").GetDouble();
                                    navigationCount++;
                                    navigationDuration += duration;
                                    navigationRenderDuration += domComplete - responseEnd;
                                    continue;
                                }
                                //if (entryType == "paint")
                                //{
                                //    double duration = entryEl.GetProperty("duration").GetDouble();
                                //    paintCount++;
                                //    paintDuration += duration;
                                //    continue;
                                //}
                            }
                            catch
                            {
                                continue; // silently ignore issues with the entries
                            }
                        } // entries loop
                    } // using JsonDocument
                } // datapoints loop

                using (Registry.AcquireMetricValueUpdateLock())
                {
                    promResourceCount_.Inc(resourceCount, commonLabels);
                    promResourceSize_.Inc(resourceSize, commonLabels);
                    promResourceDuration_.Inc(resourceDuration, commonLabels);
                    promNavigationCount_.Inc(navigationCount, commonLabels);
                    promNavigationDuration_.Inc(navigationDuration, commonLabels);
                    promNavigationRenderDuration_.Inc(navigationRenderDuration, commonLabels);
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
