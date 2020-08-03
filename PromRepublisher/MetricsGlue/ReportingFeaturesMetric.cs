using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.MetricsGlue
{
    public class ReportingFeaturesMetric : IGlueMetric
    {
        public string GlueMetricPropName { get; } = "/App/reporting/features";
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
                promAppMemoryKB_ = LinkedPromMetrics[0];
                promAppCpu_ = LinkedPromMetrics[1];
                promSysMemoryFree_ = LinkedPromMetrics[2];
                promSysSwapMemoryTotal_ = LinkedPromMetrics[3];
                promSysSwapMemoryFree_ = LinkedPromMetrics[4];
                promSysCpu_ = LinkedPromMetrics[5];
                promGlueMemory_ = LinkedPromMetrics[6];
                promGlueCpu_ = LinkedPromMetrics[7];
            }
        }

        private IPromMetric promAppMemoryKB_;
        private IPromMetric promAppCpu_;
        private IPromMetric promSysMemoryFree_;
        private IPromMetric promSysSwapMemoryTotal_;
        private IPromMetric promSysSwapMemoryFree_;
        private IPromMetric promSysCpu_;
        private IPromMetric promGlueMemory_;
        private IPromMetric promGlueCpu_;

        public ReportingFeaturesMetric()
        {
            PromMetricDefs = new PromMetricDef[]
            {
                new PromMetricDef() // index 0
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_app_process_memory_kb",
                    Help = "Application process memory in kB",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 1
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_app_process_cpu_percent",
                    Help = "Application process CPU usage",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 2
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_sys_memory_free_gb",
                    Help = "Free system memory in GB",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 3
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_sys_swap_memory_total_gb",
                    Help = "Total system swap memory in GB",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 4
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_sys_swap_memory_free_gb",
                    Help = "Free system swap memory in GB",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 5
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_sys_cpu_percent",
                    Help = "Total CPU usage in percent",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 6
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_memory_gb",
                    Help = "Memory used by Glue in GB",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },
                new PromMetricDef() // index 7
                {
                    MetricType = PromMetricType.Gauge,
                    Name = "glue_cpu_percent",
                    Help = "Glue CPU usage",
                    CommonLabels = IGlueMetric.CommonLabels,
                    SpecificLabels = new string[0],
                    UnpublishOnCollect = false
                },

            };
        }

        private class PerfAppInfo
        {
            public string AppName;
            public string Pid;
            public int MemoryKB;
            public double Cpu;
        }
        private class PerfSummaryInfo
        {
            public double sysMemoryFree;
            public double sysSwapMemoryTotal;
            public double sysSwapMemoryFree;
            public double sysCpu;
            public double glueMemory;
            public double glueCpu;
        }

        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, CommonLabelsInfo commonLabels, ILogger logger)
        {
            try
            {
                bool bPerfSummaryEncountered = false;
                PerfSummaryInfo perfSummaryInfo = new PerfSummaryInfo();
                List<PerfAppInfo> perfAppInfos = new List<PerfAppInfo>();
                var datapointsEl = metricEl.GetProperty("datapoints");
                int nDatapoints = datapointsEl.GetArrayLength();
                for (int i = 0; i < nDatapoints; ++i)
                {
                    try
                    {
                        JsonElement favMetric = datapointsEl[i].GetProperty("value").GetProperty("value");
                        string favName = favMetric.GetProperty("name").GetProperty("value").ToString();
                        switch (favName)
                        {
                            case "PerfApp":
                                ParsePerfApp(ref favMetric, perfAppInfos, logger);
                                break;
                            case "PerfSummary":
                                bPerfSummaryEncountered = true;
                                ParsePerfSummary(ref favMetric, perfSummaryInfo, logger);
                                break;
                            default:
                                continue;
                        }

                    }
                    catch
                    {
                        continue; // silently ignore issues with the datapoints
                    }
                } // datapoints loop
                
                int nAppsReported = perfAppInfos.Count;

                // reset the last reported app performance metrics
                if (bPerfSummaryEncountered || nAppsReported > 0)
                {
                    using (Registry.AcquireRegistryUpdateLock())
                    {
                        promAppMemoryKB_.Unpublish();
                        promAppCpu_.Unpublish();
                    }
                }

                using (Registry.AcquireMetricValueUpdateLock())
                {
                    // report the summary info
                    promSysMemoryFree_.Set(perfSummaryInfo.sysSwapMemoryFree, commonLabels);
                    promSysSwapMemoryTotal_.Set(perfSummaryInfo.sysSwapMemoryTotal, commonLabels);
                    promSysSwapMemoryFree_.Set(perfSummaryInfo.sysSwapMemoryFree, commonLabels);
                    promSysCpu_.Set(perfSummaryInfo.sysCpu, commonLabels);
                    promGlueMemory_.Set(perfSummaryInfo.glueMemory, commonLabels);
                    promGlueCpu_.Set(perfSummaryInfo.glueCpu, commonLabels);

                    // report the app performance metrics
                    CommonLabelsInfo appLabels = commonLabels.Clone();
                    foreach (var info in perfAppInfos)
                    {
                        appLabels.AppName = info.AppName;
                        appLabels.Pid = info.Pid;
                        promAppMemoryKB_.Set(info.MemoryKB, appLabels);
                        promAppCpu_.Set(info.Cpu, appLabels);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message + "\n" + ex.StackTrace);
                return false;
            }
            finally
            {

            }
        }

        private void ParsePerfApp(ref JsonElement fav, List<PerfAppInfo> perfAppInfos, ILogger logger)
        {
            using (JsonDocument doc = JsonDocument.Parse(fav.GetProperty("payload").GetProperty("value").GetString()))
            {
                try
                {
                    JsonElement root = doc.RootElement;
                    PerfAppInfo info = new PerfAppInfo();
                    info.AppName = root.GetProperty("name").ToString();
                    info.Pid = root.GetProperty("pid").ToString();
                    info.MemoryKB = root.TryGetProperty("memoryKB", out JsonElement memoryKBEl) ? memoryKBEl.GetInt32() : 0;
                    info.Cpu = root.TryGetProperty("cpu", out JsonElement cpuEl) ? cpuEl.GetDouble() : 0;
                    perfAppInfos.Add(info);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

        private void ParsePerfSummary(ref JsonElement fav, PerfSummaryInfo summaryInfo, ILogger logger)
        {
            using (JsonDocument doc = JsonDocument.Parse(fav.GetProperty("payload").GetProperty("value").GetString()))
            {
                JsonElement root = doc.RootElement;
                try
                {
                    JsonElement memEl = root.GetProperty("mem");
                    JsonElement cpuEl = root.GetProperty("cpu");

                    summaryInfo.sysMemoryFree = memEl.GetProperty("free").GetDouble();
                    summaryInfo.sysSwapMemoryTotal = memEl.GetProperty("swapTotal").GetDouble();
                    summaryInfo.sysSwapMemoryFree = memEl.GetProperty("swapFree").GetDouble();
                    summaryInfo.sysCpu = cpuEl.GetProperty("average").GetDouble();
                    summaryInfo.glueMemory = memEl.GetProperty("glue42").GetDouble();
                    summaryInfo.glueCpu = cpuEl.GetProperty("glue42").GetDouble();
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex.Message + "\n" + ex.StackTrace);
                }
            }
        }

    }
}
