using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Google.ReCaptcha
{
    internal interface IReCaptchaService
    {
        Task<ValidatationResponse> ValidateAsync(string captcha);

        Task<ValidatationResponse> ValidateAndThrowAsync(string captcha);

        CaptchaAttributeDefaults GetDefaults();
    }

    internal class ReCaptchaService : IReCaptchaService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ReCaptchaOptions _options;

        public ReCaptchaService(IHttpClientFactory httpClientFactory, ReCaptchaOptions options)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;
        }


        public async Task<ValidatationResponse> ValidateAsync(string captcha)
        {
            if (GoogleReCaptcha.ResponseCodeForTest != null)
            {
                return new ValidatationResponse
                {
                    ChallengeTs = DateTime.Now,
                    ErrorCodes = captcha == GoogleReCaptcha.ResponseCodeForTest
                                    ? new List<string>()
                                    : new List<string> { "invalid response code" },
                    Hostname = "",
                    Success = captcha == GoogleReCaptcha.ResponseCodeForTest
                };
            }

            using (var http = _httpClientFactory.CreateClient(GoogleHttpClientName.Name))
            {
                var httpResponse = await http.GetAsync($"?secret={_options.Secret}&response={captcha}");

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ValidatationResponse
                    {
                        Success = false,
                        ErrorCodes = new List<string> { httpResponse.StatusCode.ToString() }
                    };
                }

                var content = await httpResponse.Content.ReadAsStringAsync();

#if NETCOREAPP2_1
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ValidatationResponse>(content);
#elif NETCOREAPP3_1_OR_GREATER
                return System.Text.Json.JsonSerializer.Deserialize<ValidatationResponse>(content);
#endif
            }
        }

        public async Task<ValidatationResponse> ValidateAndThrowAsync(string captcha)
        {
            var response = await ValidateAsync(captcha);

            if (!response.Success)
            {
                throw new ReCaptchaException("InvalidCaptcha", string.Join("-", response.ErrorCodes));
            }

            return response;
        }


        public CaptchaAttributeDefaults GetDefaults()
        {
            return _options.Defaults ?? new CaptchaAttributeDefaults();
        }
    }
}
