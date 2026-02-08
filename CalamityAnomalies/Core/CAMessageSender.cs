using Transoceanic;

namespace CalamityAnomalies.Core;

public sealed class CAMessageSender : ModSystem
{
    public override void PreUpdateEntities()
    {
        if (TOSharedData.GameTimer == 360)
            TOLocalizationUtils.ChatLocalizedText(CASharedData.ModLocalizationPrefix + "Core.EnterWorldMessage.True", CASharedData.MainColor, Main.LocalPlayer);
    }
}
