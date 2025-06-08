namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty<T>(T[] argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Length == 0)
                throw new ArgumentException($"Array {paramName} cannot be empty.", paramName);
        }

        public static void ThrowIfNullOrEmptyOrAnyNull<T>(T[] argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Length; i++)
                _ = argument[i] ?? throw new ArgumentException($"Array {paramName} has a null element at [{i}].", paramName);
        }
    }
}