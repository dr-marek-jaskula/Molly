using Molly.Handlers;
using Molly.Secretary;

namespace Molly;

public class Application
{
    private readonly ISecretary _secretary;

    public Application(ISecretary secretary)
    {
        _secretary = secretary;
    }

    public async Task RunAsync()
    {
        while (true)
        {
            string recognizedText = await _secretary.Listen();
            await _secretary.SearchForCommands(recognizedText, "commands");
        }
    }
}
