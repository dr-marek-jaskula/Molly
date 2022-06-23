using Microsoft.Extensions.DependencyInjection;
using Molly.Commands;
using System.Reflection;

namespace Molly.Handlers;

public class OrchestratorHandler
{
    private readonly Dictionary<string, Type> _commandTypes = new();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public OrchestratorHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        SaveCommands();
    }

    public IEnumerable<ICommand>? GetAllCommands()
    {
        var commandType = _commandTypes.Values;

        if (commandType is null)
            return null;

        var listOfCommands = new List<ICommand>();

        using var serviceScope = _serviceScopeFactory.CreateScope();

        foreach (var command in commandType)
            listOfCommands.Add((ICommand)serviceScope.ServiceProvider.GetRequiredService(command));

        return listOfCommands;
    }

    private void SaveCommands()
    {
        IEnumerable<TypeInfo> commandTypes = ExtensionsHandler.GetCommandTypesForAssembly(typeof(ICommand).Assembly);

        foreach (var commandType in commandTypes)
        {
            var commandName = commandType.Name[..commandType.Name.IndexOf("Command")];
            _commandTypes[commandName] = commandType;
        }
    }
}