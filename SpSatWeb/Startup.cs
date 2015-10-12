using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Security.ActiveDirectory;
using System.IdentityModel.Tokens;
using System.ServiceModel.Security.Tokens;
using System.Configuration;

[assembly: OwinStartup(typeof(SpSatWeb.Startup))]

namespace SpSatWeb
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        public void ConfigureAuth(IAppBuilder app)
        {

            // AAD Bearer
            if (ConfigurationManager.ConnectionStrings["AADApp"] != null)
            {
                string aadapp = ConfigurationManager.ConnectionStrings["AADApp"].ConnectionString;
                dynamic aadapp_j = Newtonsoft.Json.JsonConvert.DeserializeObject(aadapp);
                string tenant = aadapp_j.tenant;
                string clientId = aadapp_j.clientId;
                app.UseWindowsAzureActiveDirectoryBearerAuthentication(
                    new WindowsAzureActiveDirectoryBearerAuthenticationOptions
                    {
                        Tenant = tenant,
                        TokenValidationParameters = new TokenValidationParameters { ValidAudience = clientId }
                    });
            }

            // JWT Bearer
            if (ConfigurationManager.ConnectionStrings["JWTKey"] != null)
            {
                string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
                byte[] key_b = new byte[key_s.Length * sizeof(char)];
                System.Buffer.BlockCopy(key_s.ToCharArray(), 0, key_b, 0, key_b.Length);
                app.UseJwtBearerAuthentication(
                    new Microsoft.Owin.Security.Jwt.JwtBearerAuthenticationOptions
                    {
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            IssuerSigningToken = new BinarySecretSecurityToken(key_b),
                            ValidIssuer = "SpSat",
                            ValidateIssuer = true,
                            ValidateAudience = false
                        }
                    });
            }

        }

    }
}
