using Microsoft.AspNetCore.Builder;
using ProductCatalogueApp.Services;

namespace ProductCatalogueApp.Extensions
{
    public static class ExceptionHandlerExtension
    {
        public static IApplicationBuilder UseSentryExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandler>();
        }
    }
}
