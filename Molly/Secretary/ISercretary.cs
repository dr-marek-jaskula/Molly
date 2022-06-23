namespace Molly.Secretary;

public interface ISecretary
{
    Task<string> Listen();
    Task Speak(string text);
    Task SearchForCommands(string recognizedText, string symSpellKey, bool useMollyName = true);
    Task<string> ListenForAnswer(string textAfter);
}