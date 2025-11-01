namespace Transoceanic.Core;

internal interface IUpdateReminder
{
    internal abstract Action RegisterUpdateReminder();
}

public interface IExternalUpdateReminder
{
    public abstract Action RegisterUpdateReminder();
}

public sealed class UpdateReminderHelper : ModSystem, ILocalizationPrefix, IResourceLoader
{
    private static event Action UpdateReminder;
    private static event Action ExternalUpdateReminder;

    public string LocalizationPrefix => TOMain.ModLocalizationPrefix + "Core.UpdateReminder";

    public override void PreUpdateEntities()
    {
        if (TOWorld.GameTimer == 180 && (UpdateReminder is not null || ExternalUpdateReminder is not null))
        {
            TOLocalizationUtils.ChatLiteralText($"[{this.GetTextValue("Header")}]", TOMain.TODebugWarnColor, Main.LocalPlayer);
            UpdateReminder?.Invoke();
            if (ExternalUpdateReminder is not null)
            {
                TOLocalizationUtils.ChatLiteralText(this.GetTextValue("ExternalHeader"), TOMain.TODebugWarnColor, Main.LocalPlayer);
                ExternalUpdateReminder();
            }
            TOLocalizationUtils.ChatLiteralText($"[Transoceanic] {this.GetTextValue("Message")}", TOMain.TODebugWarnColor, Main.LocalPlayer);
        }
    }

    void IResourceLoader.PostSetupContent()
    {
        foreach (IUpdateReminder updateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IUpdateReminder>())
            UpdateReminder += updateReminder.RegisterUpdateReminder();
        foreach (IExternalUpdateReminder externalUpdateReminder in TOReflectionUtils.GetTypeInstancesDerivedFrom<IExternalUpdateReminder>())
            ExternalUpdateReminder += externalUpdateReminder.RegisterUpdateReminder();
    }

    void IResourceLoader.OnModUnload() => UpdateReminder = null;
}
