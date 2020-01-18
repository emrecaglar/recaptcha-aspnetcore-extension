using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.ReCaptcha
{
    public class ReCaptchaAttribute : ActionFilterAttribute
    {
        public string InputName { get; set; }

        private InputType? _input;
        public InputType Input
        {
            get { return _input.GetValueOrDefault(); }
            set { _input = value; }
        }

        private ResultType? _result;
        public ResultType Result
        {
            get { return _result.GetValueOrDefault(); }
            set { _result = value; }
        }

        private void SetParameters(IReCaptchaService captchaService)
        {
            var defaults = captchaService.GetDefaults();

            InputName = InputName ?? defaults.InputName;
            Input = _input ?? defaults.Input;
            Result = _result ?? defaults.Result;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var captchaService = (IReCaptchaService)context.HttpContext.RequestServices.GetService(typeof(IReCaptchaService));

            SetParameters(captchaService);

            var encodedResponse = await GetEncodedResponse(context);

            if (encodedResponse == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);

                return;
            }

            var validationRespnse = await ValidateCaptcha(captchaService, encodedResponse);

            if (!validationRespnse.Success)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);

                return;
            }

            await next();
        }

        private async Task<ValidatationResponse> ValidateCaptcha(IReCaptchaService captchaService, string encodedResponse)
        {
            switch (Result)
            {
                case ResultType.Validate:
                    return await captchaService.ValidateAsync(encodedResponse);
                case ResultType.ValidateAndThrow:
                    return await captchaService.ValidateAndThrowAsync(encodedResponse);
            }

            return null;
        }

        private async Task<string> GetEncodedResponse(ActionExecutingContext context)
        {
            switch (Input)
            {
                case InputType.Query:
                    return context.HttpContext.Request.Query[InputName];
                case InputType.Header:
                    context.HttpContext.Request.Headers.TryGetValue(InputName, out StringValues captchaFromQueryString);

                    return captchaFromQueryString;
                case InputType.Body:
                    return await GetEncodedResponseFromBody(context);
            }

            return null;
        }

        private async Task<string> GetEncodedResponseFromBody(ActionExecutingContext context)
        {
            switch (context.HttpContext.Request.ContentType)
            {
                case "application/x-www-form-urlencoded":
                case "text/html":
                case "multipart/form-data":
                    context.HttpContext.Request.Form.TryGetValue(InputName, out StringValues formData);

                    return formData;
                case "application/json":
                    try
                    {
                        var streamReader = new StreamReader(context.HttpContext.Request.Body);

                        string json = await streamReader.ReadToEndAsync();

                        var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                        var key = obj.Keys.FirstOrDefault(x => x.Equals(InputName, StringComparison.OrdinalIgnoreCase));

                        if (key == null)
                        {
                            return null;
                        }

                        return $"{obj[key]}";
                    }
                    catch
                    {
                        return null;
                    }
                default:
                    throw new NotSupportedException($"Content-type is not supported: {context.HttpContext.Request.ContentType}");
            }
        }
    }

    public enum InputType
    {
        Query = 1,
        Header = 2,
        Body = 3,
    }

    public enum ResultType
    {
        Validate = 1,
        ValidateAndThrow = 2
    }
}
