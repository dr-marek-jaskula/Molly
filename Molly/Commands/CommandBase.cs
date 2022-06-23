using Molly.Secretary;

namespace Molly.Commands;

public abstract class CommandBase : ICommand
{
    protected readonly Dictionary<string, bool> _triggers = new();

    public List<string> Triggers { get => _triggers.Keys.ToList(); }

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

    public abstract Task InvokeAsync(ISecretary? secretary = null);
}