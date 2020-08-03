using System.Linq;

namespace PromRepublisher.MetricsCommon
{
    public class CommonLabelsInfo
    {
        public string AppName { get; set; }
        public string UserName { get; set; }
        public string Pid { get; set; }

        public CommonLabelsInfo() { }
        public CommonLabelsInfo(CommonLabelsInfo src)
        {
            AppName = src.AppName;
            UserName = src.UserName;
            Pid = src.Pid;
        }
        public CommonLabelsInfo Clone()
        {
            return new CommonLabelsInfo(this);
        }

        public string[] ConcatLabels(params string[] additional_labels)
        {
            return ((string[])this).Concat(additional_labels).ToArray();
        }

        public static implicit operator string[](CommonLabelsInfo labels)
        {
            return new string[] {
                labels.AppName,
                labels.UserName,
                labels.Pid
            };
        }
    }
}
