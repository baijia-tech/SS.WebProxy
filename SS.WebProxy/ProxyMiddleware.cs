using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Options;

namespace WebProxy.Core
{
    /// <summary>
    /// Proxy Middleware
    /// </summary>
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ProxyOptions _options;

        public ProxyMiddleware(RequestDelegate next, IOptions<ProxyOptions> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _options = options.Value;
        }

        public Task Invoke(HttpContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Uri uri = null;
            switch (_options.ProxyType)
            {
                case ProxyType.Uri:
                    uri = new Uri(UriHelper.BuildAbsolute(_options.Scheme, _options.Host, _options.PathBase,
                        context.Request.Path,
                        context.Request.QueryString.Add(_options.AppendQuery)));
                    break;
                case ProxyType.Query:
                    if (!string.Equals(context.Request.Path.Value, _options.PathBase.Value))
                    {
                        return _next.Invoke(context);
                    }
                    if (!context.Request.Query.ContainsKey("url"))
                    {
                        return _next.Invoke(context);
                    }
                    var url = context.Request.Query["url"];
                    if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out uri))
                    {
                        return _next.Invoke(context);
                    }
                    break;
                default:
                    _next.Invoke(context);
                    break;
            }
            if (uri == null)
            {
                _next.Invoke(context);
            }
            return context.ProxyRequest(uri);
        }
    }
}
