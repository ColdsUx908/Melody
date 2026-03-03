namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public class BloodlettingServant : CAModNPC
{
    public enum ServantPlace
    {
        Left,
        Right
    }

    public override string Texture => TOAssetUtils.FormatVanillaNPCTexturePath(NPCID.WanderingEye);
    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public ServantPlace Place;
    public float PositionRotation
    {
        get;
        set => field = TOMathUtils.NormalizeWithPeriod(value);
    }

    public bool ShouldUsePhase2Frame;
    public BehaviorCommand_Servant MasterCommandReceiver;
    public float FollowDistance;
    public float ArenaRadius;

    public const int TimeToGetPosition = 50;
    public const float ProjectileOffset = 10f;

    public static float MaxFollowDistance => 64f;
    public static float MaxFollowDistanceIncreased => 360f;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 4;
        NPCID.Sets.TrailingMode[Type] = 3;
        NPCID.Sets.TrailCacheLength[Type] = 5;
        NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 20;
        NPC.width = 30;
        NPC.height = 32;

        NPC.defense = 5;

        NPC.lifeMax = 96;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        CalamityNPC.VulnerableToCold = true;
        CalamityNPC.VulnerableToHeat = true;
        CalamityNPC.VulnerableToSickness = true;
    }

    public override void AI()
    {
        if (!NPC.TryGetMaster(NPCID.EyeofCthulhu, out NPC master))
        {
            NPC.life = 0;
            NPC.HitEffect();
            NPC.active = false;
            NPC.netUpdate = true;
            return;
        }

        NPC.target = master.target; //同步目标

        EyeofCthulhu_Anomaly masterBehavior = new() { _entity = master };

        NPC.dontTakeDamage = true;

        NPC.velocity = Vector2.Zero;

        Timer1++;
        if (ShouldUsePhase2Frame)
        {
            Timer2 = Math.Clamp(Timer2 + 1, 0, 5);
            Timer3++;
        }
        else
        {
            Timer2 = Math.Clamp(Timer2 - 1, 0, 5);
            Timer3 = Math.Max(Timer3 - 4, 0);
        }

        //更新位置和旋转

        if (masterBehavior.CurrentPhase is EyeofCthulhu_Anomaly.Phase.PhaseChange_1To2 or EyeofCthulhu_Anomaly.Phase.Phase2)
            MiscAI_Phase2();

        Lighting.AddLight(NPC.Center, 0.8f, 0f, 0f);

        //执行命令
        switch (MasterCommandReceiver)
        {
            case BehaviorCommand_Servant.ShootBlood:
                ShootBlood();
                break;
            case BehaviorCommand_Servant.IncreaseFollowDistance or BehaviorCommand_Servant.ReduceFollowDistance:
                ChangeFollowDistance();
                break;
            case BehaviorCommand_Servant.GetToArenaPosition:
                GetToArenaPosition();
                break;
        }

        void MiscAI_Phase2()
        {
            if (FollowDistance < MaxFollowDistance)
                FollowDistance = Utils.Remap(Timer1, 0f, TimeToGetPosition, 0f, MaxFollowDistance);
            if (FollowDistance >= MaxFollowDistance)
            {
                float newPositionRotation = PositionRotation;
                float targetPositionRotation = master.rotation;
                float acceleration = ShouldUsePhase2Frame ? 0.5f : 0.15f;
                EoCHandler.UpdateRotation(ref newPositionRotation, targetPositionRotation, acceleration);
                PositionRotation = newPositionRotation;
            }
            Vector2 offset = new Vector2(FollowDistance * (Place == ServantPlace.Left ? -1 : 1), 0f).RotatedBy(PositionRotation);
            NPC.Center = master.Center + offset;

            NPC.damage = ShouldUsePhase2Frame ? NPC.defDamage : 0;

            if (ShouldUsePhase2Frame)
                NPC.rotation = master.rotation - MathHelper.PiOver2;
            else
            {
                float targetRotation = TOMathUtils.NormalizeWithPeriod((Target.Center - NPC.Center).ToRotation(MathHelper.Pi));
                EoCHandler.UpdateRotation(ref NPC.rotation, targetRotation, 0.12f);
            }
        }

        void ShootBlood()
        {
            float projectileSpeed = 17.5f;
            EoCHandler.ShootProjectile(NPC, ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, projectileSpeed, 3, MathHelper.ToRadians(15f), p => p.timeLeft = 300);

            MasterCommandReceiver = BehaviorCommand_Servant.None;
        }

        void ChangeFollowDistance()
        {
            bool increase = MasterCommandReceiver == BehaviorCommand_Servant.IncreaseFollowDistance;

            Timer4 = Math.Clamp(Timer4 + increase.ToDirectionInt(), 0, 10);
            FollowDistance = MaxFollowDistance + (MaxFollowDistanceIncreased - MaxFollowDistance) * TOMathUtils.Interpolation.QuadraticEaseInOut(Timer4 / 10f);
            Vector2 offset = new Vector2(FollowDistance * (Place == ServantPlace.Left ? -1 : 1), 0f).RotatedBy(PositionRotation);
            NPC.Center = master.Center + offset;

            if ((increase && Timer4 == 10) || (!increase && Timer4 == 0))
                MasterCommandReceiver = BehaviorCommand_Servant.None;
        }

        void GetToArenaPosition()
        {
            NPC.damage = 0;

            if (masterBehavior.Phase3)
            {
                MasterCommandReceiver = BehaviorCommand_Servant.None;
                return;
            }

            float timer = masterBehavior.Timer1;
            ArenaRadius = MathHelper.Lerp(MaxFollowDistance, EoCHandler.MaxArenaRadius1, TOMathUtils.Interpolation.ExponentialEaseInOut(timer / EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1, 4f));

            float newPositionRotation = PositionRotation;
            float targetPositionRotation = Place == ServantPlace.Left ? MathHelper.Pi : 0f;
            EoCHandler.UpdateRotation(ref newPositionRotation, targetPositionRotation, 0.2f * TOMathUtils.Interpolation.CubicEaseInOut(masterBehavior.Timer1 / 10f));
            PositionRotation = newPositionRotation;

            Vector2 offset = new Vector2(ArenaRadius, 0f).RotatedBy(PositionRotation);
            Vector2 destination = masterBehavior.Phase3ArenaCenter + offset;
            NPC.Center = Vector2.SmootherStep(NPC.Center, destination, Math.Clamp(timer / EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1, 0f, 1f));

            float targetRotation = Place == ServantPlace.Left ^ timer > EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 ? 0f : MathHelper.Pi;
            EoCHandler.UpdateRotation(ref NPC.rotation, targetRotation, 0.3f * TOMathUtils.Interpolation.CubicEaseInOut(masterBehavior.Timer1 / 10f));
        }
    }

    public override void FindFrame(int frameHeight)
    {
        int frameNum;

        NPC.frameCounter += 1.0;

        switch (NPC.frameCounter)
        {
            case < 8.0:
                frameNum = 0;
                break;
            case < 16.0:
                frameNum = 1;
                break;
            default:
                NPC.frameCounter = 0.0;
                frameNum = 0;
                break;
        }

        if (ShouldUsePhase2Frame)
            frameNum += 2;

        NPC.frame.Y = frameNum * frameHeight;
    }

    public override Color? GetAlpha(Color drawColor) => (Color.Red * 0.75f) with { A = NPC.GraphicAlpha };

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D npcTexture = NPC.Texture;
        Color originalColor = NPC.GetAlpha(drawColor);
        Color newColor = Color.Tomato with { A = NPC.GraphicAlpha };
        float telegraphScalar = MathHelper.Clamp(Timer4 / 15f, 0f, 1f);
        Color telegraphColor = Color.Lerp(originalColor, newColor, telegraphScalar);

        if (Timer2 > 0)
        {
            Rectangle frame = NPC.frame;
            if (!ShouldUsePhase2Frame)
                frame.Y += npcTexture.Height / 2;

            int afterimageAmount = Math.Min(Timer2, 5);
            for (int j = 0; j < afterimageAmount; j++)
            {
                Color afterimageColor = telegraphColor * ((afterimageAmount - j) / 10f);
                Vector2 afterimagePos = NPC.oldPos[j] + new Vector2(NPC.width, NPC.height) / 2f - screenPos;
                spriteBatch.DrawFromCenter(npcTexture, afterimagePos, frame, afterimageColor, NPC.oldRot[j], NPC.scale);
            }
        }

        spriteBatch.DrawFromCenter(npcTexture, NPC.Center - screenPos, NPC.frame, telegraphColor, NPC.rotation, NPC.scale);
        return false;
    }
}

