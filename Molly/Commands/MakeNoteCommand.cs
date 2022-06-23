using Molly.Secretary;
using System.Text;

namespace Molly.Commands;

public class MakeNoteCommand : CommandBase
{
    public MakeNoteCommand()
    {
        _triggers.Add("make", false);
        _triggers.Add("note", false);
    }

    public override async Task InvokeAsync(ISecretary? secretary)
    {
        if (secretary is null)
            throw new ArgumentNullException(nameof(secretary));

        if (_triggers.All(t => t.Value))
        {
            (string mailTarget, string body) = await MakeNote(secretary);
            SaveNote(mailTarget, body);
            ResetTriggers();
        }
    }

    private static void SaveNote(string name, string body)
    {
        using FileStream file = File.Create($"{GeneralSettings.Path}{name}.txt");
        byte[] bodyInBytes = new UTF8Encoding(true).GetBytes(body);
        file.Write(bodyInBytes, 0, bodyInBytes.Length);
    }

    private static async Task<(string noteName, string body)> MakeNote(ISecretary secretary)
    {
        await secretary.Speak("Please, give the note name");
        var noteName = await secretary.ListenForAnswer("name is");
        await secretary.Speak($"The note name is: {noteName}");

        await secretary.Speak("Please, give the note body");
        var noteBody = await secretary.ListenForAnswer("body is");
        await secretary.Speak($"The note body is: {noteBody}");

        return (noteName, noteBody);
    }
}