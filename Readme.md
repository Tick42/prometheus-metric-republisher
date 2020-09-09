Prometheus Metrics Republisher Sample App
====

## Summary
This sample application accepts a pre-defined set of metrics from the Glue42 Gateway via a REST web service and then exposes these metrics for collection (scrape) by Prometheus

## Exposed Endpoints
### /api/rest/metrics
This endpoint accepts REST POST requests for receiving metrics data from the Glue42 Gateway

### /metrics
This endpoint exposes the sample app as a Prometheus target, i.e. Prometheus can "scrape" the metrics from this endpoint via HTTP GET requests

## Metrics Handled
The Republisher app handles the following Glue metrics:

### /App/performance/memory
This metric will be transformed into the following Prometheus metrics:

|Name|Type|Description|
|----|:--:|-----------|
| glue_js_heapsize_total | Gauge | Total Javascript Heap Size |
| glue_js_heapsize_used | Gauge | Used Javascript Heap Size |


### /App/performance/entries
This metric will be transformed into the following Prometheus metrics:

|Name|Type|Description|
|----|:--:|-----------|
| glue_web_resource_count | Counter | Number reported `resource` entries |
| glue_web_resource_duration_total | Counter | Sum of `duration`'s of all `resource` entries |
| glue_web_resource_size_total | Counter | Sum of `transferSize` of all `resource` entries |
| glue_web_navigation_count | Counter | Number of reported `navigation` entries |
| glue_web_navigation_duration_total | Counter | Total duration of `navigation` entries.<br>Duration is measured as the difference `loadEventEnd` - `unloadEventStart` |
| glue_web_navigation_render_duration_total | Counter | Total render duration of `navigation` entries.<br>Duration is measured as the difference `domComplete` - `responseEnd` |

### /App/reporting/features
The `PerfSummary` and `PerfApp` datapoints of this metric will be transformed into the following Prometheus metrics:

|Name|Type|Description|
|----|:--:|-----------|
| glue_app_process_cpu_percent | Gauge | Application process CPU usage |
| glue_app_process_memory_kb | Gauge | Application process memory in kB |
| glue_cpu_percent | Gauge | Glue CPU usage |
| glue_memory_gb | Gauge | Memory used by Glue in GB |
| glue_sys_cpu_percent | Gauge | Total CPU usage |
| glue_sys_memory_free_gb | Gauge | Free system memory in GB |
| glue_sys_swap_memory_total_gb | Gauge | Total system swap memory in GB |
| glue_sys_swap_memory_free_gb | Gauge | Free system swap memory in GB |

CPU usage is the average for the period between two measurements.


## Sample Configuration

### Glue42 Windows
Below is a sample window configuration to put in `system.json`
This will cause all windows running web applications to send performance metrics (`/App/performance/...`) to the Glue42 gatewaqy for publishing.

```json
    "windows": {
        "pagePerformanceMetric": {
            "enabled": true,
            "publishInterval": 60000,
            "initialPublishTimeout": 20000
        }
    }
```
- `enabled` - a boolean value controlling whether performance metrics are to be published or not.
- `publishInterval` - specifies how often performance metrics are to be sent to the Glue42 Gateway. The value is in milliseconds.
- `initialPublishTimeout` - when an application is started, it will wait the specified number of milliseconds before starting to send performance metrics to the Glue42 Gateway at regular intervals.

### Glue42 Applications
Memory and CPU usage is automatically reported every 20 minutes or, whenever a Glue application is started or stopped.  
The default reporting interval can be changed by adding the following configuration in `system.json`:  
```json
    "perfCollector": {
        "intervalSec": 60,
        "eventDelaySec": 20,
    }
```
- `intervalSec` - specifies how often application memory and CPU metrics will be collected and reported. The value is in seconds.
- `eventDelaySec` - initial delay in seconds. This makes sure that Glue does not spend sytem resources to collect performance data during startup.

### Glue42 Gateway
Below is a sample metric publishing configuration to put in `system.json`
This will publish (push) `/App/performance/...` and `/App/reporting/features` metrics to the sample app's web service endpoint
```json
    "gw": {
        "configuration": {
            "metrics": {
                "publishers": ["rest"],
                "rest": {
                    "endpoint": "http://localhost:9942/api/rest/metrics",
                },
                "filters": {
                    "publishers": [
                        {
                            "publisher": {},
                            "metrics": {
                                "whitelist": [
                                    "#/App/performance/.*",
                                    "/App/reporting/features",
                                ]
                            }
                        }
                    ],
                    "non-matched": "blacklist"
                }                
            }
        }
    }
```

### Prometheus
Below is a sample Prometheus scrape configuration for collecting data from the PromRepublisher
```yaml
scrape_configs:
  - job_name: 'glue42'
    honor_labels: true
    scrape_interval: 60s
    static_configs:
    - targets: ['localhost:9942']
```

Please note that `scrape_interval` should not be shorter than the `publishInterval` in the Glue configuration.

## Other
* The sample application is intended to be started and stopped together with the Glue instance publishing data to it.
