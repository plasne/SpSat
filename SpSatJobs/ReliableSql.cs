using System;
using System.Collections.Generic;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Configuration;

namespace SpSatJobs
{

    public class ReliableSQL
    {

        public string CoreConnectionString { get; private set; }

        public RetryPolicy Connection { get; set; }

        public RetryPolicy Command { get; set; }

        public ReliableSQL(int retries = 4)
        {

            // define the connection strings
            if (ConfigurationManager.ConnectionStrings["SQL"] != null)
            {
                this.CoreConnectionString = ConfigurationManager.ConnectionStrings["SQL"].ConnectionString;
            }

            // create or re-use the existing retry manager
            RetryManager manager;
            try
            {
                manager = RetryManager.Instance;

                // re-use
                Connection = manager.GetDefaultSqlConnectionRetryPolicy();
                Command = manager.GetDefaultSqlCommandRetryPolicy();

            }
            catch (System.InvalidOperationException)
            {

                // create the strategy
                ExponentialBackoff eb = new ExponentialBackoff("exponentialBackoffStrategy", retries,
                    TimeSpan.FromMilliseconds(2000), // min delay
                    TimeSpan.FromMilliseconds(8000), // max delay
                    TimeSpan.FromMilliseconds(2000) // delta
                    );

                // build a manager
                manager = new RetryManager(new List<RetryStrategy>() { eb }, "exponentialBackoffStrategy");
                RetryManager.SetDefault(manager);

                // create the policies
                Connection = manager.GetDefaultSqlConnectionRetryPolicy();
                Command = manager.GetDefaultSqlCommandRetryPolicy();

            }

        }

    }

}
