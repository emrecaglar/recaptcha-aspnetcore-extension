# Google ReCaptcha Extension for AspNetCore

`Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddReCaptcha(opt =>
    {
        opt.Endpoint = "https://www.google.com/recaptcha/api/siteverify";
        opt.Secret = "your_secret";
    });
}
```

OR

`Startup.cs`

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddReCaptcha();
}
```

`appsettings.json`


```json
"ReCaptcha":{
    "Endpoint":"https://www.google.com/recaptcha/api/siteverify",
    "Secret":"your_secret"
}
```


### For use recaptcha validation, add `[ReCaptcha]` attribute

```csharp
public class HomeController: ControllerBase
{
        [HttpPost]
        [ReCaptcha]
        public async Task<IActionResult> ForgotPassword()
        {
            return Ok();
        }
}
```

## Example request

```html
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <script src="https://www.google.com/recaptcha/api.js" async defer></script>
    <script src="https://code.jquery.com/jquery-3.4.1.min.js" ></script>

    <script type="text/javascript">
        $(function(){
            $('button').click(function(){
                $.ajax({
                    url:"your_api_endpoint",
                    type: 'post',
                    headers: {
                        "encodedResponse": grecaptcha.getResponse()
                    }
                });

            });
        });
    </script>

</head>
<body>
    <form action="#" method="POST">
        <div class="g-recaptcha" data-sitekey="your_public_key"></div>
        <button type="button">POST</button>
      </form>
</body>
</html>
```