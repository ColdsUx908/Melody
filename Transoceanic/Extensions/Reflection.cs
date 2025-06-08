namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Type type)
    {
        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags, out MethodInfo methodInfo)
        {
            methodInfo = type.GetMethod(methodName, flags);
            return methodInfo is not null && methodInfo.DeclaringType == type;
        }

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags) =>
            type.HasRealMethod(methodName, flags, out _);

        /// <summary>
        /// 获取指定类型中所有由该类型声明的方法。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetRealMethods(BindingFlags flags) => type.GetMethods(flags).Where(k => k.DeclaringType == type);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="otherMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="otherMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string methodNameA, string methodNameB, BindingFlags flags, out MethodInfo mainMethod, out MethodInfo otherMethod) =>
            (type.HasRealMethod(methodNameA, flags, out mainMethod), type.HasRealMethod(methodNameB, flags, out otherMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="otherMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="otherMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethod, string otherMethod, BindingFlags flags) =>
            type.MustHaveRealMethodWith(mainMethod, otherMethod, flags, out _, out _);

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
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags flags) =>
            from MethodInfo method in type.GetOverrideMethods(flags)
            select method.IsGenericMethod
                     ? method.Name + "<" + string.Join(",", method.GetGenericArguments().Select(k => k.Name)) + ">"
                     : method.Name;
    }

    extension(MethodInfo method)
    {
        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool IsOverrideOf(Type baseType)
        {
            MethodInfo baseDefinition = method.GetBaseDefinition();
            return baseDefinition.DeclaringType == baseType &&
                   !baseDefinition.DeclaringType.IsInterface;
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
                return baseDefinition.DeclaringType != method.DeclaringType &&
                       !baseDefinition.DeclaringType.IsInterface;
            }
        }

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
        public bool IsInterfaceImplementationOf<T>() => method.IsInterfaceImplementationOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否实现了某个接口类型。
        /// </summary>
        /// <returns></returns>
        public bool IsInterfaceImplementation => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);
    }
}
