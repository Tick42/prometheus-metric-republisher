Prometheus Metrics Republisher Sample App
====

## Summary
This sample application accepts a pre-defined set of metrics from the Glue42 Gateway via a REST web service and then exposes these metrics for collection (scrape) by Prometheus

## Exposed Endpoints
### /api/rest/metrics
This endpoint accepts REST POST requests for receiving metrics data from the Glue42 Gateway

### /metrics
This endpoint exposes the sample app as a Prometheus target, i.e. Prometheus can "scrape" the metrics from this endpoint via HTTP GET requests

## Sample Configuration

### Glue42 Gateway ***TODO!***
Below is a sample metric publishing configuration to put in `system.json`
This will publish/push metrics to the sample app's web service endpoint
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
                                    "TODO",
                                    "TODO",
                                    "TODO"
                                ]
                            }
                        }
                    ],
                    "non-matched": "blacklist"
                }                
            }
        }
```

## Other
###### This application uses the [prometheus-net](https://github.com/prometheus-net/prometheus-net/) client library (MIT license) as a NuGet package
