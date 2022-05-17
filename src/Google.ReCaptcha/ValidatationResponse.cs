using System;
using System.Collections.Generic;

namespace Google.ReCaptcha
{

#if NETCOREAPP2_1
    public class ValidatationResponse
    {
        [Newtonsoft.Json.JsonProperty("success")]
        public bool Success { get; set; }

        [Newtonsoft.Json.JsonProperty("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [Newtonsoft.Json.JsonProperty("hostname")]
        public string Hostname { get; set; }

        [Newtonsoft.Json.JsonProperty("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }

#elif NETCOREAPP3_1_OR_GREATER
    public class ValidatationResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("success")]
        public bool Success { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("challenge_ts")]
        public DateTime ChallengeTs { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("hostname")]
        public string Hostname { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("error-codes")]
        public List<string> ErrorCodes { get; set; }
    }
#endif
}
