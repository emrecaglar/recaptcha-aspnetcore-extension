using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Google.ReCaptcha
{
    public class ReCaptchaAttribute : ActionFilterAttribute
    {
        public string InputName { get; set; } = "EncodedResponse";

        public InputType Input { get; set; } = InputType.Header;

        public ResultType Result { get; set; } = ResultType.Validate;

        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var encodedResponse = GetEncodedResponse(context);

            if (encodedResponse == null)
            {
                context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
            }

            await ValidateCaptcha(context, encodedResponse);

            await next();
        }

        private async Task ValidateCaptcha(ActionExecutingContext context, string encodedResponse)
        {
            var captchaService = (IReCaptchaService)context.HttpContext.RequestServices.GetService(typeof(IReCaptchaService));

            switch (Result)
            {
                case ResultType.Validate:
                    var result = await captchaService.ValidateAsync(encodedResponse);

                    if (!result.Success)
                    {
                        context.Result = new StatusCodeResult(StatusCodes.Status400BadRequest);
                    }
                    break;
                case ResultType.ValidateAndThrow:
                    await captchaService.ValidateAsync(encodedResponse);
                    break;
            }
        }

        private string GetEncodedResponse(ActionExecutingContext context)
        {
            switch (Input)
            {
                case InputType.Query:
                    return context.HttpContext.Request.Query[InputName];
                case InputType.Header:
                    context.HttpContext.Request.Headers.TryGetValue(InputName, out StringValues captcha);

                    return captcha;
            }

            return null;
        }
    }

    public enum InputType
    {
        Query,
        Header
    }

    public enum ResultType
    {
        Validate,
        ValidateAndThrow
    }
}
