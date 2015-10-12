using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace SpSatWeb
{

    public class Jobs : ApiController
    {

        [HttpGet, Route("jobs/provision")]
        public void Provision()
        {
            SpSatJobs.Log<string>.Provision();
            SpSatJobs.Log<string> log = new SpSatJobs.Log<string>("log-admin", "provision", "success");
            log.Save();
        }

        [HttpGet, Route("jobs/landing/encoded")]
        public string GetLandingJob()
        {
            SpSatJobs.LandingJob job = new SpSatJobs.LandingJob("https://pelasne.sharepoint.com/sites/dev");
            return job.ToString();
        }

        [HttpGet, Route("jobs/landing/enqueue")]
        public void EnqueueLandingJob()
        {
            SpSatJobs.LandingJob job = new SpSatJobs.LandingJob("https://pelasne.sharepoint.com/sites/dev");
            job.Enqueue();
        }

        [HttpGet, Route("jobs/landing/execute")]
        public async Task<SpSatJobs.Result> ExecuteLandingJob()
        {
            SpSatJobs.LandingJob job = new SpSatJobs.LandingJob("https://pelasne.sharepoint.com/sites/dev");
            return await job.Execute();
        }

        [HttpGet, Route("jobs/log")]
        public List<SpSatJobs.Log<string>> GetLast12Hours()
        {
            return SpSatJobs.Log<string>.Load(DateTime.Now.AddHours(-12));
        }

        //[HttpGet, Route("token")]
        //public string GetToken()
        //{

        //    string uri = "https://pelasne.sharepoint.com";

        //    // encryption key
        //    string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
        //    byte[] key_b = new byte[key_s.Length * sizeof(char)];
        //    System.Buffer.BlockCopy(key_s.ToCharArray(), 0, key_b, 0, key_b.Length);

        //    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        //    ClaimsIdentity subject = new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Uri, uri, ClaimValueTypes.String) });

        //    SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        //    {
        //        Subject = subject,
        //        TokenIssuerName = "SpSat",
        //        Lifetime = new Lifetime(DateTime.UtcNow, DateTime.UtcNow.AddHours(4)),
        //        SigningCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(key_b), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256", "http://www.w3.org/2001/04/xmlenc#sha256")
        //    };

        //    SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        //    return tokenHandler.WriteToken(token);

        //}

        //[HttpGet, Route("validate")]
        //public string Validate()
        //{

        //    string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy91cmkiOiJodHRwczovL3BlbGFzbmUuc2hhcmVwb2ludC5jb20iLCJpc3MiOiJTcFNhdCIsImV4cCI6MTQ0MjUzOTU0OCwibmJmIjoxNDQyNTI1MTQ4fQ.Lo_JhnE0cAvRca91BCPFmstp4KY0RpYcpoerdouNuro";

        //    // encryption key
        //    string key_s = ConfigurationManager.ConnectionStrings["JWTKey"].ConnectionString;
        //    byte[] key_b = new byte[key_s.Length * sizeof(char)];
        //    System.Buffer.BlockCopy(key_s.ToCharArray(), 0, key_b, 0, key_b.Length);

        //    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        //    TokenValidationParameters validationParameters = new TokenValidationParameters
        //    {
        //        IssuerSigningToken = new BinarySecretSecurityToken(key_b),
        //        ValidIssuer = "SpSat",
        //        ValidateIssuer = true,
        //        ValidateAudience = false
        //    };

        //    SecurityToken validatedToken;
        //    ClaimsPrincipal principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);
        //    return principal.Claims.Single(x => x.Type == ClaimTypes.Uri).Value;

        //}

    }

}