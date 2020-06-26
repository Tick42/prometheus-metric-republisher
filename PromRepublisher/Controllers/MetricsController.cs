using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        private readonly IMetricRegistry metricRegistry_;

        public MetricsController(IMetricRegistry metricRegistry)
        {
            metricRegistry_ = metricRegistry;
        }

        // GET /metrics
        [HttpGet("")]
        public async Task<string> GetMetrics()
        {
            return await metricRegistry_.CollectAndSerializeAsync();
        }
    }
}
