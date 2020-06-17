using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PromRepublisher.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MetricsController : ControllerBase
    {
        // GET /metrics
        [HttpGet("")]
        public async Task<string> GetMetrics()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                await Prometheus.Metrics.DefaultRegistry.CollectAndExportAsTextAsync(ms);
                ms.Position = 0;
                using (StreamReader sr = new StreamReader(ms))
                {
                    return await sr.ReadToEndAsync(); ;
                }
            }
        }
    }
}
