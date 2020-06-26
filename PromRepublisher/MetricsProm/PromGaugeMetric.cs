using Prometheus;

namespace PromRepublisher.MetricsCommon
{
    public class PromGaugeMetric : IPromMetric
    {
        private Gauge gauge_;
        public PromMetricType MetricType { get; }
        public string Name { get; }
        public string[] Labels { get; }
        public int LabelCount { get; }
        public bool UnpublishOnCollect { get; set; }

        public PromGaugeMetric(string name, string help, string[] labels)
        {
            RegisterMetric(name, help, labels);
            MetricType = PromMetricType.Counter;
            Name = name;
            Labels = labels;
            LabelCount = labels.Length;
        }
        public void RegisterMetric(string name, string help, string[] labels)
        {
            gauge_ = Metrics.CreateGauge(name, help,
                new GaugeConfiguration()
                {
                    LabelNames = labels,
                    SuppressInitialValue = true,
                }
            );
        }
        public void Unpublish()
        {
            gauge_.Unpublish();
            foreach( var labelSet in gauge_.GetAllLabelValues())
            {
                gauge_.WithLabels(labelSet).Unpublish();
            }
        }

        public void Inc(double amount, string[] labels)
        {
            gauge_.WithLabels(labels).Inc(amount);
        }
        public void Set(double value, string[] labels)
        {
            gauge_.WithLabels(labels).Set(value);
        }
    }
}
