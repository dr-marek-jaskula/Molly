using Molly.Secretary;

namespace Molly.Commands;

public abstract class CommandBase : ICommand
{
    protected readonly Dictionary<string, bool> _triggers = new();
    protected readonly ISecretary _secretary;

    protected CommandBase(ISecretary secretary)
    {
        _secretary = secretary;
    }

    public void SetTrigger(string trigger)
    {
        if (_triggers.ContainsKey(trigger))
            _triggers[trigger] = true;
    }

    public void ResetTriggers()
    {
        foreach (var triggerWithState in _triggers)
            _triggers[triggerWithState.Key] = false;
    }

    public abstract Task InvokeAsync();
}