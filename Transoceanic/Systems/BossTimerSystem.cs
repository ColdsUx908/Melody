using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using Transoceanic.Configs;

namespace Transoceanic.Systems;

public sealed class BossTimerSystem : ModSystem
{
    public static int Frames { get; private set; } = 0;
    public static int Minutes { get; private set; } = 0;
    public static int Seconds { get; private set; } = 0;
    public static int Ticks { get; private set; } = 0;

    public override void PostUpdateNPCs()
    {
        if (TOMain.BossActive)
            Frames++;
        else
            Frames = 0;
        Minutes = Frames / 3600;
        Seconds = Frames % 3600 / 60;
        Ticks = Frames % 60;
        return;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (Frames > 0 && TOUtilityConfig.Instance.BossTimer)
        {
            float xPos = (float)Math.Round(Main.screenWidth * TOUtilityConfig.Instance.XScale);
            float yPos = (float)Math.Round(Main.screenHeight * TOUtilityConfig.Instance.YScale);
            int bar = TOUtilityConfig.Instance.Bar;
            float size = TOUtilityConfig.Instance.Size;

            DefaultInterpolatedStringHandler stringhandler = new(1, 1);
            stringhandler.AppendFormatted(Minutes);
            stringhandler.AppendLiteral(Seconds < 10 ? ":0" : ":");
            stringhandler.AppendFormatted(Seconds);
            if (TOUtilityConfig.Instance.DisplayTicks)
            {
                stringhandler.AppendLiteral(Ticks < 10 ? ":0" : ":");
                stringhandler.AppendFormatted(Ticks);
            }
            string display = stringhandler.ToStringAndClear();

            Vector2 textSize = FontAssets.MouseText.Value.MeasureString(display);

            layers.Add(new LegacyGameInterfaceLayer("Timer", delegate
            {
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, display, new Vector2(xPos, yPos + bar), Color.Black, 0f, textSize / 2f, size, 0, 0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, display, new Vector2(xPos, yPos - bar), Color.Black, 0f, textSize / 2f, size, 0, 0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, display, new Vector2(xPos + bar, yPos), Color.Black, 0f, textSize / 2f, size, 0, 0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, display, new Vector2(xPos - bar, yPos), Color.Black, 0f, textSize / 2f, size, 0, 0f);
                DynamicSpriteFontExtensionMethods.DrawString(Main.spriteBatch, FontAssets.MouseText.Value, display, new Vector2(xPos, yPos), Color.White, 0f, textSize / 2f, size, 0, 0f);
                return true;
            }, InterfaceScaleType.None));
        }
    }
}