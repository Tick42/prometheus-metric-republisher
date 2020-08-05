let customMetricSet01 = {myCounter:0, myGauge:42};
let serviceMetricsSystem;
let glueCustomMetricSet01;

InitGlue();
UpdateMetricsView();

async function InitGlue()
{
	"use strict";
	try
	{
		var connStatus = document.getElementById("connStatus");
		connStatus.innerText = "Connecting...";
		connStatus.style.backgroundColor = "#D0D0D0";
		console.log("Connecting to Glue...");
		var glue = window.glue = await Glue({});
		connStatus.innerText = "Connected";
		connStatus.style.backgroundColor = "#D0FFD0";
		console.log("Connected to Glue");
		InitGlueMetrics();
	}
	catch(err)
	{
		console.log("Connection to Glue failed.")
		console.log(err.message);
		connStatus.style.backgroundColor = "#FF0000";
	}
}

function InitGlueMetrics()
{
	"use strict";
	// initialize the metric subsystem
	serviceMetricsSystem = glue.metrics.subSystem('customSystem', 'Custom Metric System');

	// initialize the custom metrics
	glueCustomMetricSet01 = serviceMetricsSystem.objectMetric({name:"customMetricSet01",description:"Sample Set #01"}, customMetricSet01);
	setInterval(SendMetricUpdateToGlue, 10000); // send updates to Glue every 10 seconds
}

function SendMetricUpdateToGlue()
{
	glueCustomMetricSet01.update(customMetricSet01);
}

function UpdateMetricsView()
{
	"use strict";
	var el;
	el = document.getElementById("elCustomCounter");
	el.innerText = customMetricSet01.myCounter;

	el = document.getElementById("elCustomGauge");
	el.innerText = customMetricSet01.myGauge;
}

function BtnCustomCounter_Click(e)
{
	"use strict";
	customMetricSet01.myCounter += 1;
	SendMetricUpdateToGlue();
	UpdateMetricsView();
}

function BtnCustomGauge_Click(e)
{
	"use strict";
	customMetricSet01.myGauge += Math.floor((Math.random() * 101 - customMetricSet01.myGauge) / 3);
	SendMetricUpdateToGlue();
	UpdateMetricsView();
}
