namespace CalamityAnomalies.Core;

public sealed class CAMessageSender : ModSystem
{
    public override void PreUpdateEntities()
    {
        if (TOWorld.GameTimer == 360)
            TOLocalizationUtils.ChatLocalizedText(CAMain.ModLocalizationPrefix + "Core.EnterWorldMessage." + (CAServerConfig.Instance.Contents ? "True" : "False"), CAMain.MainColor, Main.LocalPlayer);
    }
}
