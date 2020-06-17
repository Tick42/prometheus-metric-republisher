using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Prometheus;

namespace PromRepublisher
{
    public class MetricsHandler
    {
        private readonly Counter clickStreamCounter_;
        const string clickStreamCounterName = "glue42_clickstream_events";
        const string clickStreamPageProp = "/ClickStream/Page";
        const string clickStreamLastBrowserEventProp = "/ClickStream/LastBrowserEvent";

        private readonly Counter unknownReqCounter_;
        const string unknownReqCounterName = "glue42_unrecognized_posts";

        enum LabelNames
        {
            appName,
            user,
            evType,
            reason
        }

        enum Reasons
        {
            NoMetrics,
            EmptyMetric,
            UnknownMetric,
        }

        const string identityProp = "identity";
        const string appNameProp = "applicationName";
        const string userProp = "user";
        const string metricsProp = "metrics";

        class CommonMetadata
        {
            public string appName;
            public string user;
        }

        public MetricsHandler()
        {
            clickStreamCounter_ = Metrics.CreateCounter(clickStreamCounterName, "Counts Glue42 ClickStream events",
                new CounterConfiguration()
                {
                    LabelNames = new[]
                    {
                        LabelNames.appName.ToString(),
                        LabelNames.user.ToString(),
                        LabelNames.evType.ToString()
                    },
                    SuppressInitialValue = true
                }
            );

            unknownReqCounter_ = Metrics.CreateCounter(unknownReqCounterName, "Counts unrecognized POST requests",
                new CounterConfiguration()
                {
                    LabelNames = new[]
                    {
                        LabelNames.appName.ToString(),
                        LabelNames.user.ToString(),
                        LabelNames.reason.ToString()
                    },
                    SuppressInitialValue = true
                }
            );
        }

        public void OnMetric(ref JsonElement jel)
        {
            // extract common metadata
            CommonMetadata cmData = new CommonMetadata();
            if (jel.TryGetProperty(identityProp, out JsonElement identityEl))
            {
                if (identityEl.TryGetProperty(appNameProp, out JsonElement appNameEl))
                {
                    cmData.appName = appNameEl.ToString();
                }
                if (identityEl.TryGetProperty(userProp, out JsonElement userEl))
                {
                    cmData.user = userEl.ToString();
                }
            }

            // find the "metrics" object
            if (!jel.TryGetProperty(metricsProp, out JsonElement metricsEl))
            {
                HandleUnrecognized(cmData,Reasons.NoMetrics.ToString());
                return;
            }
            if (metricsEl.ValueKind == JsonValueKind.Null)
            {
                HandleUnrecognized(cmData, Reasons.EmptyMetric.ToString());
                return;
            }


            if (TryHandleClickStream(ref metricsEl, cmData))
            {
                return;
            }
   
            // no known metric matched
            HandleUnrecognized(cmData, Reasons.UnknownMetric.ToString());
            return;
        }
        private void HandleUnrecognized(CommonMetadata cmData, string reason)
        {
            unknownReqCounter_.WithLabels(
                    cmData.appName?.ToString() ?? "",
                    cmData.user?.ToString() ?? "",
                    reason)
                .Inc();
        }
        private bool TryHandleClickStream(ref JsonElement metricsEl, CommonMetadata cmData)
        {
            if (metricsEl.TryGetProperty(clickStreamPageProp, out _))
            {
                clickStreamCounter_.WithLabels(
                        cmData.appName?.ToString() ?? "",
                        cmData.user?.ToString() ?? "",
                        clickStreamPageProp)
                    .Inc();
                return true;
            }

            if (metricsEl.TryGetProperty(clickStreamLastBrowserEventProp, out _))
            {
                clickStreamCounter_.WithLabels(
                        cmData.appName?.ToString() ?? "",
                        cmData.user?.ToString() ?? "",
                        clickStreamLastBrowserEventProp)
                    .Inc();
                return true;
            }

            return false;
        }
    }
}
