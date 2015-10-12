using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SpSatJobs
{

    [DataContract(Namespace = "https://jobs.spsat.com")]
    [KnownType(typeof(SpSatJobs.LandingJob))]
    public abstract class Job
    {

        public abstract Task<Result> Execute();

        public void Enqueue()
        {

            // connect to or create the queue
            CloudStorageAccount account = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=pelasnec0;AccountKey=OaT8WpZ1wbsgk8DIb4CK8YSZuMkOp/BJgByCDEEsDT8OIbibTiZQTdrltiMBbBM2pG1kkwyNggT7zVQ7Sxl5ig==");
            CloudQueueClient client = account.CreateCloudQueueClient();
            CloudQueue queue = client.GetQueueReference("jobs");
            queue.CreateIfNotExists();

            // enqueue
            CloudQueueMessage message = new CloudQueueMessage(ToString());
            queue.AddMessage(message);

        }

        public override string ToString()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(Job));
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlDictionaryWriter writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
                {
                    serializer.WriteObject(writer, this);
                }
                byte[] buffer = stream.ToArray();
                return Convert.ToBase64String(buffer);
            }
        }

    }

}
