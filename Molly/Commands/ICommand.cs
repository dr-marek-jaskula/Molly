using Molly.Secretary;

namespace Molly.Commands;

public interface ICommand
{
    Task InvokeAsync(ISecretary? secretary = null);
    List<string> Triggers { get; }
    void SetTrigger(string trigger);
    void ResetTriggers();
}