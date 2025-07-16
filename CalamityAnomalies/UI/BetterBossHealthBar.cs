using CalamityMod.Events;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.UI;
using Terraria.GameContent.Events;
using Terraria.GameContent.UI.BigProgressBar;
using static CalamityMod.UI.BossHealthBarManager;

namespace CalamityAnomalies.UI;


public sealed class BetterBossHealthBar : ModBossBarStyleDetour<BossHealthBarManager>, IResourceLoader
{
    /// <summary>
    /// 改进的Boss血条UI类。
    /// </summary>
    public sealed class BetterBossHPUI : BossHPUI
    {
        public static Color BaseColor { get; } = new(240, 240, 255);

        public bool HasOneToMany { get; }

        public int[] CustomOneToManyIndexes { get; }

        // TODO 这个字典是未完成的。在未来的异象模式中，Boss生成时会绑定自己的所有体节NPC。
        public Dictionary<long, NPC> CustomOneToMany { get; } = [];

        public bool HasSpecialLifeRequirement { get; } = false;

        public NPCSpecialHPGetFunction HPGetFunction { get; } = null;

        public bool Valid { get; private set; } = true;

        public new NPC AssociatedNPC => throw new InvalidOperationException("BetterBossHPUI.AssociatedNPC should not be used. Use BetterBossHPUI.NPC instead.");

        public NPC NPC { get; }

        public long Identifier { get; }

        public CAGlobalNPC AnomalyNPC { get; }

        public CalamityGlobalNPC CalamityNPC { get; }

        public new int NPCType => NPC.type;

        public new long CombinedNPCLife { get; private set; } = 0L;

        public new long CombinedNPCMaxLife { get; private set; } = 0L;

        public new bool NPCIsEnraged => Valid && NPC.active && (CalamityNPC.CurrentlyEnraged || (HasOneToMany && CustomOneToMany.Values.AsValueEnumerable().Any(n => n.Calamity().CurrentlyEnraged)));

        public new bool NPCIsIncreasingDefenseOrDR => Valid && NPC.active && (CalamityNPC.CurrentlyIncreasingDefenseOrDR || (HasOneToMany && CustomOneToMany.Values.AsValueEnumerable().Any(n => n.Calamity().CurrentlyIncreasingDefenseOrDR)));

        public int Height { get; private set; } = 70;

        public float AnimationCompletionRatio { get; private set; } = 0f;

        public float AnimationCompletionRatio2 { get; private set; } = 0f;

        public BetterBossHPUI(int npcIndex, string overridingName = null) : base(npcIndex, overridingName)
        {
            NPC = Main.npc[NPCIndex];
            Identifier = NPC.Ocean().Identifier;
            AnomalyNPC = NPC.Anomaly();
            CalamityNPC = NPC.Calamity();

            HasOneToMany = OneToMany.TryGetValue(NPCType, out int[] value);
            CustomOneToManyIndexes = value;

            foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
            {
                if (requirement(NPC))
                {
                    HasSpecialLifeRequirement = true;
                    HPGetFunction = func;
                }
            }
        }

        public new void Update() => throw new NotImplementedException("BetterBossHPUI.Update() should not be called directly. Use BetterBossHPUI.Update(bool valid) instead.");

        public void Update(bool valid)
        {
            Valid = valid;

            if (PreUpdate())
            {
                CombinedNPCLife = GetCombinedNPCLife();
                CombinedNPCMaxLife = GetCombinedNPCMaxLife();

                if (CombinedNPCLife != PreviousLife && PreviousLife != 0L)
                {
                    if (ComboDamageCountdown <= 0)
                        HealthAtStartOfCombo = CombinedNPCLife;
                    ComboDamageCountdown = 30;
                }
                PreviousLife = CombinedNPCLife;

                if (ComboDamageCountdown > 0)
                    ComboDamageCountdown--;

                CustomOneToMany.Clear();
                if (HasOneToMany)
                {
                    foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(n => CustomOneToManyIndexes.Contains(n.type)))
                        CustomOneToMany.TryAdd(npc.Ocean().Identifier, npc);
                }

                OpenAnimationTimer = Math.Clamp(OpenAnimationTimer + 1, 0, 120); //由80改为120

                if (Valid)
                {
                    EnrageTimer = Math.Clamp(EnrageTimer + (NPCIsEnraged ? 1 : -4), 0, 120);
                    IncreasingDefenseOrDRTimer = Math.Clamp(IncreasingDefenseOrDRTimer + (NPCIsIncreasingDefenseOrDR ? 1 : -4), 0, 120);
                    CloseAnimationTimer = Math.Clamp(CloseAnimationTimer - 2, 0, 120);
                }
                else
                {
                    EnrageTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                    IncreasingDefenseOrDRTimer = Math.Clamp(EnrageTimer - 4, 0, 120);
                    CloseAnimationTimer++;
                }

                if (CombinedNPCMaxLife != 0L && (InitialMaxLife == 0L || InitialMaxLife < CombinedNPCMaxLife))
                    InitialMaxLife = CombinedNPCMaxLife;

                ModifyCalBossBarHeight();

                AnimationCompletionRatio = CloseAnimationTimer > 0
                    ? 1f - MathHelper.Clamp(CloseAnimationTimer / 120f, 0f, 1f)
                    : MathHelper.Clamp(OpenAnimationTimer / 80f, 0f, 1f);
                AnimationCompletionRatio2 = CloseAnimationTimer > 0
                    ? 1f - MathHelper.Clamp(CloseAnimationTimer / 80f, 0f, 1f)
                    : MathHelper.Clamp(OpenAnimationTimer / 120f, 0f, 1f);
            }

            PostUpdate();
        }

        private long GetCombinedNPCLife()
        {
            if (!Valid || !NPC.active)
                return 0L;

            foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
            {
                if (requirement(NPC))
                    return func(NPC, checkingForMaxLife: false);
            }

            long result = NPCType == NPCID.PirateShip ? 0L : NPC.life;
            foreach ((long identifier, NPC npc) in CustomOneToMany)
            {
                if (npc.Ocean().Identifier == identifier && npc.active && npc.life > 0)
                    result += npc.life;
            }
            return result;
        }

        private long GetCombinedNPCMaxLife()
        {
            if (!Valid || !NPC.active)
                return 0L;

            foreach ((NPCSpecialHPGetRequirement requirement, NPCSpecialHPGetFunction func) in SpecialHPRequirements)
            {
                if (requirement(NPC))
                    return func(NPC, checkingForMaxLife: true);
            }

            long result = NPCType == NPCID.PirateShip ? 0L : NPC.lifeMax;
            foreach ((long identifier, NPC npc) in CustomOneToMany)
            {
                if (npc.Ocean().Identifier == identifier && npc.active && npc.lifeMax > 0)
                    result += npc.lifeMax;
            }
            return result;
        }

        private bool PreUpdate()
        {
            bool result = true;
            bool hasSingle = false;
            if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreUpdateCalBossBar)))
            {
                result &= npcBehavior.PreUpdateCalBossBar(this);
                hasSingle = true;
            }
            foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PreUpdateCalBossBar)))
                result &= anomalyGNPCBehavior.PreUpdateCalBossBar(NPC, this, hasSingle);
            return result;
        }

        private void ModifyCalBossBarHeight()
        {
            int height = 70;
            bool hasSingle = false;
            if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.ModifyCalBossBarHeight)))
            {
                npcBehavior.ModifyCalBossBarHeight(this, ref height);
                hasSingle = true;
            }
            foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.ModifyCalBossBarHeight)))
                anomalyGNPCBehavior.ModifyCalBossBarHeight(NPC, this, ref height, hasSingle);
            Height = height;
        }

        private void PostUpdate()
        {
            bool hasSingle = false;
            if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PostUpdateCalBossBar)))
                npcBehavior.PostUpdateCalBossBar(this);
            foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PostUpdateCalBossBar)))
                anomalyGNPCBehavior.PostUpdateCalBossBar(NPC, this, hasSingle);
        }

        public new void Draw(SpriteBatch spriteBatch, int x, int y)
        {
            if (PreDraw(spriteBatch, x, y))
            {
                DrawMainBar(spriteBatch, x, y);

                DrawComboBar(spriteBatch, x, y);

                (float sin, float cos) = TOMathHelper.GetTimeSinCos(0.5f, 1f, 0f, true);

                Color seperatorColor = (CAWorld.Anomaly ? Color.Lerp(BaseColor, Color.Lerp(CAMain.GetGradientColor(0.25f), CAMain.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin), Math.Clamp(AnomalyNPC.AnomalyAITimer / 120f, 0f, 1f))
                    : EnrageTimer > 0 ? Color.Lerp(BaseColor, Color.Red * 0.5f, Math.Clamp(EnrageTimer / 80f, 0f, 1f))
                    : IncreasingDefenseOrDRTimer > 0 ? Color.Lerp(BaseColor, Color.LightGray * 0.6f, Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 1f))
                    : BaseColor) * AnimationCompletionRatio;
                DrawSeperatorBar(spriteBatch, x, y, seperatorColor);

                //为了避免NPC名称过长遮挡大生命值数字，二者的绘制顺序在此处被调换了，即先绘制NPC名称，再绘制大生命值数字。
                Color? mainColor = AnomalyNPC.IsRunningAnomalyAI ? Color.Lerp(CAMain.GetGradientColor(0.25f), CAMain.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * cos * 0.8f) * AnimationCompletionRatio2
                    : EnrageTimer > 0 ? Color.Red * 0.6f * AnimationCompletionRatio2
                    : IncreasingDefenseOrDRTimer > 0 ? Color.LightGray * 0.7f * AnimationCompletionRatio2
                    : null;
                Color? borderColor = AnomalyNPC.IsRunningAnomalyAI ? Color.Lerp(CAMain.GetGradientColor(0.25f), CAMain.AnomalyUltramundaneColor, AnomalyNPC.AnomalyUltraBarTimer / 120f * sin) * AnimationCompletionRatio2
                    : EnrageTimer > 0 || IncreasingDefenseOrDRTimer > 0 ? Color.Black * 0.2f * AnimationCompletionRatio2
                    : null;
                float borderWidth = AnomalyNPC.IsRunningAnomalyAI ? (Math.Clamp(AnomalyNPC.AnomalyAITimer / 80f, 0f, 1.5f) + TOMathHelper.GetTimeSin(1f, 1f, 0f, true) * Math.Clamp(AnomalyNPC.AnomalyAITimer / 80f, 0f, 1f))
                    : EnrageTimer > 0 ? (EnrageTimer / 80f + TOMathHelper.GetTimeSin(1f, 1f, 0f, true) * Math.Clamp(EnrageTimer / 80f, 0f, 1f))
                    : IncreasingDefenseOrDRTimer > 0 ? (IncreasingDefenseOrDRTimer / 80f + TOMathHelper.GetTimeSin(1f, 1f, 0f, true) * Math.Clamp(IncreasingDefenseOrDRTimer / 80f, 0f, 1f))
                    : 0f;
                DrawNPCName(spriteBatch, x, y, null, mainColor, borderColor, borderWidth);

                DrawBigLifeText(spriteBatch, x, y);

                DrawExtraSmallText(spriteBatch, x, y);
            }

            PostDraw(spriteBatch, x, y);
        }

        private bool PreDraw(SpriteBatch spriteBatch, int x, int y)
        {
            bool result = true;
            bool hasSingle = false;
            if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PreDrawCalBossBar)))
            {
                result &= npcBehavior.PreDrawCalBossBar(this, spriteBatch, x, y);
                hasSingle = true;
            }
            foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PreDrawCalBossBar)))
                result &= anomalyGNPCBehavior.PreDrawCalBossBar(NPC, this, spriteBatch, x, y, hasSingle);
            return result;
        }

        private void PostDraw(SpriteBatch spriteBatch, int x, int y)
        {
            bool hasSingle = false;
            if (NPC.TryGetBehavior(out CASingleNPCBehavior npcBehavior, nameof(CASingleNPCBehavior.PostDrawCalBossBar)))
            {
                npcBehavior.PostDrawCalBossBar(this, spriteBatch, x, y);
                hasSingle = true;
            }
            foreach (CAGlobalNPCBehavior anomalyGNPCBehavior in GlobalNPCBehaviorHandler.BehaviorSet.GetBehaviors<CAGlobalNPCBehavior>(nameof(CAGlobalNPCBehavior.PostDrawCalBossBar)))
                anomalyGNPCBehavior.PostDrawCalBossBar(NPC, this, spriteBatch, x, y, hasSingle);
        }

        #region 公共绘制方法
        public void DrawMainBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
        {
            int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
            spriteBatch.Draw(BossMainHPBar, new Rectangle(x, y + 28, mainBarWidth, BossMainHPBar.Height), newColor ?? Color.White);
        }

        public void DrawComboBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null)
        {
            if (ComboDamageCountdown <= 0)
                return;

            int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);
            int comboHPBarWidth = (int)(400 * (float)HealthAtStartOfCombo / InitialMaxLife) - mainBarWidth;
            if (ComboDamageCountdown < 6)
                comboHPBarWidth = comboHPBarWidth * ComboDamageCountdown / 6;

            spriteBatch.Draw(BossComboHPBar, new Rectangle(x + mainBarWidth, y + 28, comboHPBarWidth, BossComboHPBar.Height), newColor ?? Color.White);
        }

        public void DrawSeperatorBar(SpriteBatch spriteBatch, int x, int y, Color? newColor = null) =>
            spriteBatch.Draw(BossSeperatorBar, new Rectangle(x, y + 18, 400, 6), newColor ?? BaseColor * AnimationCompletionRatio);

        public void DrawNPCName(SpriteBatch spriteBatch, int x, int y, string overrideText = null, Color? mainColor = null, Color? borderColor = null, float borderWidth = 0f)
        {
            string name = overrideText ?? OverridingName ?? NPC.FullName;
            Vector2 npcNameSize = MouseFont.MeasureString(name);
            Vector2 baseDrawPosition = new(x + 400 - npcNameSize.X, y + 23 - npcNameSize.Y);
            DrawBorderStringEightWay_Loop(spriteBatch, MouseFont, name, baseDrawPosition, mainColor, borderColor, Color.White * AnimationCompletionRatio2, Color.Black * 0.2f * AnimationCompletionRatio2, 8, borderWidth, 1f);
        }

        public void DrawBigLifeText(SpriteBatch spriteBatch, int x, int y, string overrideText = null)
        {
            string bigLifeText = overrideText ?? (NPCLifeRatio == 0f ? "0%" : (NPCLifeRatio * 100f).ToString("N1") + "%");
            Vector2 bigLifeTextSize = HPBarFont.MeasureString(bigLifeText);
            CalamityUtils.DrawBorderStringEightWay(spriteBatch, HPBarFont, bigLifeText, new Vector2(x, y + 22 - bigLifeTextSize.Y), MainColor * AnimationCompletionRatio2, MainBorderColour * 0.25f * AnimationCompletionRatio2);
        }

        public void DrawExtraSmallText(SpriteBatch spriteBatch, int x, int y, string overrideText = null, bool ignoreConfig = false)
        {
            if (!ignoreConfig && !CanDrawExtraSmallText)
                return;

            float whiteColorAlpha = OpenAnimationTimer switch
            {
                4 or 8 or 16 => Main.rand.NextFloat(0.7f, 0.8f),
                3 or 7 or 15 => Main.rand.NextFloat(0.4f, 0.5f),
                _ => AnimationCompletionRatio
            };
            int mainBarWidth = (int)MathHelper.Min(400f * AnimationCompletionRatio, 400f * NPCLifeRatio);

            string smallText = "";
            if (overrideText is not null)
                smallText = overrideText;
            else if (EntityExtensionHandler.TryGetValue(NPCType, out BossEntityExtension extraEntityData))
            {
                string extensionName = extraEntityData.NameOfExtensions.ToString();
                int extraEntities = CalamityUtils.CountNPCsBetter(extraEntityData.TypesToSearchFor);
                smallText += $"({extensionName}: {extraEntities}) ";
            }
            smallText += $"({CombinedNPCLife} / {InitialMaxLife})";

            CalamityUtils.DrawBorderStringEightWay(spriteBatch, ItemStackFont, smallText, new Vector2(Math.Max(x, x + mainBarWidth - (ItemStackFont.MeasureString(smallText) * 0.75f).X), y + 45), Color.White * whiteColorAlpha, Color.Black * whiteColorAlpha * 0.24f, 0.75f);
        }
        #endregion 公共绘制方法
    }


    public static DynamicSpriteFont MouseFont { get; } = FontAssets.MouseText.Value;
    public static DynamicSpriteFont ItemStackFont { get; } = FontAssets.ItemStack.Value;

    private static readonly Dictionary<long, BetterBossHPUI> _trackingBars = [];
    private const int MaxBars = 6;
    private const int MaxActiveBars = 4;

    public override void Detour_Draw(Orig_Draw orig, BossHealthBarManager self, SpriteBatch spriteBatch, IBigProgressBar currentBar, BigProgressBarInfo info)
    {
        int x = Main.screenWidth
            - (Main.playerInventory || Main.invasionType > 0 || Main.pumpkinMoon || Main.snowMoon || DD2Event.Ongoing || AcidRainEvent.AcidRainEventIsOngoing ? 670 : 420);
        int y = Main.screenHeight - 30;

        int activeCount = 0;

        foreach (BetterBossHPUI newBar in
            from pair in _trackingBars.AsValueEnumerable()
            let newBar = pair.Value
            orderby newBar.Valid descending, pair.Key ascending
            select newBar)
        {
            y -= newBar.Height;
            if (activeCount >= MaxActiveBars && newBar.Valid)
                continue;
            newBar.Draw(spriteBatch, x, y);
            if (newBar.Valid)
                activeCount++;
        }
    }

    public override void Detour_Update(Orig_Update orig, BossHealthBarManager self, IBigProgressBar currentBar, ref BigProgressBarInfo info)
    {
        HashSet<long> validIdentifiers = [];

        foreach (NPC npc in TOIteratorFactory.NewActiveNPCIterator(n => !BossExclusionList.Contains(n.type)))
        {
            long fromNPC = npc.Ocean().Identifier;
            string overridingName = null;
            if (npc.ModNPC is Apollo apollo)
                overridingName = CalamityUtils.GetTextValue("UI.ExoTwinsName" + (apollo.exoMechdusa ? "Hekate" : "Normal"));

            if (_trackingBars.ContainsKey(fromNPC))
                validIdentifiers.Add(fromNPC);
            else if (_trackingBars.Count < MaxBars && (
                (npc.IsABoss() && !(npc.type is NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail || npc.ModNPC is Artemis)) || MinibossHPBarList.Contains(npc.type) || npc.Calamity().CanHaveBossHealthBar))
            {
                _trackingBars.Add(fromNPC, new(npc.whoAmI, overridingName));
            }
        }

        foreach ((long identifier, BetterBossHPUI newBar) in _trackingBars)
        {
            newBar.Update(validIdentifiers.Contains(identifier));
            if (newBar.CloseAnimationTimer >= 120)
                _trackingBars.Remove(identifier);
        }
    }

    public static void DrawBorderStringEightWay_Loop(SpriteBatch spriteBatch, DynamicSpriteFont font, string text, Vector2 baseDrawPosition,
        Color? mainColor, Color? borderColor, Color mainColor2, Color borderColor2,
        int round, float borderWidth, float scale = 1f)
    {
        if (mainColor is not null && borderColor is not null && borderWidth > 0f)
        {
            for (int i = 0; i < round; i++)
                CalamityUtils.DrawBorderStringEightWay(spriteBatch, font, text, baseDrawPosition + new PolarVector2(borderWidth, MathHelper.TwoPi / round * i), mainColor.Value, borderColor.Value, scale);
        }
        CalamityUtils.DrawBorderStringEightWay(spriteBatch, font, text, baseDrawPosition, mainColor2, borderColor2, scale);
    }

    //灾厄还没有重写这个静态方法，所以要手动添加Detour
    public delegate void Orig_Load__Mod(Mod mod);

    public static void Detour_Load__Mod(Orig_Load__Mod orig, Mod mod)
    {
        orig(mod);
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

    public override void ApplyDetour()
    {
        base.ApplyDetour();
        TryApplyDetour(Detour_Load__Mod, false);
    }

    void IResourceLoader.OnWorldLoad() => _trackingBars.Clear();

    void IResourceLoader.OnWorldUnload() => _trackingBars.Clear();
}
