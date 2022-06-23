using Microsoft.CognitiveServices.Speech;
using Molly.Authentication;
using Molly.StringApproxAlgorithms;

namespace Molly.Secretary;

public abstract class BaseSecretary : ISecretary
{
    private readonly AuthenticationSettings _settings;
    private readonly string _speakLanguage = "en";
    private readonly string _voice = "en-US-AshleyNeural";
    private readonly string _listenLanguage = "en-US";

    protected readonly SpeechConfig _listenConfig;
    protected readonly SpeechConfig _speakConfig;
    protected readonly Dictionary<string, SymSpell> _symSpells = new();

    public string Name { get; } = "Molly";

    protected BaseSecretary(AuthenticationSettings settings)
    {
        _settings = settings;
        _symSpells.Add("name", SymSpellFactory.CreateSymSpell(new() { Name }));
        _speakConfig = ConfigureSpeaking();
        _speakConfig = ConfigureSpeaking();
        _listenConfig = ConfigureListening();
    }

    private SpeechConfig ConfigureSpeaking()
    {
        var speakConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        speakConfig.SpeechRecognitionLanguage = _speakLanguage;
        speakConfig.SpeechSynthesisVoiceName = _voice;
        return speakConfig;
    }

    private SpeechConfig ConfigureListening()
    {
        var listenConfig = SpeechConfig.FromSubscription(_settings.Key, _settings.Region);
        listenConfig.SpeechRecognitionLanguage = _listenLanguage;
        return listenConfig;
    }

    public abstract Task<string> Listen();

    public abstract Task Speak(string text);

    public abstract Task SearchForCommands(string recognizedText, string symSpellKey, bool useMollyName = true);

    public abstract Task<string> ListenForAnswer(string textAfter);
}