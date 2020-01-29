using System;

namespace Google.ReCaptcha
{
    public class ReCaptchaException : Exception
    {
        public ReCaptchaException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}