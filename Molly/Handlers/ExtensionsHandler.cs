using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Molly.Commands;
using System.Reflection;

namespace Molly.Handlers;

public static class ExtensionsHandler
{
    public static void AddCommands(this IServiceCollection services, Assembly assembly)
    {
        services.TryAddSingleton<OrchestratorHandler>();

        var commandTypes = GetCommandTypesForAssembly(assembly);

        foreach (var commandType in commandTypes)
            services.TryAddTransient(commandType);
    }

    public static IEnumerable<TypeInfo> GetCommandTypesForAssembly(Assembly assembly)
    {
        return assembly.DefinedTypes
            .Where(type => !type.IsAbstract 
                    && typeof(ICommand).IsAssignableFrom(type) 
                    && type.BaseType == typeof(CommandBase));
    }
}