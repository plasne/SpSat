using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Web.Http;
using System.Xml;

namespace SpSatWeb
{

    public class Landing : ApiController
    {

        [DataContract()]
        public class Volume
        {

            [DataMember()]
            public string Category { get; set; }

            [DataMember()]
            public int Value { get; set; }

        }

        [DataContract()]
        public class VolumeResponse
        {

            [DataMember()]
            public List<string> Claims { get; set; }

            [DataMember()]
            public List<Volume> Volumes { get; set; }

            public VolumeResponse()
            {
                Claims = new List<string>();
                Volumes = new List<Volume>();
            }
        }

        [Authorize]
        [HttpGet, Route("landing/volumes")]
        public VolumeResponse GetVolumes()
        {
            VolumeResponse response = new VolumeResponse();

            // identify any claims
            ClaimsIdentity identity = User.Identity as ClaimsIdentity;
            foreach (Claim claim in identity.Claims)
            {
                response.Claims.Add("[" + claim.Type + "] " + claim.Value);
            }

            // add the data
            Random rnd = new Random();
            response.Volumes.Add(new Volume { Category = "A", Value = rnd.Next(1, 100) });
            response.Volumes.Add(new Volume { Category = "B", Value = rnd.Next(1, 100) });
            response.Volumes.Add(new Volume { Category = "C", Value = rnd.Next(1, 100) });
            response.Volumes.Add(new Volume { Category = "D", Value = rnd.Next(1, 100) });
            response.Volumes.Add(new Volume { Category = "E", Value = rnd.Next(1, 100) });

            return response;
        }

        [DataContract()]
        public class Quote
        {

            [DataMember()]
            public string Symbol { get; set; }

            [DataMember()]
            public decimal Last { get; set; }

            [DataMember()]
            public string Change { get; set; }

            [DataMember()]
            public decimal Open { get; set; }

            [DataMember()]
            public decimal High { get; set; }

            [DataMember()]
            public decimal Low { get; set; }

            [DataMember()]
            public Int64 Volume { get; set; }

        }

        [HttpGet, Route("landing/stockquote")]
        public Quote GetStockQuote()
        {

            // sometimes too slow for the demo
            //StockQuote.StockQuoteSoapClient sq = new StockQuote.StockQuoteSoapClient();
            //string raw = sq.GetQuote("MSFT");
            //XmlDocument xml = new XmlDocument();
            //xml.LoadXml(raw);
            //quote.Symbol = xml.DocumentElement.SelectSingleNode("Stock/Symbol").InnerText;
            //quote.Last = decimal.Parse(xml.DocumentElement.SelectSingleNode("Stock/Last").InnerText);
            //quote.Change = xml.DocumentElement.SelectSingleNode("Stock/Change").InnerText;
            //quote.Open = decimal.Parse(xml.DocumentElement.SelectSingleNode("Stock/Open").InnerText);
            //quote.High = decimal.Parse(xml.DocumentElement.SelectSingleNode("Stock/High").InnerText);
            //quote.Low = decimal.Parse(xml.DocumentElement.SelectSingleNode("Stock/Low").InnerText);
            //quote.Volume = Int64.Parse(xml.DocumentElement.SelectSingleNode("Stock/Volume").InnerText);

            Quote quote = new Quote
            {
                Symbol = "MSFT",
                Change = "-0.77",
                Last = 43.48M,
                Low = 43.33M,
                High = 43.99M,
                Volume = 57426153
            };

            return quote;

        }

    }

}