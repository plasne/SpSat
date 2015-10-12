using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace SpSatJobs
{

    [DataContract]
    public class Log<T>
    {

        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public string Scope { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public T Message { get; set; }

        public void Save(List<Type> knownTypes = null)
        {
            ReliableSQL sql = new ReliableSQL();
            sql.Connection.ExecuteAction(() =>
            {
                using (ReliableSqlConnection connection = new ReliableSqlConnection(sql.CoreConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "INSERT INTO [Diagnostics_Logs] ([timestamp], [scope], [type], [message]) VALUES (@timestamp, @scope, @type, @message)";
                        cmd.Parameters.Add("timestamp", SqlDbType.DateTime).Value = Timestamp;
                        cmd.Parameters.Add("scope", SqlDbType.VarChar, 50).Value = Scope;
                        cmd.Parameters.Add("type", SqlDbType.VarChar, 50).Value = Type;
                        if (typeof(T) == typeof(string))
                        {
                            cmd.Parameters.Add("message", SqlDbType.NText).Value = Message as string;
                        }
                        else
                        {
                            DataContractSerializer serializer = (knownTypes != null) ? new DataContractSerializer(typeof(T), knownTypes) : new DataContractSerializer(typeof(T));
                            XmlWriterSettings settings = new XmlWriterSettings
                            {
                                Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
                                Indent = true,
                                OmitXmlDeclaration = false
                            };
                            using (StringWriter textWriter = new StringWriter())
                            {
                                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter, settings))
                                {
                                    serializer.WriteObject(xmlWriter, Message);
                                }
                                cmd.Parameters.Add("message", SqlDbType.NText).Value = textWriter.ToString();
                            }
                        }
                        sql.Command.ExecuteAction(() =>
                        {
                            cmd.ExecuteNonQuery();
                        });
                    }
                }
            });
        }

        public static List<Log<T>> Load(DateTime since, string scope = null, string type = null, List<Type> knownTypes = null)
        {
            List<Log<T>> logs = new List<Log<T>>();
            ReliableSQL sql = new ReliableSQL();
            sql.Connection.ExecuteAction(() =>
            {
                using (ReliableSqlConnection connection = new ReliableSqlConnection(sql.CoreConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "SELECT [timestamp], [scope], [type], [message] FROM [Diagnostics_Logs] WHERE [timestamp] >= @since ORDER BY [timestamp] DESC";
                        cmd.Parameters.Add("since", SqlDbType.DateTime).Value = since;
                        if (!string.IsNullOrWhiteSpace(scope))
                        {
                            cmd.CommandText += " AND scope = @scope";
                            cmd.Parameters.Add("scope", SqlDbType.VarChar, 50).Value = scope;
                        }
                        if (!string.IsNullOrWhiteSpace(type))
                        {
                            cmd.CommandText += " AND type = @type";
                            cmd.Parameters.Add("type", SqlDbType.VarChar, 50).Value = type;
                        }
                        sql.Command.ExecuteAction(() =>
                        {
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    Log<T> log = new Log<T>((string)reader[1], (string)reader[2]);
                                    log.Timestamp = (DateTime)reader[0];
                                    if (typeof(T) == typeof(string))
                                    {
                                        log.Message = (T)reader[3];
                                    }
                                    else
                                    {
                                        DataContractSerializer serializer = (knownTypes != null) ? new DataContractSerializer(typeof(T), knownTypes) : new DataContractSerializer(typeof(T));
                                        string message = (string)reader[3];
                                        using (StringReader textReader = new StringReader(message))
                                        {
                                            using (XmlReader xmlReader = XmlReader.Create(textReader))
                                            {
                                                log.Message = (T)serializer.ReadObject(xmlReader);
                                            }
                                        }
                                    }
                                    logs.Add(log);
                                }
                            }
                        });
                    }
                }
            });
            return logs;
        }

        public static void Trim()
        {
            ReliableSQL sql = new ReliableSQL();
            sql.Connection.ExecuteAction(() =>
            {
                using (ReliableSqlConnection connection = new ReliableSqlConnection(sql.CoreConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "DELETE FROM [dbo].[Diagnostics_Logs] WHERE [timestamp] < DATEADD(day, -15, GetDate());";
                        sql.Command.ExecuteAction(() =>
                        {
                            cmd.ExecuteNonQuery();
                        });
                    }
                }
            });
        }




        public static void Provision()
        {
            ReliableSQL sql = new ReliableSQL();
            sql.Connection.ExecuteAction(() =>
            {
                using (ReliableSqlConnection connection = new ReliableSqlConnection(sql.CoreConnectionString))
                {
                    connection.Open();
                    using (SqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "CREATE TABLE [dbo].[Diagnostics_Logs] ([id] [int] IDENTITY(1,1) NOT NULL, [timestamp] [datetime] NOT NULL, " +
                            "[scope] [varchar](50) NOT NULL, [type] [varchar](50) NOT NULL, [message] [nvarchar](MAX) NOT NULL); " +
                            "CREATE UNIQUE CLUSTERED INDEX [Idx_Diagnostics_Logs] ON [Diagnostics_Logs](id); " +
                            "CREATE INDEX [Idx_Diagnostics_Logs_Timestamp] ON [Diagnostics_Logs] (timestamp); " +
                            "CREATE INDEX [Idx_Diagnostics_Logs_Recent] ON [Diagnostics_Logs] (timestamp) INCLUDE ([scope], [type], [message]);";
                        sql.Command.ExecuteAction(() =>
                        {
                            cmd.ExecuteNonQuery();
                        });
                    }
                }
            });
        }

        public Log(string scope, string type, T message)
        {
            this.Timestamp = DateTime.Now;
            this.Scope = scope;
            this.Type = type;
            this.Message = message;
        }

        public Log(string scope, string type)
        {
            this.Timestamp = DateTime.Now;
            this.Scope = scope;
            this.Type = type;
        }


    }


}
