using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Google.ReCaptcha
{
    public class ValidatationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }

        [JsonProperty("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
}
