namespace Transoceanic.Hooks;

public static class TOHookUtils
{
    public static Type GetTerrariaType(string typeName) =>
        TOMain.TerrariaTypes.FirstOrDefault(t => t.Name == typeName) ??
        throw new ArgumentException($"Type '{typeName}' not found in Terraria types.", nameof(typeName));
}
