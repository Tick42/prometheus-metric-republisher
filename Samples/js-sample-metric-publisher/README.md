# Custom Metrics Handling Sample

## Javascript app
The sample javascript app registers a custom metric with two values.

The essential part of the sample is the metric registration:
```js
	// initialize the metric subsystem
	serviceMetricsSystem = glue.metrics.subSystem('customSystem', 'Custom Metric System');

	// initialize the custom metrics
	glueCustomMetricSet01 = serviceMetricsSystem.objectMetric({name:"customMetricSet01",description:"Sample Set #01"});
```
This will define a metric called `/App/customSystem/customMetricSet01` with Glue.

## Glue gateway configuration in `system.json`
The Glue gateway needs to be configured to publish the custom metric.  
Here is a sample gateway metric publishing configuration, note the line where the metric `/App/customSystem/customMetricSet01` is whitelisted, i.e. enabled for publishing:  
```json
    "gw": {
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
                                    "/App/customSystem/customMetricSet01"
                                ]
                            }
                        }
                    ],
                    "non-matched": "blacklist"
                }                
            }
        }
```


## PromRepublisher: add the custome metric transformation class
The sample includes the source file `CustomMetricSet01.cs` which implements the transformation of the two metric values to Prometheus metrics, named "glue_custom_my_counter" and "glue_custom_my_gauge".

* Add the provided source file to the `PromRepublisher` project (e.g. under "MetricsGlue").

* In `Startup.cs`, add the new metric (`CustomMetricSet01`) to the metric registry:
```csharp
        public void ConfigureServices(IServiceCollection services)
        {
            Prometheus.Metrics.SuppressDefaultMetrics();
            IMetricRegistry registry = new MetricRegistry();
            // add Glue metrics;
            registry.AddGlueMetric(new PerfMemoryMetric());
            registry.AddGlueMetric(new PerfEntriesMetric());
            registry.AddGlueMetric(new ReportingFeaturesMetric());
            registry.AddGlueMetric(new CustomMetricSet01());
            services.AddSingleton(registry);

            services.AddSingleton(new MetricHandler(registry));
            services.AddControllers();
        }
```