using Microsoft.AspNetCore.Http;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Services
{
    public class RequestMonitor
    {
        private readonly RequestDelegate _next;
        private readonly RavenClient _ravenClient;
        public RequestMonitor(RequestDelegate next, string ravenConnection)
        {
            this._next = next;
            this._ravenClient = new RavenClient(ravenConnection);
        }
        public async Task Invoke(HttpContext context)
        {
            string message = $"Request for {context.Request.Path} with trace id {context.TraceIdentifier} received ({context.Request.ContentLength ?? 0} bytes).\r\n" +
                $"Request headers: {DictionaryToString(context.Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))}\r\n" +
                $"Request host: {context.Request.Host}\r\n" +
                $"Response headers: {DictionaryToString(context.Response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()))}\r\n" +
                $"Response status code: {context.Response.StatusCode}";
      
            SentryEvent sentryEvent = new SentryEvent(message);
            sentryEvent.Level = ErrorLevel.Info;

            _ravenClient.Capture(sentryEvent);

            await _next.Invoke(context);
        }

        private object DictionaryToString(Dictionary<string, string> dictionary)
        {
            StringBuilder sb = new StringBuilder();

            foreach(var key in dictionary.Keys)
            {
                sb.Append(key + ": " + dictionary.GetValueOrDefault(key));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }
    }
}
