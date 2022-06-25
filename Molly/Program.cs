using Molly;
using Molly.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Molly.Secretary;
using Molly.Handlers;

var services = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"{GeneralSettings.Path}appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

services.RegisterAuthentication(configuration.ConfigureAuthentication());

services.AddSingleton<ISecretary, Secretary>();

services.AddCommands(typeof(Program).Assembly);

services.AddSingleton<Application>();

var serviceProvider = services.BuildServiceProvider();

var application = serviceProvider.GetRequiredService<Application>();

await application.RunAsync();

public static class GeneralSettings
{
    public static readonly string Path =
        AppDomain.CurrentDomain.BaseDirectory.Split(@"bin\", StringSplitOptions.None)[0];
}