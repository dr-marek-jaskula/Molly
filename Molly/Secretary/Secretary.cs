using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Molly.StringApproxAlgorithms;
using System.Text;
using Molly.Authentication;
using Molly.Commands;
using Molly.Handlers;

namespace Molly.Secretary;

public class Secretary : BaseSecretary
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public Secretary(AuthenticationSettings settings, HandlerOrchestrator handlerOrchestrator) : base(settings)
    {
        var x = handlerOrchestrator.GetAllCommands();

        if (x is not null)
            _commands.Add("hello", x.First());
    }

    public override async Task<string> Listen()
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new SpeechRecognizer(_listenConfig, audioConfig);

        var recognizedSpeech = await recognizer.RecognizeOnceAsync();

        return recognizedSpeech.Text;
    }

    public override async Task Speak(string text)
    {
        using var speechSynthesizer = new SpeechSynthesizer(_speakConfig);
        await speechSynthesizer.SpeakTextAsync(text);
    }

    public override async Task SearchForCommands(string recognizedText, string symSpellKey, bool useMollyName = true)
    {
        var listOfWords = Helpers.SplitToWords(recognizedText);

        if (!listOfWords.Any())
            return;

        if (useMollyName)
        {
            if ("Molly" != Helpers.Capitalize(SymSpellAlgorithm.FindBestSuggestion(listOfWords.First(), _symSpells["name"])))
                return;

            listOfWords.RemoveAt(0);
        }

        string heardCommand;
        string commandKey;

        foreach (string word in listOfWords)
        {
            heardCommand = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells[symSpellKey]);

            if (string.IsNullOrEmpty(heardCommand))
                continue;

            commandKey = FindCommandKey(heardCommand);

            _commands[commandKey].SetTrigger(heardCommand);
            await _commands[commandKey].InvokeAsync();
        }

        foreach (var command in _commands)
            command.Value.ResetTriggers();
    }

    private string FindCommandKey(string heardCommand)
    {
        foreach (var commandKey in _commands.Keys)
            if (commandKey.Contains(heardCommand))
                return commandKey;

        throw new ArgumentException("No command key fits the heard command");
    }

    //Move this out
    private void SaveNote(string name, string body)
    {
        using FileStream file = File.Create($"{GeneralSettings.Path}{name}.txt");
        byte[] bodyInBytes = new UTF8Encoding(true).GetBytes(body);
        file.Write(bodyInBytes, 0, bodyInBytes.Length);
    }

    private async Task<(string noteName, string body)> MakeNote()
    {
        await Speak("Please, give the note name");
        var noteName = await ListenForAnswer("name is");
        await Speak($"The note name is: {noteName}");

        await Speak("Please, give the note body");
        var noteBody = await ListenForAnswer("body is");
        await Speak($"The note body is: {noteBody}");

        return (noteName, noteBody);
    }

    private async Task<string> ListenForAnswer(string textAfter)
    {
        string answer;

        while (string.IsNullOrEmpty(answer = await Listen()))
        {
        }

        if (answer.Contains(textAfter, StringComparison.CurrentCultureIgnoreCase))
        {
            int indexFrom = answer.IndexOf(textAfter) + textAfter.Length + 1;

            if (indexFrom >= answer.Length)
                return await ListenForAnswer(textAfter);

            return answer[indexFrom..^1];
        }

        return answer;
    }

    private void SetCommands()
    {
        //_commands.Add("goodbye", new("Molly will say goodbye and then end the program", new() { "goodbye" },
        //    async () =>
        //    {
        //        await Speak("Goodbye Mark. See you next time?");
        //        Environment.Exit(0);
        //    }));

        //_commands.Add("make note", new("Molly will make a note preceded by questions about note title and body", new() { "make", "note" },
        //    async () =>
        //    {
        //        (string mailTarget, string body) = await MakeNote();
        //        SaveNote(mailTarget, body);
        //    }));
    }
}