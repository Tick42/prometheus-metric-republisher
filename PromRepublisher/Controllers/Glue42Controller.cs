using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PromRepublisher.Controllers
{
    [Route("api/rest/metrics")]
    [ApiController]
    public class Glue42Controller : ControllerBase
    {
        private readonly MetricsHandler metricsHandler_;

        public Glue42Controller(MetricsHandler metricsHandler)
        {
            metricsHandler_ = metricsHandler;
        }

        // POST api/rest/metrics
        [HttpPost]
        public void Post([FromBody] System.Text.Json.JsonElement jel)
        {
            metricsHandler_.OnMetric(ref jel);
        }
    }
}
