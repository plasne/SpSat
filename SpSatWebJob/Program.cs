using System;
using Microsoft.Azure.WebJobs;
using System.Configuration;

namespace SpSatWebJob
{
    class Program
    {
        static void Main()
        {
            string connstring = ConfigurationManager.ConnectionStrings["Storage"].ConnectionString;
            var config = new JobHostConfiguration
            {
                DashboardConnectionString = connstring,
                StorageConnectionString = connstring
            };
            config.Queues.BatchSize = 1;
            config.Queues.MaxDequeueCount = 1; // before it goes to poison
            config.Queues.MaxPollingInterval = TimeSpan.FromMinutes(1);
            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
