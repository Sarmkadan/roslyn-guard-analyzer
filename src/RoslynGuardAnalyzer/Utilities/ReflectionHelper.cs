#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RoslynGuardAnalyzer.Utilities;

/// <summary>
/// Helper methods for reflection operations on types and members.
/// Used to extract metadata from code elements for analysis.
/// </summary>
public static class ReflectionHelper
{
    /// <summary>
    /// Gets all public methods from a type.
    /// </summary>
    public static IEnumerable<MethodInfo> GetPublicMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
            .Where(m => !m.IsSpecialName);
    }

    /// <summary>
    /// Gets all public properties from a type.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetPublicProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Gets all public fields from a type.
    /// </summary>
    public static IEnumerable<FieldInfo> GetPublicFields(Type type)
    {
        return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }

    /// <summary>
    /// Checks if a type implements a specific interface.
    /// </summary>
    public static bool ImplementsInterface(Type type, Type interfaceType)
    {
        return type.GetInterfaces().Any(i =>
            i == interfaceType || (i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType));
    }

    /// <summary>
    /// Checks if a type is a subclass of a base type.
    /// </summary>
    public static bool IsSubclassOf(Type type, Type baseType)
    {
        return type.IsSubclassOf(baseType);
    }

    /// <summary>
    /// Gets all attributes of a specific type from a member.
    /// </summary>
    public static IEnumerable<T> GetAttributes<T>(MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttributes(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Checks if a method is async.
    /// </summary>
    public static bool IsAsync(MethodInfo method)
    {
        var returnType = method.ReturnType;

        if (returnType == typeof(Task))
            return true;

        if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
            return true;

        return false;
    }

    /// <summary>
    /// Checks if a method is virtual (can be overridden).
    /// </summary>
    public static bool IsVirtual(MethodInfo method)
    {
        return method.IsVirtual && !method.IsFinal;
    }

    /// <summary>
    /// Gets the parameter count of a method or constructor.
    /// </summary>
    public static int GetParameterCount(MethodBase method)
    {
        return method.GetParameters().Length;
    }

    /// <summary>
    /// Gets parameter names from a method.
    /// </summary>
    public static IEnumerable<string> GetParameterNames(MethodBase method)
    {
        return method.GetParameters().Select(p => p.Name ?? string.Empty);
    }

    /// <summary>
    /// Gets all types that implement a specific interface.
    /// </summary>
    public static IEnumerable<Type> GetImplementationsOfInterface(Type interfaceType, Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsInterface && ImplementsInterface(t, interfaceType));
    }

    /// <summary>
    /// Gets all types with a specific attribute.
    /// </summary>
    public static IEnumerable<Type> GetTypesWithAttribute<T>(Assembly assembly) where T : Attribute
    {
        return assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(T)).Any());
    }

    /// <summary>
    /// Gets the full type name including namespace.
    /// </summary>
    public static string GetFullName(Type type)
    {
        return type.FullName ?? type.Name;
    }

    /// <summary>
    /// Checks if a type is a value type (struct) vs reference type (class).
    /// </summary>
    public static bool IsValueType(Type type)
    {
        return type.IsValueType;
    }

    /// <summary>
    /// Checks if a type is abstract.
    /// </summary>
    public static bool IsAbstract(Type type)
    {
        return type.IsAbstract;
    }

    /// <summary>
    /// Checks if a type is sealed.
    /// </summary>
    public static bool IsSealed(Type type)
    {
        return type.IsSealed;
    }

    /// <summary>
    /// Gets the base type of a type (excluding object).
    /// </summary>
    public static Type? GetBaseType(Type type)
    {
        var baseType = type.BaseType;
        return baseType == typeof(object) ? null : baseType;
    }

    /// <summary>
    /// Gets the inheritance hierarchy of a type.
    /// </summary>
    public static IEnumerable<Type> GetInheritanceHierarchy(Type type)
    {
        var current = type;
        while (current is not null && current != typeof(object))
        {
            yield return current;
            current = current.BaseType;
        }
    }
}
