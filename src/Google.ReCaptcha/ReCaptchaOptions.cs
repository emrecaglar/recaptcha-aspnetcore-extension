namespace Google.ReCaptcha
{
    public class ReCaptchaOptions
    {
        public ReCaptchaOptions()
        {
            Defaults = new CaptchaAttributeDefaults();
        }

        public string Secret { get; set; }

        public CaptchaAttributeDefaults Defaults { get; set; }
    }

    public class CaptchaAttributeDefaults
    {
        public string InputName { get; set; } = "EncodedResponse";

        public InputType Input { get; set; } = InputType.Header;

        public ResultType Result { get; set; } = ResultType.Validate;
    }
}
