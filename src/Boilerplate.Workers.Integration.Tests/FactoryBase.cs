﻿using Boilerplate.Workers.Jobs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Boilerplate.Workers.Integration.Tests;

public class FactoryBase : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var jobsWorker = services.Single(s => s.ImplementationType == typeof(JobsWorker));
            services.Remove(jobsWorker);
        });
    }
}