using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CalamityAnomalies.Contents.AnomalyMode.NPCs;
using CalamityAnomalies.Contents.BossRushMode;
using CalamityAnomalies.GlobalInstances;
using CalamityAnomalies.Utilities;
using CalamityMod;
using CalamityMod.Events;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
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
using static CalamityMod.UI.BossHealthBarManager;

namespace CalamityAnomalies.IL;

/// <summary>
/// 钩子。
/// <br/>应用类：<see cref="BossHealthBarManager"/>
/// </summary>
public class CalBossBarDetour : ITODetourProvider, ITOLoader
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

        public CAGlobalNPC AssociatedAnomalyNPC => field ??= AssociatedNPC.Anomaly();

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
                EnrageTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                IncreasingDefenseOrDRTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                CloseAnimationTimer++;
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

            CloseAnimationTimer = Math.Clamp(CloseAnimationTimer - 2, 0, 120);
            OpenAnimationTimer = Math.Clamp(OpenAnimationTimer + 1, 0, 120); //由80改为120
            EnrageTimer = Math.Clamp(EnrageTimer + NPCIsEnraged.ToDirectionInt(), 0, 120);
            IncreasingDefenseOrDRTimer = Math.Clamp(IncreasingDefenseOrDRTimer + NPCIsIncreasingDefenseOrDR.ToDirectionInt(), 0, 120);

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

            Color color = (CAWorld.Anomaly ? Color.Lerp(baseColor, Color.HotPink, Math.Clamp(AssociatedAnomalyNPC.AnomalyAITimer / 120f, 0f, 1f))
                : CAWorld.BossRush ? Color.Lerp(baseColor, Color.Lerp(BossRushMode.BossRushModeColor, Color.Red, animationCompletionRatio * 0.4f), Math.Clamp(AssociatedAnomalyNPC.BossRushAITimer / 120f, 0f, 1f))
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
            if (CAWorld.AnomalyUltramundane && AssociatedAnomalyNPC.AnomalyUltraBarTimer > 0)
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
                    AssociatedAnomalyNPC.AnomalyUltraBarTimer / 120f + borderDelta);
            }
            else if (CAWorld.BossRush)
            {
                DrawBorderStringEightWay_Loop(
                spriteBatch,
                mouseFont,
                npcName,
                baseDrawPosition,
                Color.Lerp(BossRushMode.BossRushModeColor, Color.Red, animationCompletionRatio * 0.45f) * animationCompletionRatio2,
                Color.Lerp(BossRushMode.BossRushModeColor, Color.Red, animationCompletionRatio * 0.55f) * animationCompletionRatio2,
                Color.White * animationCompletionRatio2,
                Color.Black * 0.2f * animationCompletionRatio2,
                8,
                Math.Clamp(AssociatedAnomalyNPC.BossRushAITimer, 0f, 120f) / 120f + borderDelta * 2f);
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

    internal static Type Type_BossHealthBarManager { get; } = typeof(BossHealthBarManager);

    internal static MethodInfo Method_BossHealthBarManager_Draw { get; } = Type_BossHealthBarManager.GetMethod("Draw", TOMain.UniversalBindingFlags);
    internal static MethodInfo Method_BossHealthBarManager_Update { get; } = Type_BossHealthBarManager.GetMethod("Update", TOMain.UniversalBindingFlags);

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

    Dictionary<MethodInfo, Delegate> ITODetourProvider.DetoursToApply => new()
    {
        [Method_BossHealthBarManager_Draw] = On_BossHealthBarManager_Draw,
        [Method_BossHealthBarManager_Update] = On_BossHealthBarManager_Update,
    };

    void ITOLoader.PostSetupContent()
    {
        MinibossHPBarList.Add(NPCID.LunarTowerVortex);
        MinibossHPBarList.Add(NPCID.LunarTowerStardust);
        MinibossHPBarList.Add(NPCID.LunarTowerNebula);
        MinibossHPBarList.Add(NPCID.LunarTowerSolar);
        MinibossHPBarList.Add(NPCID.PirateShip);
        OneToMany[NPCID.SkeletronHead] = [NPCID.SkeletronHand];
        OneToMany[NPCID.SkeletronPrime] = [NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser];
        OneToMany[NPCID.Golem] = [NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree];
        OneToMany[NPCID.BrainofCthulhu] = [NPCID.Creeper];
        OneToMany[NPCID.MartianSaucerCore] = [NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon];
        OneToMany[NPCID.PirateShip] = [NPCID.PirateShipCannon];
        OneToMany[ModContent.NPCType<CeaselessVoid>()] = [ModContent.NPCType<DarkEnergy>()];
        OneToMany[ModContent.NPCType<RavagerBody>()] =
        [
            ModContent.NPCType<RavagerClawRight>(),
            ModContent.NPCType<RavagerClawLeft>(),
            ModContent.NPCType<RavagerLegRight>(),
            ModContent.NPCType<RavagerLegLeft>(),
            ModContent.NPCType<RavagerHead>()
        ];
        OneToMany[ModContent.NPCType<EbonianPaladin>()] = [];
        OneToMany[ModContent.NPCType<CrimulanPaladin>()] = [];
    }

    void ITOLoader.OnModUnload()
    {
        MinibossHPBarList.Remove(NPCID.LunarTowerVortex);
        MinibossHPBarList.Remove(NPCID.LunarTowerStardust);
        MinibossHPBarList.Remove(NPCID.LunarTowerNebula);
        MinibossHPBarList.Remove(NPCID.LunarTowerSolar);
        MinibossHPBarList.Remove(NPCID.PirateShip);
        OneToMany[NPCID.SkeletronHead] = [NPCID.SkeletronHead, NPCID.SkeletronHand];
        OneToMany[NPCID.SkeletronPrime] = [NPCID.SkeletronPrime, NPCID.PrimeSaw, NPCID.PrimeVice, NPCID.PrimeCannon, NPCID.PrimeLaser];
        OneToMany[NPCID.Golem] = [NPCID.Golem, NPCID.GolemFistLeft, NPCID.GolemFistRight, NPCID.GolemHead, NPCID.GolemHeadFree];
        OneToMany[NPCID.BrainofCthulhu] = [NPCID.BrainofCthulhu, NPCID.Creeper];
        OneToMany[NPCID.MartianSaucerCore] = [NPCID.MartianSaucerCore, NPCID.MartianSaucerTurret, NPCID.MartianSaucerCannon];
        OneToMany[NPCID.PirateShip] = [NPCID.PirateShip, NPCID.PirateShipCannon];
        OneToMany[ModContent.NPCType<CeaselessVoid>()] =
        [
            ModContent.NPCType<CeaselessVoid>(),
            ModContent.NPCType<DarkEnergy>()
        ];
        OneToMany[ModContent.NPCType<RavagerBody>()] =
        [
            ModContent.NPCType<RavagerBody>(),
            ModContent.NPCType<RavagerClawRight>(),
            ModContent.NPCType<RavagerClawLeft>(),
            ModContent.NPCType<RavagerLegRight>(),
            ModContent.NPCType<RavagerLegLeft>(),
            ModContent.NPCType<RavagerHead>()
        ];
        OneToMany[ModContent.NPCType<EbonianPaladin>()] = OneToMany[ModContent.NPCType<CrimulanPaladin>()] =
        [
            ModContent.NPCType<EbonianPaladin>(),
            ModContent.NPCType<SplitEbonianPaladin>(),
            ModContent.NPCType<CrimulanPaladin>(),
            ModContent.NPCType<SplitCrimulanPaladin>()
        ];
    }

    void ITOLoader.OnWorldLoad() => _trackingBars.Clear();

    void ITOLoader.OnWorldUnload() => _trackingBars.Clear();
}
