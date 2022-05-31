using Microsoft.Extensions.Logging;
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

        private ILogger Logger { get; set; }

        public ReCaptchaService(IHttpClientFactory httpClientFactory, ReCaptchaOptions options, ILoggerFactory loggerFactory)
        {
            _httpClientFactory = httpClientFactory;
            _options = options;

            Logger = loggerFactory?.CreateLogger(this.GetType());
        }


        public async Task<ValidatationResponse> ValidateAsync(string captcha)
        {
            Logger?.LogDebug("ValidateAsync...");

            if (GoogleReCaptcha.ResponseCodeForTest != null)
            {
                Logger?.LogWarning("Validate captcha for debugging. captcha code: {@code}", GoogleReCaptcha.ResponseCodeForTest);

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
                Logger?.LogDebug("Validating captcha via google... captcha code: {@code}", captcha);

                var httpResponse = await http.GetAsync($"?secret={_options.Secret}&response={captcha}");

                Logger?.LogDebug("captcha validation response status code: {@statusCode}", httpResponse.StatusCode);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return new ValidatationResponse
                    {
                        Success = false,
                        ErrorCodes = new List<string> { httpResponse.StatusCode.ToString() }
                    };
                }

                var content = await httpResponse.Content.ReadAsStringAsync();

                Logger?.LogDebug("captcha validation response content {@content}", content);

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
                Logger?.LogWarning("invalid captcha. throwing recaptcha exception.");

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
