using Molly.Secretary;

namespace Molly.Commands;

public class GoodbyeCommand : CommandBase
{
    public GoodbyeCommand()
    {
        _triggers.Add("goodbye", false);
    }

    public override async Task InvokeAsync(ISecretary? secretary)
    {
        if (secretary is null)
            throw new ArgumentNullException(nameof(secretary));

        if (_triggers.All(t => t.Value))
        {
            await secretary.Speak("Goodbye. See you next time.");
            Environment.Exit(0);
        }
    }
}