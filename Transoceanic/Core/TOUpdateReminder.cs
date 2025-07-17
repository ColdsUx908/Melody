namespace Transoceanic.Core;

public sealed class TOUpdateReminder : ModSystem, IResourceLoader
{
    internal static event Action UpdateReminder;

    public override void PreUpdateEntities()
    {
        if (TOWorld.GameTimer == 300 && UpdateReminder is not null)
        {
            TOLocalizationUtils.ChatLiteralText("[TRANSOCEANIC UPDATE REMINDER]", TOMain.TODebugWarnColor, Main.LocalPlayer);
            UpdateReminder();
            TOLocalizationUtils.ChatLiteralText("[Transoceanic] If you see this Message, please inform the developer.", TOMain.TODebugWarnColor, Main.LocalPlayer);
        }
    }
}
