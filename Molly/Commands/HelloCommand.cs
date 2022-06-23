﻿using Molly.Secretary;

namespace Molly.Commands;

public class HelloCommand : CommandBase
{
    public HelloCommand()
    {
        _triggers.Add("hello", false);
    }

    public override async Task InvokeAsync(ISecretary? secretary)
    {
        if (secretary is null)
            throw new ArgumentNullException(nameof(secretary));

        if (_triggers.All(t => t.Value))
        {
            await secretary.Speak("Hello Mark. How are you?");
            ResetTriggers();
        }
    }
}