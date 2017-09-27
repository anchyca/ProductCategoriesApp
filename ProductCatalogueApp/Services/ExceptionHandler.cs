using Microsoft.AspNetCore.Http;
using SharpRaven;
using SharpRaven.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ProductCatalogueApp.Services
{
    public class ExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly RavenClient _ravenClient;

        public ExceptionHandler(RequestDelegate next, string ravenConnection)
        {
            this._next = next;
            this._ravenClient = new RavenClient(ravenConnection);
        }
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                _ravenClient.Capture(new SentryEvent(e));
            }
        }
    }
}
