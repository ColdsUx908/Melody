using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityAnomalies.Systems;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Transoceanic;
using Transoceanic.Core;
using Transoceanic.Data;

namespace CalamityAnomalies.NPCs;


public partial class CANPCHook : ITODetourProvider, ITOLoader
{
    internal static Dictionary<ulong, BossHealthBarManager.BossHPUI> _trackingBars = [];

    public static MethodInfo CalBossBarUIDraw => typeof(BossHealthBarManager.BossHPUI).GetMethod("Draw", TOMain.UniversalBindingFlags);
    public static MethodInfo CalBossBarUIUpdate => typeof(BossHealthBarManager.BossHPUI).GetMethod("Update", TOMain.UniversalBindingFlags);
    public static MethodInfo CalBossBarDraw => typeof(BossHealthBarManager).GetMethod("Draw", TOMain.UniversalBindingFlags);
    public static MethodInfo CalBossBarUpdate => typeof(BossHealthBarManager).GetMethod("Update", TOMain.UniversalBindingFlags);

    public delegate void Orig_CalBossBarUIDraw(BossHealthBarManager.BossHPUI self, SpriteBatch spriteBatch, int x, int y);
    public delegate void Orig_CalBossBarUIUpdate(BossHealthBarManager.BossHPUI self);
    public delegate void Orig_CalBossBarDraw(BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info);
    public delegate void Orig_CalBossBarUpdate(BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info);

    /// <summary>
    /// 灾厄Boss血条绘制钩子。
    /// <br/>顺便修复了一些问题。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    internal static void CalBossBarUIDrawDetour(Orig_CalBossBarUIDraw orig, BossHealthBarManager.BossHPUI self, SpriteBatch spriteBatch, int x, int y)
    {
        //只在异象模式下生效。
        //后续可 能将强制异象判定移除。
        if (!CAWorld.Anomaly)
        {
            orig(self, spriteBatch, x, y);
            return;
        }

        NPC loc_AssociatedNPC = self.AssociatedNPC;
        if (loc_AssociatedNPC is null)
            return;

        CAGlobalNPC anomalyNPC = loc_AssociatedNPC.Anomaly();
        if (!anomalyNPC.IsRunningAnomalyAI)
        {
            orig(self, spriteBatch, x, y);
            return;
        }

        int loc_NPCType = self.NPCType;
        long loc_CombinedNPCLife = self.CombinedNPCLife;
        long loc_InitialMaxLife = self.InitialMaxLife;
        int loc_OpenAnimationTimer = self.OpenAnimationTimer;
        int loc_CloseAnimationTimer = self.CloseAnimationTimer;
        float loc_NPCLifeRatio = self.NPCLifeRatio;
        long loc_HealthAtStartOfCombo = self.HealthAtStartOfCombo;
        int loc_ComboDamageCountdown = self.ComboDamageCountdown;
        string loc_OverridingName = self.OverridingName;
        Color loc_MainColor = BossHealthBarManager.BossHPUI.MainColor;
        Color loc_MainBorderColour = BossHealthBarManager.BossHPUI.MainBorderColour;

        //针对荷兰飞盗船的特殊处理（灾厄对荷兰飞盗船的生命值计算有误）
        if (loc_NPCType == NPCID.PirateShip)
        {
            loc_CombinedNPCLife -= loc_AssociatedNPC.life;
            loc_InitialMaxLife -= loc_AssociatedNPC.lifeMax;
        }

        Color baseColor = new(240, 240, 255);
        DynamicSpriteFont mouseFont = FontAssets.MouseText.Value;
        DynamicSpriteFont itemStackFont = FontAssets.ItemStack.Value;

        #region 基础条
        float animationCompletionRatio = loc_CloseAnimationTimer > 0
            ? 1f - MathHelper.Clamp(loc_CloseAnimationTimer / 120f, 0f, 1f)
            : MathHelper.Clamp(loc_OpenAnimationTimer / 80f, 0f, 1f);
        float animationCompletionRatio2 = loc_CloseAnimationTimer > 0
            ? 1f - MathHelper.Clamp(loc_CloseAnimationTimer / 80f, 0f, 1f)
            : MathHelper.Clamp(loc_OpenAnimationTimer / 120f, 0f, 1f);
        float whiteColorAlpha = loc_OpenAnimationTimer switch
        {
            4 or 8 or 16 => Main.rand.NextFloat(0.7f, 0.8f),
            3 or 7 or 15 => Main.rand.NextFloat(0.4f, 0.5f),
            _ => animationCompletionRatio
        };
        int mainBarWidth = (int)MathHelper.Min(400f * animationCompletionRatio, 400f * loc_NPCLifeRatio);

        spriteBatch.Draw(BossHealthBarManager.BossMainHPBar, new Rectangle(x, y + 28, mainBarWidth, BossHealthBarManager.BossMainHPBar.Height), Color.White);

        if (loc_ComboDamageCountdown > 0)
        {
            int comboHPBarWidth = (int)(400 * (float)loc_HealthAtStartOfCombo / loc_InitialMaxLife) - mainBarWidth;
            if (loc_ComboDamageCountdown < 6)
                comboHPBarWidth = comboHPBarWidth * loc_ComboDamageCountdown / 6;

            spriteBatch.Draw(BossHealthBarManager.BossComboHPBar, new Rectangle(x + mainBarWidth, y + 28, comboHPBarWidth, BossHealthBarManager.BossComboHPBar.Height), Color.White);
        }

        Color color = (CAWorld.Anomaly ? Color.Lerp(baseColor, Color.HotPink, Math.Max(anomalyNPC.AnomalyAITimer, 120) / 120f) :
            self.NPCIsEnraged ? Color.Lerp(baseColor, Color.Red * 0.5f, self.EnrageTimer / 120f) :
            self.NPCIsIncreasingDefenseOrDR ? Color.Lerp(baseColor, Color.LightGray * 0.5f, self.IncreasingDefenseOrDRTimer / 120f) : baseColor)
            * animationCompletionRatio;
        spriteBatch.Draw(BossHealthBarManager.BossSeperatorBar, new Rectangle(x, y + 18, 400, 6), color);
        #endregion

        //为了避免NPC名称过长遮挡大生命值数字，二者的绘制顺序在此处被调换了。

        #region NPC名称
        string npcName = loc_OverridingName ?? loc_AssociatedNPC.FullName;
        Vector2 npcNameSize = mouseFont.MeasureString(npcName);
        Vector2 baseDrawPosition = new(x + 400 - npcNameSize.X, y + 23 - npcNameSize.Y);
        float borderDelta = (MathF.Sin(Main.GlobalTimeWrappedHourly * 4.5f) + 1f) * 0.5f;
        float borderWidth;
        if (CAWorld.AnomalyUltramundane && anomalyNPC.AnomalyUltraBarTimer > 0)
        {
            borderWidth = +anomalyNPC.AnomalyUltraBarTimer / 120f + borderDelta;
            for (int i = 0; i < 8; i++)
            {
                CalamityUtils.DrawBorderStringEightWay(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition + new PolarVector2(borderWidth, MathHelper.PiOver4 * i),
                    Color.HotPink * 0.6f * animationCompletionRatio2,
                    Color.Red * 0.2f * animationCompletionRatio2);
            }
        }
        else if (self.NPCIsEnraged)
        {
            if (self.EnrageTimer > 0)
            {
                borderWidth = self.EnrageTimer / 80f + borderDelta * 2f;
                for (int i = 0; i < 4; i++)
                {
                    CalamityUtils.DrawBorderStringEightWay(
                        spriteBatch,
                        mouseFont,
                        npcName,
                        baseDrawPosition + (Vector2)new PolarVector2(borderWidth, MathHelper.PiOver2 * i),
                        Color.Red * 0.6f * animationCompletionRatio2,
                        Color.Black * 0.2f * animationCompletionRatio2);
                }
            }
        }
        else if (self.NPCIsIncreasingDefenseOrDR && self.IncreasingDefenseOrDRTimer > 0)
        {
            borderWidth = self.IncreasingDefenseOrDRTimer / 80f + borderDelta * 2f;
            for (int i = 0; i < 4; i++)
            {
                CalamityUtils.DrawBorderStringEightWay(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition + (Vector2)new PolarVector2(borderWidth, MathHelper.PiOver2 * i),
                    Color.LightGray * 0.6f * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2);
            }
        }
        CalamityUtils.DrawBorderStringEightWay(
        spriteBatch,
        mouseFont,
        npcName,
            baseDrawPosition,
            Color.White * animationCompletionRatio2,
            Color.Black * 0.2f * animationCompletionRatio2);
        #endregion

        #region 大生命值数字
        string bigLifeText = loc_NPCLifeRatio == 0f ? "0%" : (loc_NPCLifeRatio * 100f).ToString("N1") + "%";
        Vector2 bigLifeTextSize = BossHealthBarManager.HPBarFont.MeasureString(bigLifeText);
        CalamityUtils.DrawBorderStringEightWay(
            spriteBatch,
            BossHealthBarManager.HPBarFont,
            bigLifeText,
            new Vector2(x, y + 22 - bigLifeTextSize.Y),
            loc_MainColor,
            loc_MainBorderColour * 0.25f * animationCompletionRatio);
        #endregion

        #region 小生命值数字和额外信息
        if (!BossHealthBarManager.CanDrawExtraSmallText)
            return;
        string smallText = $"({loc_CombinedNPCLife} / {loc_InitialMaxLife})";
        if (BossHealthBarManager.EntityExtensionHandler.TryGetValue(loc_NPCType, out BossHealthBarManager.BossEntityExtension extraEntityData))
        {
            string extentionName = extraEntityData.NameOfExtensions.ToString();
            int extraEntities = CalamityUtils.CountNPCsBetter(extraEntityData.TypesToSearchFor);
            string extentionText = $"({extentionName}: {extraEntities})";
            smallText = extentionText + " " + smallText;
        }
        float x2 = Math.Max(x, x + mainBarWidth - (itemStackFont.MeasureString(smallText) * 0.75f).X);
        float y2 = y + 45;
        CalamityUtils.DrawBorderStringEightWay(
            spriteBatch,
            itemStackFont,
            smallText,
            new Vector2(x2, y2),
            Color.White * whiteColorAlpha,
            Color.Black * whiteColorAlpha * 0.24f,
            0.75f);
        #endregion
    }

    /// <summary>
    /// 灾厄Boss血条更新钩子。
    /// <br/>仅提供提前返回机制。
    /// <br/>主要机制在 <see cref="CalBossBarUIUpdateNew(BossHealthBarManager.BossHPUI, bool)"/> 中实现。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    internal static void CalBossBarUIUpdateDetour(Orig_CalBossBarUIUpdate orig, BossHealthBarManager.BossHPUI self)
    {
        if (CAWorld.Anomaly)
            return;

        orig(self);
    }

    /// <summary>
    /// 重新实现的Boss血条更新方法。
    /// </summary>
    /// <param name="self"></param>
    /// <param name="isValid"></param>
    internal static void CalBossBarUIUpdateNew(BossHealthBarManager.BossHPUI self, bool isValid)
    {
        if (!isValid)
        {
            self.EnrageTimer = Math.Max(self.EnrageTimer - 4, 0);
            self.IncreasingDefenseOrDRTimer = Math.Max(self.EnrageTimer - 4, 0);
            self.CloseAnimationTimer = Math.Min(self.CloseAnimationTimer + 1, 120);
            return;
        }

        if (self.CombinedNPCLife != self.PreviousLife && self.PreviousLife != 0L)
        {
            if (self.ComboDamageCountdown <= 0)
                self.HealthAtStartOfCombo = self.CombinedNPCLife;
            self.ComboDamageCountdown = 30;
        }

        self.PreviousLife = self.CombinedNPCLife;

        if (self.ComboDamageCountdown > 0)
            self.ComboDamageCountdown--;

        self.CloseAnimationTimer = Math.Max(self.CloseAnimationTimer - 2, 0);
        self.OpenAnimationTimer = Math.Min(self.OpenAnimationTimer + 1, 120); //由80改为120
        self.EnrageTimer = Math.Min(self.EnrageTimer + self.NPCIsEnraged.ToDirectionInt(), 120);
        self.IncreasingDefenseOrDRTimer = Math.Min(self.IncreasingDefenseOrDRTimer + self.NPCIsIncreasingDefenseOrDR.ToDirectionInt(), 120);
        if (self.CombinedNPCMaxLife != 0L && (self.InitialMaxLife == 0L || self.InitialMaxLife < self.CombinedNPCMaxLife))
            self.InitialMaxLife = self.CombinedNPCMaxLife;
    }

    /// <summary>
    /// 灾厄Boss血条总控绘制钩子。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="currentBar"></param>
    /// <param name="info"></param>
    internal static void CalBossBarDrawDetour(Orig_CalBossBarDraw orig, BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        if (!CAWorld.Anomaly)
        {
            orig(self, spriteBatch, currentBar, info);
            return;
        }

        int x = Main.screenWidth - 420;
        int y = Main.screenHeight - 100;
        if (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing)
            x -= 250;

        foreach ((_, BossHealthBarManager.BossHPUI bar) in _trackingBars)
        {
            bar.Draw(spriteBatch, x, y);
            y -= 70;
        }
    }

    /// <summary>
    /// 灾厄Boss血条总控更新钩子。
    /// <br/>修复了“卡血条”问题。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="currentBar"></param>
    /// <param name="info"></param>
    internal static void CalBossBarUpdateDetour(Orig_CalBossBarUpdate orig, BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        if (!CAWorld.Anomaly)
        {
            orig(self, currentBar, ref info);
            return;
        }

        HashSet<ulong> validIdentifiers = [];

        foreach (NPC npc in TOMain.ActiveNPCs)
        {
            if (BossHealthBarManager.BossExclusionList.Contains(npc.type))
                continue;

            ulong fromNPC = npc.Ocean().Identifier;
            bool flag = npc.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail || npc.type == ModContent.NPCType<Artemis>();
            string overridingName = null;
            if (npc.type == ModContent.NPCType<Apollo>())
                overridingName = CalamityUtils.GetTextValue("UI.ExoTwinsName" + (npc.ModNPC<Apollo>().exoMechdusa ? "Hekate" : "Normal"));

            if (_trackingBars.ContainsKey(fromNPC))
                validIdentifiers.Add(fromNPC);
            else if (_trackingBars.Count < BossHealthBarManager.MaximumBars &&
                ((npc.IsABoss() && !flag) || BossHealthBarManager.MinibossHPBarList.Contains(npc.type) || npc.Calamity().CanHaveBossHealthBar))
                _trackingBars.Add(fromNPC, new(npc.whoAmI, overridingName));
        }

        foreach ((ulong identifier, BossHealthBarManager.BossHPUI bar) in _trackingBars)
        {
            bar.Update();
            CalBossBarUIUpdateNew(bar, validIdentifiers.Contains(identifier));
            if (bar.CloseAnimationTimer >= 120)
                _trackingBars.Remove(identifier);
        }
    }
}
