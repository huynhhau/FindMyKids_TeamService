using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace FindMyKids.TeamService.Models
{
    public class Recaptcha
    {
        private string m_Success;
        [JsonProperty("Success")]
        public string success
        {
            get { return m_Success; }
            set { m_Success = value; }
        }

        private List<string> m_ErrorCodes;
        [JsonProperty("error-codes")]
        public List<string> ErrorCodes
        {
            get { return m_ErrorCodes; }
            set { m_ErrorCodes = value; }
        }
        public static bool Validate(string EncodeResponse, string PrivateKey)
        {
            var client = new System.Net.WebClient();
            var reply = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}",PrivateKey,EncodeResponse));
            var captchaResponse = JsonConvert.DeserializeObject<Recaptcha>(reply);
            return Convert.ToBoolean(captchaResponse.success);
        }

    }
}
