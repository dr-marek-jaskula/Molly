namespace Molly.Commands;

public interface ICommand
{
    Task InvokeAsync();
    void SetTrigger(string trigger);
    void ResetTriggers();
}