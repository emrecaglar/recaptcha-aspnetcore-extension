using System;
using System.Collections.Generic;
using System.Text;

namespace Google.ReCaptcha
{
    public class ReCaptchaOptions
    {
        public ReCaptchaOptions()
        {
            Defaults = new CaptchaAttributeDefaults();
        }

        public string Secret { get; set; }

        [Obsolete("Fixed url is now used")]
        public string Endpoint { get; set; }

        public CaptchaAttributeDefaults Defaults { get; set; }
    }

    public class CaptchaAttributeDefaults
    {
        public string InputName { get; set; } = "EncodedResponse";

        public InputType Input { get; set; } = InputType.Header;

        public ResultType Result { get; set; } = ResultType.Validate;
    }
}
