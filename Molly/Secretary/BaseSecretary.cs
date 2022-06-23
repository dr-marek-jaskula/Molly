using Microsoft.CognitiveServices.Speech;
using Molly.Authentication;
using Molly.StringApproxAlgorithms;

namespace Molly.Secretary;

public abstract class BaseSecretary : ISecretary
{
    private readonly AuthenticationSettings _settings;
    private readonly string SpeakLanguage = "en";
    private readonly string Voice = "en-US-AshleyNeural";
    private readonly string ListenLanguage = "en-US";

    protected readonly SpeechConfig _listenConfig;
    protected readonly SpeechConfig _speakConfig;
    protected readonly Dictionary<string, SymSpell> _symSpells = new();

    public string Name { get; } = "Molly";

    protected BaseSecretary(AuthenticationSettings settings)
    {
        _settings = settings;
        _symSpells.Add("commands", SymSpellFactory.CreateSymSpell(@$"{GeneralSettings.Path}\SymSpell\commands.txt", 4));
        _symSpells.Add("name", SymSpellFactory.CreateSymSpell(new() { Name }));
        _speakConfig = ConfigureSpeaking();
        _speakConfig = ConfigureSpeaking();
        _listenConfig = ConfigureListening();
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