using Molly.Secretary;

namespace Molly.Commands;

public class HelloCommand : CommandBase
{
    public HelloCommand(ISecretary secretary) : base(secretary)
    {
        _triggers.Add("hello", false);
    }

    public override async Task InvokeAsync()
    {
        if (_triggers.All(t => t.Value))
        {
            await _secretary.Speak("Hello Mark. How are you?");
            ResetTriggers();
        }
    }
}