using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalamityAnomalies.Contents.AnomalyNPCs;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.Utilities;
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
using Transoceanic.Core.ExtraData.Maths;
using Transoceanic.Core.GameData;
using Transoceanic.Core.IL;
using static CalamityMod.UI.BossHealthBarManager; //灾厄的嵌套类构造会使不用using static的代码十分丑陋

namespace CalamityAnomalies.IL;

public partial class CANPCHook : ITODetourProvider, ITOLoader
{
    /// <summary>
    /// 改进的Boss血条UI类。
    /// </summary>
    public class BetterBossHPUI : BossHPUI
    {
        public bool HasOneToMany { get; }

        public int[] CustomOneToManyIndexes { get; }

        /*
         * 这个字典是未完成的。
         * 在未来的异象模式中，Boss生成时会绑定自己的所有体节NPC。
         */
        public Dictionary<ulong, NPC> CustomOneToMany { get; } = [];

        public bool HasSpecialLifeRequirement { get; } = false;

        public NPCSpecialHPGetFunction HPGetFunction { get; } = null;

        public ulong Identifier { get; }

        public bool Valid { get; private set; } = true;

        public new NPC AssociatedNPC => Main.npc[NPCIndex]; //在新的实例化机制中，NPCIndex不可能越界

        public new int NPCType => AssociatedNPC.type;

        public new long CombinedNPCLife
        {
            get
            {
                if (AssociatedNPC == null || !AssociatedNPC.active)
                    return 0L;

                foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
                {
                    if (requirement(AssociatedNPC))
                        return func(AssociatedNPC, checkingForMaxLife: false);
                }

                long result = NPCType == NPCID.PirateShip ? 0L : AssociatedNPC.life;
                foreach ((ulong identifier, NPC npc) in CustomOneToMany)
                {
                    if (npc.Ocean().Identifier == identifier && npc.active && npc.life > 0)
                        result += npc.life;
                }
                return result;
            }
        }


        public new long CombinedNPCMaxLife
        {
            get
            {
                if (AssociatedNPC == null || !AssociatedNPC.active)
                    return 0L;

                foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
                {
                    if (requirement(AssociatedNPC))
                        return func(AssociatedNPC, checkingForMaxLife: true);
                }

                long result = NPCType == NPCID.PirateShip ? 0L : AssociatedNPC.lifeMax;
                foreach ((ulong identifier, NPC npc) in CustomOneToMany)
                {
                    if (npc.Ocean().Identifier == identifier && npc.active && npc.lifeMax > 0)
                        result += npc.lifeMax;
                }
                return result;
            }
        }

        public BetterBossHPUI(int npcIndex, string overridingName = null) : base(npcIndex, overridingName)
        {
            Identifier = AssociatedNPC.Ocean().Identifier;

            HasOneToMany = OneToMany.TryGetValue(NPCType, out int[] value);
            CustomOneToManyIndexes = value;

            foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
            {
                if (requirement(AssociatedNPC))
                {
                    HasSpecialLifeRequirement = true;
                    HPGetFunction = func;
                }
            }
        }

        public new void Update() => throw new NotImplementedException("BetterBossHPUI.Update() should not be called directly. Use Update(bool valid) instead.");

        public void Update(bool valid)
        {
            if (!(Valid = valid))
            {
                EnrageTimer = Math.Max(EnrageTimer - 4, 0);
                IncreasingDefenseOrDRTimer = Math.Max(EnrageTimer - 4, 0);
                CloseAnimationTimer = Math.Min(CloseAnimationTimer + 1, 120);
                return;
            }

            long combinedNPCLife = CombinedNPCLife;
            if (combinedNPCLife != PreviousLife && PreviousLife != 0L)
            {
                if (ComboDamageCountdown <= 0)
                    HealthAtStartOfCombo = combinedNPCLife;
                ComboDamageCountdown = 30;
            }
            PreviousLife = combinedNPCLife;

            if (ComboDamageCountdown > 0)
                ComboDamageCountdown--;

            CloseAnimationTimer = Math.Max(CloseAnimationTimer - 2, 0);
            OpenAnimationTimer = Math.Min(OpenAnimationTimer + 1, 120); //由80改为120
            EnrageTimer = Math.Min(EnrageTimer + NPCIsEnraged.ToDirectionInt(), 120);
            IncreasingDefenseOrDRTimer = Math.Min(IncreasingDefenseOrDRTimer + NPCIsIncreasingDefenseOrDR.ToDirectionInt(), 120);

            CustomOneToMany.Clear();
            if (HasOneToMany)
            {
                foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(k => CustomOneToManyIndexes.Contains(k.type)))
                    CustomOneToMany.Add(npc.Ocean().Identifier, npc);
            }

            long combinedNPCMaxLife = CombinedNPCMaxLife;
            if (combinedNPCMaxLife != 0L && (InitialMaxLife == 0L || InitialMaxLife < combinedNPCMaxLife))
                InitialMaxLife = combinedNPCMaxLife;
        }

        public new void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            //只在异象模式下生效。
            //后续可 能将强制异象判定移除。
            if (!CAWorld.Anomaly)
            {
                base.Draw(spriteBatch, x, y);
                return;
            }

            CAGlobalNPC anomalyNPC = AssociatedNPC.Anomaly();
            if (!anomalyNPC.IsRunningAnomalyAI)
            {
                base.Draw(spriteBatch, x, y);
                return;
            }

            bool registered = AnomalyNPCOverrideHelper.Registered(NPCType, out AnomalyNPCOverride anomalyNPCOverride);
            if (registered)
            {
                anomalyNPCOverride.PreDrawCalBossBar(this, spriteBatch, x, y);
                if (!anomalyNPCOverride.AllowOrigCalMethod(OrigCalMethodType.DrawBossBar))
                    return;
            }

            Color baseColor = new(240, 240, 255);
            DynamicSpriteFont mouseFont = FontAssets.MouseText.Value;
            DynamicSpriteFont itemStackFont = FontAssets.ItemStack.Value;

            float animationCompletionRatio = CloseAnimationTimer > 0
                ? 1f - MathHelper.Clamp(CloseAnimationTimer / 120f, 0f, 1f)
                : MathHelper.Clamp(OpenAnimationTimer / 80f, 0f, 1f);
            float animationCompletionRatio2 = CloseAnimationTimer > 0
                ? 1f - MathHelper.Clamp(CloseAnimationTimer / 80f, 0f, 1f)
                : MathHelper.Clamp(OpenAnimationTimer / 120f, 0f, 1f);

            #region 基础条
            float whiteColorAlpha = OpenAnimationTimer switch
            {
                4 or 8 or 16 => Main.rand.NextFloat(0.7f, 0.8f),
                3 or 7 or 15 => Main.rand.NextFloat(0.4f, 0.5f),
                _ => animationCompletionRatio
            };
            int mainBarWidth = (int)MathHelper.Min(400f * animationCompletionRatio, 400f * NPCLifeRatio);

            spriteBatch.Draw(BossMainHPBar, new Rectangle(x, y + 28, mainBarWidth, BossMainHPBar.Height), Color.White);

            if (ComboDamageCountdown > 0)
            {
                int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
                if (ComboDamageCountdown < 6)
                    comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;

                spriteBatch.Draw(BossComboHPBar, new Rectangle(x + mainBarWidth, y + 28, comboHPBarWidth, BossComboHPBar.Height), Color.White);
            }

            Color color = (CAWorld.Anomaly ? Color.Lerp(baseColor, Color.HotPink, Math.Max(anomalyNPC.AnomalyAITimer, 120) / 120f)
                : NPCIsEnraged ? Color.Lerp(baseColor, Color.Red * 0.5f, EnrageTimer / 120f)
                : NPCIsIncreasingDefenseOrDR ? Color.Lerp(baseColor, Color.LightGray * 0.5f, IncreasingDefenseOrDRTimer / 120f) : baseColor) * animationCompletionRatio;
            spriteBatch.Draw(BossSeperatorBar, new Rectangle(x, y + 18, 400, 6), color);
            #endregion

            //为了避免NPC名称过长遮挡大生命值数字，二者的绘制顺序在此处被调换了。

            #region NPC名称
            string npcName = OverridingName ?? AssociatedNPC.FullName;
            Vector2 npcNameSize = mouseFont.MeasureString(npcName);
            Vector2 baseDrawPosition = new(x + 400 - npcNameSize.X, y + 23 - npcNameSize.Y);
            float borderDelta = (MathF.Sin(Main.GlobalTimeWrappedHourly * 4.5f) + 1f) * 0.5f;
            if (CAWorld.AnomalyUltramundane && anomalyNPC.AnomalyUltraBarTimer > 0)
            {
                DrawBorderStringEightWay_Loop(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition,
                    Color.HotPink * 0.6f * animationCompletionRatio2,
                    Color.Red * 0.2f * animationCompletionRatio2,
                    Color.White * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2,
                    8,
                    anomalyNPC.AnomalyUltraBarTimer / 120f + borderDelta);
            }
            else if (NPCIsEnraged && EnrageTimer > 0)
            {
                DrawBorderStringEightWay_Loop(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition,
                    Color.Red * 0.6f * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2,
                    Color.White * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2,
                    8,
                    EnrageTimer / 80f + borderDelta * 2f);
            }
            else if (NPCIsIncreasingDefenseOrDR && IncreasingDefenseOrDRTimer > 0)
            {
                DrawBorderStringEightWay_Loop(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition,
                    Color.Red * 0.6f * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2,
                    Color.White * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2,
                    8,
                    EnrageTimer / 80f + borderDelta * 2f);
            }
            else
            {
                CalamityUtils.DrawBorderStringEightWay(
                    spriteBatch,
                    mouseFont,
                    npcName,
                    baseDrawPosition,
                    Color.White * animationCompletionRatio2,
                    Color.Black * 0.2f * animationCompletionRatio2);
            }
            #endregion

            #region 大生命值数字
            string bigLifeText = NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%";
            Vector2 bigLifeTextSize = HPBarFont.MeasureString(bigLifeText);
            CalamityUtils.DrawBorderStringEightWay(
                spriteBatch,
                HPBarFont,
                bigLifeText,
                new Vector2(x, y + 22 - bigLifeTextSize.Y),
                MainColor * animationCompletionRatio2,
                MainBorderColour * 0.25f * animationCompletionRatio2);
            #endregion

            #region 小生命值数字和额外信息
            if (!CanDrawExtraSmallText)
                return;
            string smallText = $"({CombinedNPCLife} / {InitialMaxLife})";
            if (EntityExtensionHandler.TryGetValue(NPCType, out BossHealthBarManager.BossEntityExtension extraEntityData))
            {
                string extentionName = extraEntityData.NameOfExtensions.ToString();
                int extraEntities = CalamityUtils.CountNPCsBetter(extraEntityData.TypesToSearchFor);
                string extentionText = $"({extentionName}: {extraEntities})";
                smallText = extentionText + " " + smallText;
            }
            CalamityUtils.DrawBorderStringEightWay(
                spriteBatch,
                itemStackFont,
                smallText,
                new Vector2(
                    (float)Math.Max(x, x + mainBarWidth - (itemStackFont.MeasureString(smallText) * 0.75f).X),
                    y + 45),
                Color.White * whiteColorAlpha,
                Color.Black * whiteColorAlpha * 0.24f,
                0.75f);
            #endregion

            if (registered)
                anomalyNPCOverride.PostDrawCalBossBar(this, spriteBatch, x, y);
        }
    }

    private static readonly Dictionary<ulong, BetterBossHPUI> _trackingBars = [];
    private const int MaxBars = 6;
    private const int MaxActiveBars = 4;

    internal static Type Type_BossHealthBarManager => typeof(BossHealthBarManager);

    internal static MethodInfo Method_BossHealthBarManager_Draw => Type_BossHealthBarManager.GetMethod("Draw", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_BossHealthBarManager_Update => Type_BossHealthBarManager.GetMethod("Update", TOMain.UniversalBindingFlags);

    internal delegate void Orig_BossHealthBarManager_Draw(BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info);
    internal delegate void Orig_BossHealthBarManager_Update(BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info);

    /// <summary>
    /// 灾厄Boss血条总控绘制钩子。
    /// </summary>
    /// <param name="orig"></param>
    /// <param name="self"></param>
    /// <param name="spriteBatch"></param>
    /// <param name="currentBar"></param>
    /// <param name="info"></param>
    internal static void On_BossHealthBarManager_Draw(Orig_BossHealthBarManager_Draw orig, BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        if (!CAWorld.Anomaly)
        {
            orig(self, spriteBatch, currentBar, info);
            return;
        }

        int x = Main.screenWidth -
            (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing ? 670 : 420);
        int y = Main.screenHeight - 100;

        foreach (BetterBossHPUI newBar in
            from pair in _trackingBars
            let newBar = pair.Value
            orderby newBar.Valid descending, pair.Key ascending
            select newBar)
        {
            newBar.Draw(spriteBatch, x, y);
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
    internal static void On_BossHealthBarManager_Update(Orig_BossHealthBarManager_Update orig, BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        if (!CAWorld.Anomaly)
        {
            orig(self, currentBar, ref info);
            return;
        }

        List<ulong> validIdentifiers = [];

        foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(k => !BossExclusionList.Contains(k.type)))
        {
            ulong fromNPC = npc.Ocean().Identifier;
            string overridingName = null;
            if (npc.type == ModContent.NPCType<Apollo>())
                overridingName = CalamityUtils.GetTextValue("UI.ExoTwinsName" + (npc.ModNPC<Apollo>().exoMechdusa ? "Hekate" : "Normal"));

            if (_trackingBars.ContainsKey(fromNPC) && npc.active)
                validIdentifiers.Add(fromNPC);
            else if (_trackingBars.Values.Count < MaxBars
                && _trackingBars.Values.Count(newBar => newBar.Valid) < MaxActiveBars
                && ((npc.IsABoss() && !(npc.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail || npc.type == ModContent.NPCType<Artemis>()))
                || MinibossHPBarList.Contains(npc.type) || npc.Calamity().CanHaveBossHealthBar))
                _trackingBars.Add(fromNPC, new(npc.whoAmI, overridingName));
        }

        foreach ((ulong identifier, BetterBossHPUI newBar) in _trackingBars)
        {
            newBar.Update(validIdentifiers.Contains(identifier));
            if (newBar.CloseAnimationTimer >= 120)
                _trackingBars.Remove(identifier);
        }
    }

    public static void DrawBorderStringEightWay_Loop(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition,
        Color mainColor, Color borderColor, Color mainColor2, Color borderColor2,
        int round, float borderWidth, float scale = 1f)
    {
        for (int i = 0; i < round; i++)
            CalamityUtils.DrawBorderStringEightWay(
                spriteBatch,
                font,
                text,
                baseDrawPosition + new PolarVector2(borderWidth, MathHelper.TwoPi / round * i),
                mainColor,
                borderColor,
                scale);
        CalamityUtils.DrawBorderStringEightWay(
            spriteBatch,
            font,
            text,
            baseDrawPosition,
            mainColor2,
            borderColor2,
            scale);
    }
}
