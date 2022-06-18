using Molly;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"{GeneralSettings.Path}appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

AuthenticationSettings authenticationSettings = new(
    configuration["AuthenticationSettings:key"],
    configuration["AuthenticationSettings:region"]);

ISecretary molly = new Secretary(authenticationSettings);

while (true)
{
    string recognizedText = await molly.Listen();
    await molly.SearchCommands(recognizedText, "commands");
}
