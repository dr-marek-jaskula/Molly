namespace Molly;

public class Command
{
    private readonly Func<Task> _executable;

    public string Description { get; set; } = string.Empty;
    public Dictionary<string, bool> TriggersWithState { get; set; } = new();

    public Command(List<string> triggers, Func<Task> executable)
    {
        triggers.ForEach(t => TriggersWithState[t] = false);
        _executable = executable;
    }

    public Command(string description, List<string> triggers, Func<Task> executable) : this(triggers, executable)
    {
        Description = description;
    }

    public async Task Invoke()
    {
        if (TriggersWithState.All(t => t.Value))
        {
            await _executable.Invoke();
            ResetTriggers();
        }
    }

    public void SetTrigger(string trigger)
    {
        if (TriggersWithState.ContainsKey(trigger))
            TriggersWithState[trigger] = true;
    }

    public void ResetTriggers()
    {
        foreach (var triggerWithState in TriggersWithState)
            TriggersWithState[triggerWithState.Key] = false;
    }
}