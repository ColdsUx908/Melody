using Transoceanic.RuntimeEditing;

namespace Transoceanic.Publicizer;

public interface IPublicizer
{
    public static abstract Type C_type { get; }
}

public abstract record Publicizer<T> : IPublicizer
{
    public static Type C_type { get; } = typeof(T);

    public static FieldInfo GetInstanceField(string name) => C_type.GetField(name, TOReflectionUtils.InstanceBindingFlags);

    public static PropertyInfo GetInstanceProperty(string name) => C_type.GetProperty(name, TOReflectionUtils.InstanceBindingFlags);

    public static MethodInfo GetInstanceMethod(string name) => C_type.GetMethod(name, TOReflectionUtils.InstanceBindingFlags);

    public static ConstructorInfo GetInstanceConstructor(Type[] parameterTypes) => C_type.GetConstructor(TOReflectionUtils.InstanceBindingFlags, parameterTypes);

    public static EventInfo GetInstanceEvent(string name) => C_type.GetEvent(name, TOReflectionUtils.InstanceBindingFlags);

    public static FieldInfo GetStaticField(string name) => C_type.GetField(name, TOReflectionUtils.StaticBindingFlags);

    public static PropertyInfo GetStaticProperty(string name) => C_type.GetProperty(name, TOReflectionUtils.StaticBindingFlags);

    public static MethodInfo GetStaticMethod(string name) => C_type.GetMethod(name, TOReflectionUtils.StaticBindingFlags);

    public static ConstructorInfo GetStaticConstructor() => C_type.TypeInitializer;

    public static EventInfo GetStaticEvent(string name) => C_type.GetEvent(name, TOReflectionUtils.StaticBindingFlags);
}

public abstract record PublicizerBase<T>(T Source) : Publicizer<T> where T : class;
