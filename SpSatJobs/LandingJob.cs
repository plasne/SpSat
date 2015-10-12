using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.IdentityModel.Tokens;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;
using Microsoft.SharePoint.Client;
using System.Security;
using System.Configuration;

namespace SpSatJobs
{

    [DataContract(Namespace = "https://jobs.spsat.com")]
    public class LandingJob : Job
    {

        [DataMember]
        public string SiteUrl { get; set; }

#pragma warning disable 1998
        public override async Task<Result> Execute()
        {
            Result result = new Result();
            try
            {
                Console.Out.WriteLine("Generating access token...");

                // encryption key
                string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
                byte[] key_b = new byte[key_s.Length * sizeof(char)];
                System.Buffer.BlockCopy(key_s.ToCharArray(), 0, key_b, 0, key_b.Length);

                // create the token
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                ClaimsIdentity subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Uri, SiteUrl, ClaimValueTypes.String) });
                SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = subject,
                    TokenIssuerName = "SpSat",
                    Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddHours(4)),
                    SigningCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(key_b), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", "http://www.w3.org/2001/04/xmlenc#sha256")
                };
                SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

                using (ClientContext context = new ClientContext(SiteUrl))
                {

                    // authenticate
                    string usernamePassword = ConfigurationManager.ConnectionStrings["SharePoint"].ConnectionString;
                    dynamic usernamePassword_j = Newtonsoft.Json.JsonConvert.DeserializeObject(usernamePassword);
                    string username = usernamePassword_j.username;
                    string password = usernamePassword_j.password;
                    SecureString password_s = new SecureString();
                    Array.ForEach(password.ToCharArray(), password_s.AppendChar);
                    context.Credentials = new SharePointOnlineCredentials(username, password_s);

                    // write the token to the property bag
                    PropertyValues webProperties = context.Web.AllProperties;
                    webProperties["accessToken"] = tokenHandler.WriteToken(token);
                    context.Web.Update();
                    context.ExecuteQuery();

                }

                Console.Out.WriteLine("Completed generating access token.");
                result.Status = Status.Success;
            }
            catch (Exception ex)
            {
                result.Status = Status.Failure;
                result.Log.Add(ex.Message);
            }
            return result;
        }
#pragma warning restore 1998

        public LandingJob(string siteUrl)
        {
            SiteUrl = siteUrl;
        }
    }

}
