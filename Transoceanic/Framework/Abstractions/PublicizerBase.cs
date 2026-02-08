namespace Transoceanic.Framework.Abstractions;

public interface IPublicizer
{
    public static abstract Type C_type { get; }
}

public abstract record Publicizer<T> : IPublicizer
{
    public static Type C_type { get; } = typeof(T);

    /// <inheritdoc cref="PublicizerHelper.GetInstanceField(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetInstanceField(string name) => PublicizerHelper.GetInstanceField(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetInstanceProperty(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetInstanceProperty(string name) => PublicizerHelper.GetInstanceProperty(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetInstanceMethod(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(string name) => PublicizerHelper.GetInstanceMethod(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetInstanceMethod(Type, string, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(string name, Type[] parameterTypes) => PublicizerHelper.GetInstanceMethod(C_type, name, parameterTypes);
    /// <inheritdoc cref="PublicizerHelper.GetInstanceConstructor(Type, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetInstanceConstructor(Type[] parameterTypes) => PublicizerHelper.GetInstanceConstructor(C_type, parameterTypes);
    /// <inheritdoc cref="PublicizerHelper.GetInstanceEvent(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetInstanceEvent(string name) => PublicizerHelper.GetInstanceEvent(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetStaticField(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetStaticField(string name) => PublicizerHelper.GetStaticField(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetStaticProperty(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetStaticProperty(string name) => PublicizerHelper.GetStaticProperty(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetStaticMethod(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(string name) => PublicizerHelper.GetStaticMethod(C_type, name);
    /// <inheritdoc cref="PublicizerHelper.GetStaticMethod(Type, string, Type[])"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(string name, Type[] parameterTypes) => PublicizerHelper.GetStaticMethod(C_type, name, parameterTypes);
    /// <inheritdoc cref="PublicizerHelper.GetStaticConstructor(Type)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetStaticConstructor() => PublicizerHelper.GetStaticConstructor(C_type);
    /// <inheritdoc cref="PublicizerHelper.GetStaticEvent(Type, string)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetStaticEvent(string name) => PublicizerHelper.GetStaticEvent(C_type, name);
}

public abstract record PublicizerBase<T>(T Source) : Publicizer<T>;
