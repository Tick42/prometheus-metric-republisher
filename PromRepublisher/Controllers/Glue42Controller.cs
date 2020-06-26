using Microsoft.AspNetCore.Mvc;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.Controllers
{
    [Route("api/rest/metrics")]
    [ApiController]
    public class Glue42Controller : ControllerBase
    {
        private readonly MetricHandler metricHandler_;

        public Glue42Controller(MetricHandler metricHandler)
        {
            metricHandler_ = metricHandler;
        }

        // POST api/rest/metrics
        [HttpPost]
        public void Post([FromBody] System.Text.Json.JsonElement jel)
        {
            metricHandler_.OnMetric(ref jel);
        }
    }
}
