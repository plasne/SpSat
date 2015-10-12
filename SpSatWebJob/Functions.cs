using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Xml;
using Microsoft.WindowsAzure.Scheduler.Models;

namespace SpSatWebJob
{
    public class Functions
    {

        public async static void ProcessQueueMessage([QueueTrigger("jobs")] string message, TextWriter log)
        {
            SpSatJobs.Job job = null;
            SpSatJobs.Result result = null;
            try
            {

                // attempt to decode as just a Base64 string, but if not, it was serialized as a StorageQueueMessage
                byte[] buffer = null;
                try
                {
                    buffer = Convert.FromBase64String(message);
                }
                catch
                {

                    // deserialize if provided by a scheduler
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(StorageQueueMessage));
                    using (MemoryStream stream = new MemoryStream(Encoding.Unicode.GetBytes(message)))
                    {
                        StorageQueueMessage sqm = xmlSerializer.Deserialize(stream) as StorageQueueMessage;
                        if (sqm != null) buffer = Convert.FromBase64String(sqm.Message);
                    }

                }

                // deserialize the message
                DataContractSerializer dcSerializer = new DataContractSerializer(typeof(SpSatJobs.Job));
                using (MemoryStream stream = new MemoryStream(buffer))
                {
                    using (XmlDictionaryReader reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max))
                    {
                        job = (SpSatJobs.Job)dcSerializer.ReadObject(reader, false);

                        // execute and write to the console
                        result = await job.Execute();
                        foreach (string logEntry in result.Log)
                        {
                            Console.Out.WriteLine(logEntry);
                        }

                        // log to SQL
                        if (result.SaveLog)
                        {
                            string status = "unknown";
                            switch (result.Status)
                            {
                                case SpSatJobs.Status.Success:
                                    status = "success";
                                    break;
                                case SpSatJobs.Status.Failure:
                                    status = "failure";
                                    break;
                            }
                            SpSatJobs.Log<SpSatJobs.Result> joblog = new SpSatJobs.Log<SpSatJobs.Result>(job.GetType().ToString(), status, result);
                            joblog.Save();
                        }

                    }
                }

            }
            catch (Exception ex)
            {

                // determine the complete error message
                string msg = ex.Message;
                Exception current = ex.InnerException;
                while (current != null)
                {
                    msg += ", inner: " + ((current.InnerException != null) ? current.InnerException.Message : "(none)");
                    current = current.InnerException;
                }

                // write out exception to console
                Console.Out.WriteLine("exception: " + msg);

                // try to commit to SQL
                try
                {
                    string type = (job != null) ? job.GetType().ToString() : "Job";
                    SpSatJobs.Log<string> joblog = new SpSatJobs.Log<string>(type, "failure", msg);
                    joblog.Save();
                }
                catch
                {
                    // ignore any failures to save to SQL
                }

                throw;
            }
        }

    }
}
