﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Si.IdCheck.Workers.Jobs;

namespace Si.IdCheck.Workers.Integration.Tests;

public class FactoryBase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var alertsWorker = services.Single(s => s.ImplementationType == typeof(OngoingMonitoringAlertsWorker));
            var jobsWorker = services.Single(s => s.ImplementationType == typeof(OngoingMonitoringAlertsWorker));
            services.Remove(jobsWorker);
            services.Remove(alertsWorker);
        });
    }
}