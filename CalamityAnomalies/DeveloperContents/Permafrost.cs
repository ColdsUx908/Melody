using System;
using System.Collections.Generic;
using CalamityAnomalies.UI;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Transoceanic;
using Transoceanic.Drawing;
using Transoceanic.MathHelp;

namespace CalamityAnomalies.Developer;

public sealed class Permafrost : CANPCOverride<SupremeCalamitas>
{
    private static Color BlueColor => Color.Lerp(Color.LightCyan, Color.Blue, TOMathHelper.GetTimeSin(0.35f, 1f, 0f, true));
    private static List<Color> NameColors { get; } =
    [
        Color.LightYellow,
        Color.Cyan,
        Color.SkyBlue,
        Color.LightPink,
        Color.LightSkyBlue
    ];

    public override decimal Priority => 935m;

    public override bool ShouldProcess => ModNPC.permafrost;

    public override bool PreDrawCalBossBar(BetterBossHealthBarManager.BetterBossHPUI newBar, SpriteBatch spriteBatch, int x, int y)
    {
        newBar.DrawBaseBars(spriteBatch, x, y, null, BlueColor);
        newBar.DrawNPCName(spriteBatch, x, y, null,
            BlueColor,
            NameColors.LerpMany(TOMathHelper.GetTimeSin(5f, 0.8f, 0f, true)),
            Math.Clamp(OceanNPC.ActiveTime, 0f, 120f) / 80f + TOMathHelper.GetTimeSin(1f, 1f, 0f, true));
        newBar.DrawBigLifeText(spriteBatch, x, y);
        newBar.DrawExtraSmallText(spriteBatch, x, y);

        return false;
    }
}
