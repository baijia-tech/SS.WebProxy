using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebProxy.Core
{
    public static class ProxyExtensions
    {

        public static void UseProxy(this IApplicationBuilder app, Uri baseUri)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            var options = new ProxyOptions
            {
                Scheme = baseUri.Scheme,
                Host = new HostString(baseUri.Authority),
                PathBase = baseUri.AbsolutePath,
                AppendQuery = new QueryString(baseUri.Query),
                ProxyType = ProxyType.Uri
            };
            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        public static void UseProxy(this IApplicationBuilder app, string path)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (path == null) throw new ArgumentNullException(nameof(path));

            var options = new ProxyOptions
            {
                PathBase = new PathString(path),
                ProxyType = ProxyType.Query
            };
            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }


        /// <summary>
        ///     Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        public static void UseProxy(this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));

            app.UseMiddleware<ProxyMiddleware>();
        }

        /// <summary>
        ///     Runs proxy forwarding requests to the server specified by options.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="options">Proxy options</param>
        public static void UseProxy(this IApplicationBuilder app, ProxyOptions options)
        {
            if (app == null) throw new ArgumentNullException(nameof(app));
            if (options == null) throw new ArgumentNullException(nameof(options));

            app.UseMiddleware<ProxyMiddleware>(Options.Create(options));
        }

        /// <summary>
        ///     Forwards current request to the specified destination uri.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="destinationUri">Destination Uri</param>
        public static async Task ProxyRequest(this HttpContext context, Uri destinationUri)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (destinationUri == null) throw new ArgumentNullException(nameof(destinationUri));

            if (context.WebSockets.IsWebSocketRequest)
            {
                await context.AcceptProxyWebSocketRequest(destinationUri.ToWebSocketScheme());
            }
            else
            {
                var proxyService = context.RequestServices.GetRequiredService<ProxyService>();

                using (var requestMessage = context.CreateProxyHttpRequest(destinationUri))
                {
                    var prepareRequestHandler = proxyService.Options.PrepareRequest;
                    if (prepareRequestHandler != null)
                    {
                        var res = await prepareRequestHandler(context.Request, requestMessage);
                        if (res >= 100)
                        {
                            context.Response.StatusCode = res;
                            await context.Response.WriteAsync("AccessDenied", Encoding.UTF8);
                            return;
                        }
                    }

                    using (var responseMessage = await context.SendProxyHttpRequest(requestMessage))
                    {
                        await context.CopyProxyHttpResponse(responseMessage);
                    }
                }
            }
        }
    }
}
