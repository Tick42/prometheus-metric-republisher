using Prometheus;

namespace PromRepublisher.MetricsCommon
{
    public class PromCounterMetric : IPromMetric
    {
        private Counter counter_;
        public PromMetricType MetricType { get; }
        public string Name { get; }
        public string[] Labels { get; }
        public int LabelCount { get; }
        public bool UnpublishOnCollect { get; set; }

        public PromCounterMetric(string name, string help, string[] labels)
        {
            RegisterMetric(name, help, labels);
            MetricType = PromMetricType.Counter;
            Name = name;
            Labels = labels;
            LabelCount = labels.Length;
        }
        public void RegisterMetric(string name, string help, string[] labels)
        {
            counter_ = Metrics.CreateCounter(name, help,
                new CounterConfiguration()
                {
                    LabelNames = labels,
                    SuppressInitialValue = true,
                }
            );
        }
        public void Unpublish()
        {
            counter_.Unpublish();
            foreach (var labelSet in counter_.GetAllLabelValues())
            {
                counter_.WithLabels(labelSet).Unpublish();
            }
        }

        public void Inc(double amount, string[] labels)
        {
            counter_.WithLabels(labels).Inc(amount);
        }

        public void Set(double value, string[] labels)
        {
            counter_.WithLabels(labels).IncTo(value);
        }

    }
}
