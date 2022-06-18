using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech;
using Molly.StringApproxAlgorithms;

namespace Molly;

public interface ISecretary
{
    Task<string> Listen();
    Task Speak(string text);
    Task SearchCommands(string recognizedText, string symSpellKey, bool useMollyName = true);
}

public abstract class BaseSecretary : ISecretary
{
    public string Name = "Molly";
    public readonly string ListenLanguage = "en-US";
    public readonly string SpeakLanguage = "en";
    public readonly string Voice = "en-US-AshleyNeural";
    protected readonly AuthenticationSettings _settings;
    public readonly Dictionary<string, SymSpell> _symSpells = new();
    protected readonly SpeechConfig _listenConfig;
    protected readonly SpeechConfig _speakConfig;

    public BaseSecretary(AuthenticationSettings settings)
    {
        _settings = settings;
        _symSpells.Add("commands", SymSpellFactory.CreateSymSpell(@$"{GeneralSettings.Path}\SymSpell\commands.txt", 4));
        _symSpells.Add("name", SymSpellFactory.CreateSymSpell(new() { Name }));
        _symSpells.Add("continue", SymSpellFactory.CreateSymSpell(new() { "yes", "no" }));
        _symSpells.Add("newLine", SymSpellFactory.CreateSymSpell(new() { "new", "line" }));
        _symSpells.Add("mailTargets", SymSpellFactory.CreateSymSpell(@$"{GeneralSettings.Path}\SymSpell\mailTargets.txt", 4));
        _symSpells.Add("title", SymSpellFactory.CreateSymSpell(new() { "title", "is" }));
        _symSpells.Add("body", SymSpellFactory.CreateSymSpell(new() { "body", "is" }));
        _speakConfig = ConfigureSpeaking();
        _listenConfig = ConfigureListening();
    }

    public BaseSecretary(AuthenticationSettings settings, string language, string voice, string listenLanguage) : this(settings)
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
    public abstract Task SearchCommands(string recognizedText, string symSpellKey, bool useMollyName = true);
}

public class Secretary : BaseSecretary
{
    private int _writeMail = 0;

    public Secretary(AuthenticationSettings settings) 
        : base(settings)
    {
    }

    public Secretary(AuthenticationSettings settings, string language, string voice, string listenLanguage) 
        : base(settings, language, voice, listenLanguage)
    {
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

    public override async Task SearchCommands(string recognizedText, string symSpellKey, bool useMollyName = true)
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

        string command;

        foreach (string word in listOfWords)
        {
            command = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells[symSpellKey]);

            if (string.IsNullOrEmpty(command))
                continue;

            await ExecuteCommand(command);
        }
    }

    private async Task ExecuteCommand(string command)
    {
        if (command == "hello")
            await Speak("Hello Marek. How are you?");
        else if (command == "goodbye")
        {
            await Speak("Goodbye Marek. See you next time?");
            System.Environment.Exit(0);
        }
        else if (command == "write")
        {
            _writeMail++;
        }
        else if (command == "mail")
        {
            _writeMail++;
        }

        if (_writeMail == 2)
        {
            _writeMail = 0;
            await WriteMail();
        }
    }

    private async Task WriteMail()
    {
        string mail = string.Empty;

        await Speak("Please, give the mail target");
        do
        {
            var listenToMailTarget = Helpers.SplitToWords(await Listen());

            if (!listenToMailTarget.Any())
                continue;

            foreach (string word in listenToMailTarget)
            {
                mail = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells["mailTargets"]);

                if (string.IsNullOrEmpty(mail))
                    continue;
            }

        } while (string.IsNullOrEmpty(mail));

        await Speak($"The mail target is: {mail}");

        await Speak("Please, give the mail title");

        string mailTitle = string.Empty;
        string searchForTitileIs;

        do
        {
            var listenToMailTitle = Helpers.SplitToWords(await Listen());

            if (!listenToMailTitle.Any())
                continue;

            foreach (string word in listenToMailTitle)
            {
                mailTitle = $"{mailTitle} {word}";

                searchForTitileIs = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells["title"]);
                
                if (searchForTitileIs == "title")
                    mailTitle = string.Empty;
                else if (searchForTitileIs == "is")
                    mailTitle = string.Empty;
            }

        } while (string.IsNullOrEmpty(mailTitle));

        await Speak($"The mail title is: {mailTitle}");

        await Speak("Please, give the mail body");
        string mailBody = string.Empty;

        do
        {
            var listenToMailBody = Helpers.SplitToWords(await Listen());

            if (!listenToMailBody.Any())
                continue;

            foreach (string word in listenToMailBody)
            {
                mailBody = $"{mailBody} {word}";

                searchForTitileIs = SymSpellAlgorithm.FindBestSuggestion(word, _symSpells["body"]);

                if (searchForTitileIs == "body")
                    mailBody = string.Empty;
                else if (searchForTitileIs == "is")
                    mailBody = string.Empty;
            }

        } while (string.IsNullOrEmpty(mailBody));

        await Speak($"The mail body is: {mailBody}");
    }
}