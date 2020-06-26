namespace PromRepublisher.MetricsCommon
{
    public enum PromMetricType
    {
        Counter,
        Gauge,
    }

    public class PromMetricDef
    {
        public PromMetricType MetricType;
        public string Name;
        public string Help;
        public string[] CommonLabels;
        public string[] SpecificLabels;
        public bool UnpublishOnCollect;
    }
    public interface IPromMetric
    {
        public PromMetricType MetricType { get; }
        public string Name { get; }
        public string[] Labels { get; }
        public int LabelCount { get; }
        public bool UnpublishOnCollect { get; set; }
        
        public void RegisterMetric(string name, string help, string[] labels);
        public void Unpublish();

        // methods for updating metric values
        public void Inc(double amount, string[] labels);
        public void Set(double value, string[] labels);
    }
}
