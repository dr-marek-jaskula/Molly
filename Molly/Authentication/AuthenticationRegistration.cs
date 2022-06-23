using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Molly.Authentication;

public static class AuthenticationRegistration
{
    public static AuthenticationSettings ConfigureAuthentication(this IConfigurationRoot configuration)
    {
        AuthenticationSettings authenticationSettings = new(
            configuration["AuthenticationSettings:key"],
            configuration["AuthenticationSettings:region"]);

        return authenticationSettings;
    }

    public static void RegisterAuthentication(this IServiceCollection services, AuthenticationSettings authenticationSettings)
    {
        services.AddSingleton(authenticationSettings);
    }
}
