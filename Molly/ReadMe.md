## Molly - The Secretary

This is a simple voice application creating for tutorial purposes.

Molly response to three commands if the command start from word **Molly** and contains: 
- **hello**: Molly will answer "Hello. How are you?".
- **goodbye**: Molly will answer "Goodbye. See you next time."
- **make**, "note" in any order: Molly will ask about the note title, then note body. Finally the text file will be created.

Application can be easy extended: to add a new command just create a class that inherits from the base abstract class **CommandBase**.
This class should override the **InvokeAsync** method.

Technologies:
Azure Speech Service
Dependency Injection Framework
SymSpell