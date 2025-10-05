namespace Transoceanic.Core;

internal interface IUpdateReminder
{
    public abstract Action RegisterUpdateReminder();
}

public sealed class UpdateReminderHelper : ModSystem, ILocalizationPrefix, IResourceLoader
{
    private static event Action UpdateReminder;

    public string LocalizationPrefix => TOMain.ModLocalizationPrefix + "Core.UpdateReminder";

    public override void PreUpdateEntities()
    {
        if (TOWorld.GameTimer == 180 && UpdateReminder is not null)
        {
            TOLocalizationUtils.ChatLiteralText($"[{this.GetTextValue("Header")}]", TOMain.TODebugWarnColor, Main.LocalPlayer);
            UpdateReminder();
            TOLocalizationUtils.ChatLiteralText($"[Transoceanic] {this.GetTextValue("Message")}", TOMain.TODebugWarnColor, Main.LocalPlayer);
        }
    }

    void IResourceLoader.PostSetupContent()
    {
        foreach (IUpdateReminder updateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IUpdateReminder>())
            UpdateReminder += updateReminder.RegisterUpdateReminder();
    }

    void IResourceLoader.OnModUnload() => UpdateReminder = null;
}
