using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Molly.StringApproxAlgorithms;

namespace Molly;

public interface ISecretary
{
    Task<string> Listen();
    Task Speak(string text);
}

public abstract class BaseSecretary : ISecretary
{
    public string Name = "Molly";
    public readonly string ListenLanguage = "en-US";
    public readonly string SpeakLanguage = "en";
    public readonly string Voice = "en-US-AshleyNeural";
    protected readonly AuthenticationSettings _settings;
    protected readonly SymSpell _symSpellCommands;
    protected readonly SymSpell _symSpellName;
    protected readonly SpeechConfig _listenConfig;
    protected readonly SpeechConfig _speakConfig;

    public BaseSecretary(AuthenticationSettings settings, SymSpell symSpellCommands)
    {
        _settings = settings;
        _symSpellCommands = symSpellCommands;
        _symSpellName = SymSpellFactory.CreateSymSpell(new() { Name });
        _speakConfig = ConfigureSpeaking();
        _listenConfig = ConfigureListening();
    }

    public BaseSecretary(AuthenticationSettings settings, SymSpell symSpellCommands, string language, string voice, string listenLanguage) : this(settings, symSpellCommands)
    {
        SpeakLanguage = language;
        Voice = voice;
        ListenLanguage = listenLanguage;
    }

    protected SpeechConfig ConfigureSpeaking()
    {
        var speakConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        speakConfig.SpeechRecognitionLanguage = SpeakLanguage;
        speakConfig.SpeechSynthesisVoiceName = Voice;
        return speakConfig;
    }

    protected SpeechConfig ConfigureListening()
    {
        var listenConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        listenConfig.SpeechRecognitionLanguage = ListenLanguage;
        return listenConfig;
    }

    public abstract Task<string> Listen();

    public abstract Task Speak(string text);
}

public class Secretary : BaseSecretary
{
    public Secretary(AuthenticationSettings settings, SymSpell symSpellCommands) 
        : base(settings, symSpellCommands)
    {
    }

    public Secretary(AuthenticationSettings settings, SymSpell symSpellCommands, string language, string voice, string listenLanguage) 
        : base(settings, symSpellCommands, language, voice, listenLanguage)
    {
    }

    public override async Task<string> Listen()
    {
        using var audioConfig = AudioConfig.FromDefaultMicrophoneInput();
        using var recognizer = new SpeechRecognizer(_listenConfig, audioConfig);

        var recognizedSpeech = await recognizer.RecognizeOnceAsync();

        SearchCommands(recognizedSpeech.Text);

        return recognizedSpeech.Text;
    }

    public override async Task Speak(string text)
    {
        using var speechSynthesizer = new SpeechSynthesizer(_speakConfig);
        await speechSynthesizer.SpeakTextAsync(text);
    }

    private void SearchCommands(string recognizedText)
    {
        var listOfWords = Helpers.SplitToWords(recognizedText);

        if (!listOfWords.Any())
            return;

        if ("Molly" != Helpers.Capitalize(SymSpellAlgorithm.FindBestSuggestion(listOfWords.First(), _symSpellName)))
            return;

        listOfWords.RemoveAt(0);

        string command;

        foreach (string word in listOfWords)
        {
            command = SymSpellAlgorithm.FindBestSuggestion(word, _symSpellCommands);

            if (string.IsNullOrEmpty(command))
                continue;

            Console.WriteLine("Recognized command!");
        }
    }
}