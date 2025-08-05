using Transoceanic.RuntimeEditing;

namespace Transoceanic.Core.Extensions;

public static partial class TOExtensions
{
    extension(ArgumentException)
    {
        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ThrowIfNullOrEmpty<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Count == 0)
                throw new ArgumentException($"Argument {paramName} cannot be empty.", paramName);
        }

        /// <summary>
        /// 当列表为 <see langword="null"/> 时抛出 <see cref="ArgumentNullException"/> 异常，不含任何元素或包含值为 <see langword="null"/> 的元素时抛出 <see cref="ArgumentException"/> 异常。
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public static void ThrowIfNullOrEmptyOrAnyNull<T>([NotNull] IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Count; i++)
                _ = argument[i] ?? throw new ArgumentException($"Argument {paramName} has a null element at [{i}].", paramName);
        }
    }

    extension(ArgumentOutOfRangeException)
    {
        /// <summary>
        /// 当传递的枚举值在枚举类型中未定义时抛出 <see cref="ArgumentOutOfRangeException"/>。
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static void ThrowIfNotDefined<TEnum>(TEnum argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where TEnum : Enum
        {
            if (!Enum.IsDefinedBetter(argument))
                throw new ArgumentOutOfRangeException(paramName, argument, $"Value {argument} is not defined in enum {typeof(TEnum).Name}.");
        }
    }

    extension(Enum)
    {
        /// <summary>
        /// 检查枚举值是否定义。
        /// <br/>十分高效。
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool IsDefinedBetter<TEnum>(TEnum value) where TEnum : Enum =>
            value.ToString()[0] is '+' or '-' or '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9';
    }

    extension(IList<Color> colors)
    {
        /// <summary>
        /// 在多个颜色间提供插值。
        /// </summary>
        /// <param name="ratio">插值比率。范围为 [0, 1]。</param>
        /// <returns></returns>
        public Color LerpMany(float ratio)
        {
            ArgumentException.ThrowIfNullOrEmpty(colors);

            switch (colors.Count)
            {
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

    extension<T>(IList<T> values)
    {
        public bool TryGetValue(int index, out T value)
        {
            if (index >= 0 && index < values.Count)
            {
                value = values[index];
                return true;
            }
            value = default;
            return false;
        }
    }

    extension(MethodBase method)
    {
        public T Attribute<T>(bool inherit = true) where T : Attribute => method.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();

        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => method.Attribute<T>(inherit) is not null;

        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = method.Attribute<T>(inherit)) is not null;
    }

    extension(MethodInfo method)
    {
        public string RealName => method.IsGenericMethod
            ? method.Name + "<" + string.Join(", ", method.GetGenericArguments().Select(t => t.Name)) + ">"
            : method.Name;

        public string RealNameWithParameter => method.RealName + "(" + string.Join(", ", method.ParameterTypes.Select(t => t.Name)) + ")";

        public string TypedName => method.DeclaringType.NestedName + method.Name;

        public string TypedRealName => method.DeclaringType.NestedRealName + method.RealName;

        public string TypedRealNameWithParameter => method.DeclaringType.NestedRealName + method.RealNameWithParameter;

        public Type[] ParameterTypes => [.. method.GetParameters().Select(p => p.ParameterType)];

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
            method.IsSpecialName ? (
                method.Name.StartsWith("add_") ? method.DeclaringType.GetEvent(method.Name[4..], TOReflectionUtils.UniversalBindingFlags)
                : method.Name.StartsWith("remove_") ? method.DeclaringType.GetEvent(method.Name[7..], TOReflectionUtils.UniversalBindingFlags)
                : null
            ) : null) is not null;

        public bool IsEventAccessor => method.TryGetEventAccessor(out _);

        /// <summary>
        /// 判定指定方法是否为真正的虚方法。
        /// <br/>具体判定逻辑：必须为虚方法，且不是重写方法或最终方法（sealed，用于判定逻辑上非虚的接口实现）。
        /// </summary>
        public bool IsRealVirtualOrAbstract => (method.IsVirtual && !method.IsOverride && !method.IsFinal) || method.IsAbstract;

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

    extension(Regex regex)
    {
        public bool TryMatch(string input, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            return (match = regex.Match(input)).Success;
        }
    }

    extension(Regex)
    {
        public static bool TryMatch(string input, string pattern, [NotNullWhen(true)] out Match match)
        {
            ArgumentNullException.ThrowIfNull(input);
            ArgumentNullException.ThrowIfNull(pattern);
            return (match = Regex.Match(input, pattern)).Success;
        }
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
            ? type.Name[..type.Name.IndexOf('`')] + "<" + string.Join(", ", type.GetGenericArguments().Select(a => a.Name)) + ">"
            : type.Name;

        public string NestedName => type.DeclaringType is not null ? type.DeclaringType.NestedName + "." + type.Name : type.Name; //递归

        public string NestedRealName => type.DeclaringType is not null ? type.DeclaringType.NestedRealName + "." + type.RealName : type.RealName; //递归

        public IEnumerable<string> GetMethodNames(BindingFlags bindingAttr) => type.GetMethods(bindingAttr).Select(m => m.Name);

        public IEnumerable<string> GetMethodNamesExceptObject(BindingFlags bindingAttr)
        {
            foreach (MethodInfo method in type.GetMethods(bindingAttr))
            {
                string name = method.Name;
                if (!TOReflectionUtils.ObjectMethods.Contains(name))
                    yield return name;
            }
        }

        /// <summary>
        /// 检查指定类型中是否存在对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            (methodInfo = type.GetMethod(methodName, bindingAttr)) is not null;

        /// <summary>
        /// 获取指定类型中所有由该类型声明的方法。
        /// </summary>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public MethodInfo[] GetRealMethods(BindingFlags bindingAttr) => type.GetMethods(bindingAttr | BindingFlags.DeclaredOnly);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="bindingAttr"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr, out MethodInfo methodInfo) =>
            type.HasMethod(methodName, bindingAttr | BindingFlags.DeclaredOnly, out methodInfo);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags bindingAttr) =>
            type.HasRealMethod(methodName, bindingAttr, out _);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="requiredMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="requiredMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr, out MethodInfo mainMethod, out MethodInfo requiredMethod) =>
            (type.HasRealMethod(mainMethodName, bindingAttr, out mainMethod), type.HasRealMethod(requiredMethodName, bindingAttr, out requiredMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethodName"/> 依赖于 <paramref name="requiredMethodName"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethodName"/> 存在而 <paramref name="requiredMethodName"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags bindingAttr) =>
            type.MustHaveRealMethodWith(mainMethodName, requiredMethodName, bindingAttr, out _, out _);

        /// <summary>
        /// 获取指定类型中所有重写方法。
        /// </summary>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetOverrideMethods(BindingFlags bindingAttr) => type.GetRealMethods(bindingAttr).Where(m => m.IsOverride);

        /// <summary>
        /// 获取指定类型中所有重写方法的名称。
        /// </summary>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags bindingAttr) => type.GetOverrideMethods(bindingAttr).Select(m => m.Name);

        public T Attribute<T>(bool inherit = true) where T : Attribute => type.GetCustomAttributes(typeof(T), inherit).OfType<T>().FirstOrDefault();

        public bool HasAttribute<T>(bool inherit = true) where T : Attribute => type.Attribute<T>(inherit) is not null;

        public bool TryGetAttribute<T>([NotNullWhen(true)] out T attribute, bool inherit = true) where T : Attribute => (attribute = type.Attribute<T>(inherit)) is not null;
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
