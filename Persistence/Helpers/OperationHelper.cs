using System.ComponentModel;
using System.Reflection;

namespace Persistence.Helpers;

/// <summary>
/// This class helps with every database restart,
/// automatically adding all method names and controllers to it.
/// When deploying, it is better to remove it and use manual entry.
/// </summary>
public static class OperationHelper
{
    public record Action(string Name, string Description);
    /// <summary>
    /// A type that represents information about an operation
    /// </summary>
    /// <param name="Controller">Controller name</param>
    /// <param name="Actions">List of controller methods</param>
    public record Operation(string Controller, List<Action> Actions);
    /// <summary>
    /// Method for obtaining all necessary information about methods.
    /// </summary>
    /// <returns>Methods and controllers that represent HTTP access points.</returns>
    public static List<Operation> GetOperations()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var controllers = assemblies
            .SelectMany(assembly => assembly.GetTypes()) 
            .Where(type => type.IsClass 
                           && type.Namespace != null 
                           && type.Namespace.EndsWith(".Controllers")
                           && !type.Name.Contains('<')
                           && !type.Namespace.Contains("Microsoft")); 
        
        var operations = new List<Operation>();
        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName 
                            && m.GetCustomAttributes(typeof(DescriptionAttribute), false).Any())
                .Select(m => new Action(m.Name, m.GetCustomAttribute<DescriptionAttribute>().Description))
                .ToList();
            operations.Add(new Operation(controller.Name.Replace("Controller", ""), methods));
        }
        return operations;
    }
}