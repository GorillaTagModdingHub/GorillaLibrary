using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaLibrary.Extensions;

public static class ReflectionExtensions
{
    public static T GetField<T>(this object source, string name)
    {
        FieldInfo field = AccessTools.Field(source.GetType(), name);
        return (T)field.GetValue(source);
    }

    public static void SetField(this object source, string name, object value)
    {
        FieldInfo field = AccessTools.Field(source.GetType(), name);
        field.SetValue(source, value);
    }

    public static T GetProperty<T>(this object source, string name)
    {
        PropertyInfo property = AccessTools.Property(source.GetType(), name);
        return (T)property.GetValue(source);
    }

    public static void SetProperty(this object source, string name, object value)
    {
        PropertyInfo property = AccessTools.Property(source.GetType(), name);
        property.SetValue(source, value);
    }

    public static void InvokeMethod(this object source, string name, params object[] parameters)
    {
        MethodInfo method = AccessTools.Method(source.GetType(), name);
        method.Invoke(source, parameters);
    }

    public static void InvokeMethod(this object source, string name, Type[] methodParameters, params object[] invokeParameters)
    {
        MethodInfo method = AccessTools.Method(source.GetType(), name, methodParameters);
        method.Invoke(source, invokeParameters);
    }
}
