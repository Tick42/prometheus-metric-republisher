using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PromRepublisher.MetricsCommon;

namespace PromRepublisher.Controllers
{
    [Route("api/rest/metrics")]
    [ApiController]
    public class Glue42Controller : ControllerBase
    {
        private readonly MetricHandler metricHandler_;
        private readonly ILogger logger_;

        public Glue42Controller(MetricHandler metricHandler, ILogger<Glue42Controller> logger )
        {
            metricHandler_ = metricHandler;
            logger_ = logger;
        }

        // POST api/rest/metrics
        [HttpPost]
        public void Post([FromBody] System.Text.Json.JsonElement jel)
        {
            metricHandler_.OnMetric(ref jel, logger_);
        }
    }
}
