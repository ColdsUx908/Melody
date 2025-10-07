namespace Transoceanic.Data.Publicizer;

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

public static class PublicizerHelper
{
    /// <summary>
    /// 获取实例字段，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingFieldException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetInstanceField(Type type, string name) => type.GetField(name, TOReflectionUtils.InstanceBindingFlags) ?? throw new MissingFieldException(type.FullName, name);
    /// <summary>
    /// 获取实例属性，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMemberException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetInstanceProperty(Type type, string name) => type.GetProperty(name, TOReflectionUtils.InstanceBindingFlags) ?? throw new MissingMemberException(type.FullName, name);
    /// <summary>
    /// 获取实例方法，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(Type type, string name) => type.GetMethod(name, TOReflectionUtils.InstanceBindingFlags) ?? throw new MissingMethodException(type.FullName, name);
    /// <summary>
    /// 获取实例方法（重载），找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetInstanceMethod(Type type, string name, Type[] parameterTypes) => type.GetMethod(name, TOReflectionUtils.InstanceBindingFlags, null, parameterTypes, null) ?? throw new MissingMethodException(type.FullName, name);
    /// <summary>
    /// 获取实例构造函数，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetInstanceConstructor(Type type, Type[] parameterTypes) => type.GetConstructor(TOReflectionUtils.InstanceBindingFlags, parameterTypes) ?? throw new MissingMethodException(type.FullName, ".ctor");
    /// <summary>
    /// 获取实例事件，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMemberException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetInstanceEvent(Type type, string name) => type.GetEvent(name, TOReflectionUtils.InstanceBindingFlags) ?? throw new MissingMemberException(type.FullName, name);
    /// <summary>
    /// 获取静态字段，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingFieldException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static FieldInfo GetStaticField(Type type, string name) => type.GetField(name, TOReflectionUtils.StaticBindingFlags) ?? throw new MissingFieldException(type.FullName, name);
    /// <summary>
    /// 获取静态属性，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMemberException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PropertyInfo GetStaticProperty(Type type, string name) => type.GetProperty(name, TOReflectionUtils.StaticBindingFlags) ?? throw new MissingMemberException(type.FullName, name);
    /// <summary>
    /// 获取静态方法，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(Type type, string name) => type.GetMethod(name, TOReflectionUtils.StaticBindingFlags) ?? throw new MissingMethodException(type.FullName, name);
    /// <summary>
    /// 获取静态方法（重载），找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetStaticMethod(Type type, string name, Type[] parameterTypes) => type.GetMethod(name, TOReflectionUtils.StaticBindingFlags, null, parameterTypes, null) ?? throw new MissingMethodException(type.FullName, name);
    /// <summary>
    /// 获取静态构造函数，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMethodException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ConstructorInfo GetStaticConstructor(Type type) => type.TypeInitializer ?? throw new MissingMethodException(type.FullName, ".cctor");
    /// <summary>
    /// 获取静态事件，找不到则抛出异常。
    /// </summary>
    /// <exception cref="MissingMemberException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static EventInfo GetStaticEvent(Type type, string name) => type.GetEvent(name, TOReflectionUtils.StaticBindingFlags) ?? throw new MissingMemberException(type.FullName, name);
}
