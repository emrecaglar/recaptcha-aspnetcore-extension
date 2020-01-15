using Newtonsoft.Json;
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

                return JsonConvert.DeserializeObject<ValidatationResponse>(content);
            }
        }

        public async Task<ValidatationResponse> ValidateAndThrowAsync(string captcha)
        {
            var response = await ValidateAsync(captcha);

            if (!response.Success)
            {
                throw new Exception(string.Join("-", response.ErrorCodes));
            }

            return response;
        }
    }
}
