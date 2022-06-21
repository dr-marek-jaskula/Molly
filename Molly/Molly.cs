using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Molly.StringApproxAlgorithms;
using System.Text;

namespace Molly;

public interface ISecretary
{
    Task<string> Listen();
    Task Speak(string text);
    Task SearchForCommands(string recognizedText, string symSpellKey, bool useMollyName = true);
}

public abstract class BaseSecretary : ISecretary
{
    protected readonly AuthenticationSettings _settings;
    protected readonly SpeechConfig _listenConfig;
    protected readonly SpeechConfig _speakConfig;
    protected readonly Dictionary<string, SymSpell> _symSpells = new();
    
    public string Name { get; set; } = "Molly";
    public string ListenLanguage { get; } = "en-US";
    public string SpeakLanguage { get; } = "en";
    public string Voice { get; } = "en-US-AshleyNeural";

    public BaseSecretary(AuthenticationSettings settings)
    {
        _settings = settings;
        _symSpells.Add("commands", SymSpellFactory.CreateSymSpell(@$"{GeneralSettings.Path}\SymSpell\commands.txt", 4));
        _symSpells.Add("name", SymSpellFactory.CreateSymSpell(new() { Name }));
        _symSpells.Add("continue", SymSpellFactory.CreateSymSpell(new() { "yes", "no" }));
        _symSpells.Add("newLine", SymSpellFactory.CreateSymSpell(new() { "new", "line" }));
        _speakConfig = ConfigureSpeaking();
        _listenConfig = ConfigureListening();
    }

    public BaseSecretary(AuthenticationSettings settings, string language, string voice, string listenLanguage) : this(settings)
    {
        SpeakLanguage = language;
        Voice = voice;
        ListenLanguage = listenLanguage;
    }

    private SpeechConfig ConfigureSpeaking()
    {
        var speakConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        speakConfig.SpeechRecognitionLanguage = SpeakLanguage;
        speakConfig.SpeechSynthesisVoiceName = Voice;
        return speakConfig;
    }

    private SpeechConfig ConfigureListening()
    {
        var listenConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        listenConfig.SpeechRecognitionLanguage = ListenLanguage;
        return listenConfig;
    }

    public abstract Task<string> Listen();
    public abstract Task Speak(string text);
    public abstract Task SearchForCommands(string recognizedText, string symSpellKey, bool useMollyName = true);
}

public class Secretary : BaseSecretary
{
    private readonly Dictionary<string, Command> _commands = new();

    public Secretary(AuthenticationSettings settings) 
        : base(settings)
    {
        SetCommands();
    }

    public Secretary(AuthenticationSettings settings, string language, string voice, string listenLanguage) 
        : base(settings, language, voice, listenLanguage)
    {
        SetCommands();
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

        foreach (string word in listOfWords)
        {
            heardCommand = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells[symSpellKey]);

            if (string.IsNullOrEmpty(heardCommand))
                continue;

            _commands[heardCommand].SetTrigger(heardCommand);
            await _commands[heardCommand].Invoke();
        }

        foreach (var command in _commands)
            command.Value.ResetTriggers();
    }

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

        while (string.IsNullOrEmpty((answer = await Listen())))
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
        _commands.Add("hello", new("Molly will say hello and ask how are you", new() { "hello" },
            async () => 
            {
                await Speak("Hello Mark. How are you?");
            }));

        _commands.Add("goodbye", new("Molly will say goodbye and then end the program", new() { "goodbye" },
            async () =>
            {
                await Speak("Goodbye Mark. See you next time?");
                System.Environment.Exit(0);
            }));

        _commands.Add("MakeNote", new("Molly will make a note preceded by questions about note title and body", new() { "make", "note" },
            async () =>
            {
                (string mailTarget, string body) = await MakeNote();
                SaveNote(mailTarget, body);
            }));
    }
}