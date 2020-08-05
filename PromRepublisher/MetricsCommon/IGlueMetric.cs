using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PromRepublisher.MetricsCommon
{
    public interface IGlueMetric
    {
        // public static readonly string[] CommonLabels = { "app", "user", "proc" };

        public static readonly CommonLabelsInfo CommonLabels = new CommonLabelsInfo { AppName = "app", AppInstance = "appInstance", UserName = "user", Pid = "pid" };
        public PromMetricDef[] PromMetricDefs { get; }
        public string GlueMetricPropName { get; }
        public IPromMetric[] LinkedPromMetrics { get; set; }
        public IMetricRegistry Registry { get; set; }
        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, CommonLabelsInfo commonLabels, ILogger logger);
        public static CommonLabelsInfo GetCommonLabelValues(ref JsonElement jel)
        {
            var result = new CommonLabelsInfo();
            if (jel.TryGetProperty("identity", out JsonElement identityEl))
            {

                result.AppName = identityEl.TryGetProperty("application", out JsonElement appNameEl) ? appNameEl.ToString() : "";
                result.UserName = identityEl.TryGetProperty("user", out JsonElement userEl) ? userEl.ToString() : "";
                result.Pid = identityEl.TryGetProperty("process", out JsonElement processEl) ? processEl.ToString() : "";
                result.AppInstance = identityEl.TryGetProperty("instance", out JsonElement instanceEl) ? instanceEl.ToString() : "";
            }
            else
            {
                result.AppName = "";
                result.UserName = "";
                result.Pid = "";
            }
            return result;
        }
        
    }
}
