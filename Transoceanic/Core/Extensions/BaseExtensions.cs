namespace Transoceanic.Core.Extensions;

public static partial class TOExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty<T>(IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Count == 0)
                throw new ArgumentException($"Argument {paramName} cannot be empty.", paramName);
        }

        public static void ThrowIfNullOrEmptyOrAnyNull<T>(IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Count; i++)
                _ = argument[i] ?? throw new ArgumentException($"Argument {paramName} has a null element at [{i}].", paramName);
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        public static void ThrowIfNotDefined<TEnum>(TEnum argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where TEnum : Enum
        {
            if (Enum.IsDefinedBetter(argument))
                throw new ArgumentOutOfRangeException(paramName, argument, $"Value {argument} is not defined in enum {typeof(TEnum).Name}.");
        }
    }

    extension(Enum)
    {
        public static bool IsDefinedBetter<TEnum>(TEnum value) where TEnum : Enum =>
            value.ToString()[0] is '+' or '-' or '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';
    }

    extension(IList<Color> colors)
    {
        public Color LerpMany(float ratio)
        {
            ArgumentNullException.ThrowIfNull(colors, nameof(colors));

            ratio = Math.Clamp(ratio, 0f, colors.Count - 1);

            switch (colors.Count)
            {
                case 0:
                    return Color.White;
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], ratio);
                default:
                    if (ratio <= 0f)
                        return colors[0];
                    if (ratio >= 1f)
                        return colors[^1];
                    (int index, float localRatio) = TOMathHelper.SplitFloat(Math.Clamp(ratio * (colors.Count - 1), 0f, colors.Count - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }

    extension(MethodBase method)
    {
        public bool HasAttribute<T>() where T : Attribute => method.GetAttribute<T>() is not null;
    }

    extension(MethodInfo method)
    {
        public string FullName => method.IsGenericMethod
            ? method.Name + "<" + string.Join(",", method.GetGenericArguments().Select(k => k.Name)) + ">"
            : method.Name;

        public Type[] ParameterTypes => [.. method.GetParameters().Select(k => k.ParameterType)];

        public MethodAttributes AccessLevel => method.Attributes & MethodAttributes.MemberAccessMask;

        public bool CanBeAccessedOutsideAssembly => method.AccessLevel is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool IsOverrideOf(Type baseType)
        {
            MethodInfo baseDefinition = method.GetBaseDefinition();
            return baseDefinition.DeclaringType == baseType && !baseDefinition.DeclaringType.IsInterface;
        }

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsOverrideOf<T>() => method.IsOverrideOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否为某个基类型的重写方法。
        /// </summary>
        /// <returns></returns>
        public bool IsOverride
        {
            get
            {
                MethodInfo baseDefinition = method.GetBaseDefinition();
                return baseDefinition.DeclaringType != method.DeclaringType && !baseDefinition.DeclaringType.IsInterface;
            }
        }

        public bool TryGetPropertyAccessor(out PropertyInfo property) => (property =
            method.IsSpecialName && (method.Name.StartsWith("get_") || method.Name.StartsWith("set_"))
            ? method.DeclaringType.GetProperty(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
            : null) is not null;

        public bool IsPropertyAccessor => method.TryGetPropertyAccessor(out _);

        public bool TryGetEventAccessor(out EventInfo eventInfo) => (eventInfo =
            method.IsSpecialName && (method.Name.StartsWith("add_") || method.Name.StartsWith("remove_"))
            ? method.DeclaringType.GetEvent(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
            : null) is not null;

        /// <summary>
        /// 判定指定方法是否为真正的虚方法。
        /// <br/>具体判定逻辑：必须为虚方法，且不是重写方法或最终方法（sealed元数据，用于判定逻辑上非虚的接口实现）。
        /// </summary>
        public bool IsRealVirtualOrAbstract => method.IsVirtual && !method.IsOverride && !method.IsFinal;

        public bool IsRealVirtual => method.IsRealVirtualOrAbstract && !method.IsAbstract;

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public bool IsInterfaceImplementationOf(Type interfaceType)
        {
            InterfaceMapping map;
            try
            {
                map = method.DeclaringType.GetInterfaceMap(interfaceType);
            }
            catch (NotSupportedException)
            {
                return false;
            }

            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.TargetMethods[i] == method)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsInterfaceImplementationOf<T>() where T : class => method.IsInterfaceImplementationOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否实现了某个接口类型。
        /// </summary>
        /// <returns></returns>
        public bool IsInterfaceImplementation => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);

        public bool IsExplicitInterfaceImplementation => method.IsInterfaceImplementation && method.Name.Contains('.');

        public bool IsImplicitInterfaceImplementation => method.IsInterfaceImplementation && !method.Name.Contains('.');
    }

    extension(StringBuilder builder)
    {
        public void AppendLocalizedLine(string key) => builder.AppendLine(Language.GetTextValue(key));

        public void AppendLocalizedLineWith(string key, params object[] args) => builder.AppendLine(Language.GetTextFormat(key, args));

        public void AppendTODebugErrorMessage()
        {
            builder.Append(Environment.NewLine);
            builder.AppendLocalizedLine(TOMain.DebugErrorMessageKey);
        }
    }

    extension(Type type)
    {
        public string RealName => type.IsGenericType
            ? type.Name[..type.Name.IndexOf('`')] + "<" + string.Join(", ", type.GetGenericArguments().Select(k => k.Name)) + ">"
            : type.Name;

        /// <summary>
        /// 检查指定类型中是否存在对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasMethod(string methodName, BindingFlags flags, out MethodInfo methodInfo) =>
            (methodInfo = type.GetMethod(methodName, flags)) is not null;

        /// <summary>
        /// 获取指定类型中所有由该类型声明的方法。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public MethodInfo[] GetRealMethods(BindingFlags flags) => type.GetMethods(flags | BindingFlags.DeclaredOnly);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags, out MethodInfo methodInfo) =>
            type.HasMethod(methodName, flags | BindingFlags.DeclaredOnly, out methodInfo);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags) =>
            type.HasRealMethod(methodName, flags, out _);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="requiredMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="requiredMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags flags, out MethodInfo mainMethod, out MethodInfo requiredMethod) =>
            (type.HasRealMethod(mainMethodName, flags, out mainMethod), type.HasRealMethod(requiredMethodName, flags, out requiredMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethodName"/> 依赖于 <paramref name="requiredMethodName"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethodName"/> 存在而 <paramref name="requiredMethodName"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags flags) =>
            type.MustHaveRealMethodWith(mainMethodName, requiredMethodName, flags, out _, out _);

        /// <summary>
        /// 获取指定类型中所有重写方法。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetOverrideMethods(BindingFlags flags) => type.GetRealMethods(flags).Where(k => k.IsOverride);

        /// <summary>
        /// 获取指定类型中所有重写方法的名称。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags flags) => type.GetOverrideMethods(flags).Select(k => k.Name);
    }

    extension(Vector2 vector)
    {
        public void Deconstruct(out float x, out float y)
        {
            x = vector.X;
            y = vector.Y;
        }

        /// <summary>
        /// 获取向量的顺时针旋转角。
        /// </summary>
        /// <returns>零向量返回0，否则返回 [0, 2π) 范围内的浮点值。</returns>
        public float Angle => vector.Y switch
        {
            > 0f => MathF.Atan2(vector.Y, vector.X),
            0f => vector.X switch
            {
                >= 0f => 0f, //零向量返回0，方向为x轴正方向返回0
                _ => MathHelper.Pi //方向为x轴负方向返回Pi
            },
            _ => MathHelper.TwoPi + MathF.Atan2(vector.Y, vector.X), //将Atan2方法返回的负值转换为正值
        };

        /// <summary>
        /// 安全地将向量化为单位向量。
        /// </summary>
        /// <returns>零向量返回零向量，否则返回单位向量。</returns>
        public Vector2 SafelyNormalized => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取模为特定值的原向量同向向量。不改变原向量值。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Vector2 ToCustomLength(float length) => vector.SafelyNormalized * length;
    }

    extension(ref Vector2 vector)
    {
        public void CopyFrom(Vector2 other)
        {
            vector.X = other.X;
            vector.Y = other.Y;
        }

        public float Modulus
        {
            get => vector.Length();
            set => vector.CopyFrom(vector.ToCustomLength(value));
        }
    }

    extension(Vector2)
    {
        /// <summary>
        /// 计算两个向量的夹角。
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static float IncludedAngle(Vector2 value1, Vector2 value2) => (float)Math.Acos(Vector2.Dot(value1, value2) / (value1.Modulus * value2.Modulus));

        /// <summary>
        /// 获取两个向量角平分线的单位方向向量。
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static Vector2 UnitAngleBisector(Vector2 value1, Vector2 value2) => new PolarVector2((value1.Angle + value2.Angle) / 2);
    }
}
