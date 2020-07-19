# SS.WebProxy
A HTTP(s) proxy middleware supported asp.net core 3.1+


## Usage

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddProxy(options =>
    {
        options.MessageHandler = new HttpClientHandler
        {
            AllowAutoRedirect = false
        };
        
        options.PrepareRequest = (originalRequest, message) =>
        {
            message.Headers.Add("X-Forwarded-Host", originalRequest.Host.Host);
            return Task.FromResult(0);
        };
    });
}

```

```
public void Configure(IApplicationBuilder app)
{
    app.Map("/zeizhi", app => { app.UseProxy(new Uri("https://zeizhi.com"); });
}

```

or 

```
public void Configure(IApplicationBuilder app)
{
    app.UseProxy("/proxy"); //  https://domain/proxy?url=https://www.baidu.com
}

```

