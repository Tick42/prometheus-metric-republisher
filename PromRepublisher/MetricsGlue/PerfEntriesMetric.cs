using System.Text.Json;

namespace PromRepublisher.MetricsCommon
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
                promResourceCount = LinkedPromMetrics[0];
                promResourceSize = LinkedPromMetrics[1];
                promResourceDuration = LinkedPromMetrics[2];
                promNavigationCount = LinkedPromMetrics[3];
                promNavigationDuration = LinkedPromMetrics[4];
                promPaintCount = LinkedPromMetrics[5];
                promPaintDuration = LinkedPromMetrics[6];
            }
        }

        private IPromMetric promResourceCount;
        private IPromMetric promResourceSize;
        private IPromMetric promResourceDuration;
        private IPromMetric promNavigationCount;
        private IPromMetric promNavigationDuration;
        private IPromMetric promPaintCount;
        private IPromMetric promPaintDuration;

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
                    Name = "glue_web_paint_count",
                    Help = "Number of reported paint entries",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
                new PromMetricDef() // index 6
                {
                    MetricType = PromMetricType.Counter,
                    Name = "glue_web_paint_duration_total",
                    Help = "Total duration of paint entries",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                },
            };
        }

        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, string[] commonLabels)
        {
            try
            {
                long resourceCount = 0;
                long resourceSize = 0;
                double resourceDuration = 0;
                long navigationCount = 0;
                double navigationDuration = 0;
                long paintCount = 0;
                double paintDuration = 0;

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
                                    navigationCount++;
                                    navigationDuration += duration;
                                    continue;
                                }
                                if (entryType == "paint")
                                {
                                    double duration = entryEl.GetProperty("duration").GetDouble();
                                    paintCount++;
                                    paintDuration += duration;
                                    continue;
                                }
                            }
                            catch
                            {
                                continue; // silently ignore issues with the entries
                            }
                        }

                    }
                }

                using (Registry.AcquireMetricValueUpdateLock())
                {
                    promResourceCount.Inc(resourceCount, commonLabels);
                    promResourceSize.Inc(resourceSize, commonLabels);
                    promResourceDuration.Inc(resourceDuration, commonLabels);
                    promNavigationCount.Inc(navigationCount, commonLabels);
                    promNavigationDuration.Inc(navigationDuration, commonLabels);
                    promPaintCount.Inc(paintCount, commonLabels);
                    promPaintDuration.Inc(paintDuration, commonLabels);
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
