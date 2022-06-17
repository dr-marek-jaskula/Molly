using Molly;
using Molly.StringApproxAlgorithms;
using Microsoft.Extensions.Configuration;

var symSpellCommands = SymSpellFactory.CreateSymSpell(@$"{GeneralSettings.Path}\SymSpell\commands.txt", 4);

var configuration = new ConfigurationBuilder()
    .AddJsonFile($"{GeneralSettings.Path}appsettings.json")
    .AddUserSecrets<Program>()
    .Build();

AuthenticationSettings authenticationSettings = new(
    configuration["AuthenticationSettings:key"],
    configuration["AuthenticationSettings:region"]);

ISecretary molly = new Secretary(authenticationSettings, symSpellCommands);

while (true)
{
    string recognizedText = await molly.Listen();
    await molly.Speak(recognizedText);
}
