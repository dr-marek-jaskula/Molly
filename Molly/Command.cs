namespace Molly;

public class Command
{
    public Func<Task> _Executable { get; set; }
    public string Description { get; set; } = string.Empty;
    public List<(string trigger, bool wasUsed)> Triggers { get; set; } = new();

    public Command(List<string> triggers, Func<Task> executable)
    {
        Triggers = triggers.Select(t => (t, false)).ToList();
        _Executable = executable;
    }

    public Command(string description, List<string> triggers, Func<Task> executable) : this(triggers, executable)
    {
        Description = description;
    }

    public async Task Invoke()
    {
        if (Triggers.All(t => t.wasUsed))
        {
            await _Executable.Invoke();
            ResetTriggers();
        }
    }

    public void SetTrigger(string trigger)
    {
        var foundtrigger = Triggers.First(t => t.trigger == trigger);
        foundtrigger.wasUsed = true;
    }

    private void ResetTriggers()
    {
        Triggers.ForEach(t => t.wasUsed = false);
    }
}