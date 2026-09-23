using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TicketManagement.Api.Swagger
{
    // AddSwaggerGen's own configure-lambda overload can't take a constructor
    // dependency - it just receives a bare SwaggerGenOptions. Discovering
    // which API versions actually exist requires IApiVersionDescriptionProvider,
    // which only becomes available at DI-resolution time (after
    // AddApiVersioning().AddApiExplorer() have registered it), not at the
    // moment AddSwaggerGen is called during startup wiring. IConfigureOptions<T>
    // is the standard way to get constructor-injected configuration for an
    // options type instead - this is the same pattern the aspnet-api-versioning
    // project's own Swashbuckle sample uses.
    //
    // The payoff: this class runs once per discovered version and registers one
    // Swagger document per version automatically. Adding a v2.0 controller
    // later needs zero changes here - the provider will just report a second
    // description and a second document appears.
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _provider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
        {
            _provider = provider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo
                {
                    Title = "Ticket Management API",
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated
                        ? "This API version is deprecated. Migrate to a supported version."
                        : "Ticket Management System API"
                });
            }
        }
    }
}
