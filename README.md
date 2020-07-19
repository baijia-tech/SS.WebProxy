# SS.WebProxy
A HTTP(s) proxy middleware supported asp.net core 3.1+


=================

[![NuGet](https://img.shields.io/nuget/dt/Senparc.Weixin.svg)](https://www.nuget.org/packages/SS.WebProxy)
[![GitHub commit activity the past week, 4 weeks, year](https://img.shields.io/github/commit-activity/4w/baijia-tech/SS.WebProxy.svg)](https://github.com/baijia-tech/SS.WebProxy/commits/master)
[![license](https://img.shields.io/github/license/baijia-tech/SS.WebProxy.svg)](http://www.apache.org/licenses/LICENSE-2.0)

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
   app.Map("", app => { app.UseProxy(new Uri("https://zeizhi.com")); });
}

```

or 

```
public void Configure(IApplicationBuilder app)
{
    app.UseProxy("/proxy"); //  https://domain/proxy?url=https://www.baidu.com
}

```

