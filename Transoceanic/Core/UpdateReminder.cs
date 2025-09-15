using Transoceanic.Core.Utilities;

namespace Transoceanic.Core;

internal interface IUpdateReminder
{
    public abstract Action RegisterUpdateReminder();
}

public sealed class UpdateReminderHelper : ModSystem, IResourceLoader
{
    private static event Action UpdateReminder;

    public override void PreUpdateEntities()
    {
        if (TOWorld.GameTimer == 300 && UpdateReminder is not null)
        {
            TOLocalizationUtils.ChatLiteralText("[TRANSOCEANIC UPDATE REMINDER]", TOMain.TODebugWarnColor, Main.LocalPlayer);
            UpdateReminder();
            TOLocalizationUtils.ChatLiteralText("[Transoceanic] If you see this Message, please inform the developer.", TOMain.TODebugWarnColor, Main.LocalPlayer);
        }
    }

    void IResourceLoader.PostSetupContent()
    {
        foreach (IUpdateReminder updateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IUpdateReminder>())
        {
            Action action = updateReminder.RegisterUpdateReminder();
            if (action is not null)
                UpdateReminder += action;
        }
    }

    void IResourceLoader.OnModUnload() => UpdateReminder = null;
}
