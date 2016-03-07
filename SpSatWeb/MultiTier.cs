using SpSatJobs;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web.Http;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Data.SqlClient;
using System.Data;
using System.Net.Http;
using System.Net;
using System.Text.RegularExpressions;

namespace SpSatWeb
{
    public class MultiTier : ApiController
    {

        [Authorize]
        [HttpGet, Route("multitier/logs")]
        public List<SpSatJobs.Log<string>> GetLogs()
        {
            return SpSatJobs.Log<string>.Load(DateTime.Now.AddHours(-12));
        }

        [Authorize]
        [HttpGet, Route("multitier/pairs")]
        public List<KeyValuePair<string, string>> GetPairs()
        {
            try
            {
                List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

                ReliableSQL sql = new ReliableSQL();
                sql.Connection.ExecuteAction(() =>
                {
                    using (ReliableSqlConnection connection = new ReliableSqlConnection("Data Source=plasne-sql01.database.windows.net; Initial Catalog=plasne-db01; Authentication=Active Directory Integrated;"))
                    {
                        connection.Open();
                        using (SqlCommand cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT [key], [value] FROM [pairs]";

                            sql.Command.ExecuteAction(() =>
                            {
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        pairs.Add(new KeyValuePair<string, string>((string)reader[0], (string)reader[1]));
                                    }
                                }
                            });
                        }
                    }
                });

                return pairs;
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError) { ReasonPhrase = Regex.Replace(ex.Message, @"\t|\n|\r", "") });
            }
        }

    }

}
