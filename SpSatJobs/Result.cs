using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SpSatJobs
{

    [DataContract(Namespace = "https://jobs.spsat.com")]
    public enum Status
    {
        [EnumMember]
        Unknown,
        [EnumMember]
        Success,
        [EnumMember]
        Failure
    }

    [DataContract(Namespace = "https://jobs.spsat.com")]
    public class Result
    {

        [DataMember]
        public Status Status { get; set; }

        [DataMember]
        public bool SaveLog { get; set; }

        [DataMember]
        public List<string> Log { get; set; }

        public Result()
        {
            Status = Status.Unknown;
            SaveLog = true;
            Log = new List<string>();
        }
    }


}
