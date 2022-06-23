using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Molly.Authentication;
using Molly.Commands;
using Molly.Handlers;
using Molly.StringApproxAlgorithms;

namespace Molly.Secretary;

public class Secretary : BaseSecretary
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public Secretary(AuthenticationSettings settings, OrchestratorHandler handler) : base(settings)
    {
        var commands = handler.GetAllCommands();

        if (commands is null)
            return;

        List<string> commandNames = new();

        foreach (var command in commands)
        {
            commandNames.AddRange(command.Triggers);
            var combinedName = string.Join('-', command.Triggers);
            _commands.Add(combinedName, command);
        }

        _symSpells.Add("commands", SymSpellFactory.CreateSymSpell(commandNames, commandNames.Count()));
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
            await _commands[commandKey].InvokeAsync(this);
        }

        foreach (var command in _commands)
            command.Value.ResetTriggers();
    }

    public override async Task<string> ListenForAnswer(string textAfter)
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

    private string FindCommandKey(string heardCommand)
    {
        foreach (var commandKey in _commands.Keys)
            if (commandKey.Contains(heardCommand))
                return commandKey;

        throw new ArgumentException("No command key fits the heard command");
    }
}