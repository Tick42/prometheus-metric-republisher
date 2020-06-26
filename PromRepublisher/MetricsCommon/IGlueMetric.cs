using System.Text.Json;

namespace PromRepublisher.MetricsCommon
{
    public interface IGlueMetric
    {
        public static readonly string[] CommonLabels = { "app", "user", "proc" };
        public PromMetricDef[] PromMetricDefs { get; }
        public string GlueMetricPropName { get; }
        public IPromMetric[] LinkedPromMetrics { get; set; }
        public IMetricRegistry Registry { get; set; }
        public bool ParseJson(ref JsonElement metricEl, ref JsonElement rootEl, string[] commonLabels = null);
        public static string[] GetCommonLabelValues(ref JsonElement jel)
        {
            string appName = null;
            string user = null;
            string process = null;
            if (jel.TryGetProperty("identity", out JsonElement identityEl))
            {
                if (identityEl.TryGetProperty("applicationName", out JsonElement appNameEl))
                {
                    appName = appNameEl.ToString();
                }
                if (identityEl.TryGetProperty("user", out JsonElement userEl))
                {
                    user = userEl.ToString();
                }
                if (identityEl.TryGetProperty("process", out JsonElement processEl))
                {
                    process = processEl.ToString();
                }
            }
            return new string[] { appName ?? "", user ?? "", process ?? "" };
        }

        public static void CopyCommonLabelValues(string[] from, string[] to)
        {
            int nLabels = CommonLabels.Length;
            for( int i = 0; i < nLabels; ++i )
            {
                to[i] = from[i];
            }
        }
        
    }
}
