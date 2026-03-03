using CalamityMod.Graphics;

namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class EyeofCthulhuArena : CAModProjectile, IContentLoader
{
    public sealed class ArenaEye
    {
        public EyeofCthulhuArena Master;
        public int Index;
        public float Rotation;
        public bool ShouldFaceTarget;
        public int Timer;
        public int Timer2;
        public BehaviorCommand_ArenaEye MasterCommandReceiver;
        public List<EyeHighlight> Highlights = [];

        public Projectile MasterProjectile => Master.Projectile;
        public Player Target => Master.Master.PlayerTarget;
        public Vector2 Center => Master.GetEyeCenter(Index);

        public Vector2 OffsetCenter => Center + new PolarVector2(BloodlettingServant.ProjectileOffset, Rotation + MathHelper.Pi);

        public ArenaEye(EyeofCthulhuArena master, int index)
        {
            Master = master;
            Index = index;
        }

        public void AI()
        {
            Timer++;

            float targetRotation = ShouldFaceTarget ? (Target.Center - Master.GetEyeCenter(Index)).ToRotation(MathHelper.Pi) : Master.GetEyeRotation(Index);
            float rotationSpeed = Math.Max(Master.RotationSpeed + 0.1f, 0.3f);
            EoCHandler.UpdateRotation(ref Rotation, targetRotation, rotationSpeed);

            foreach (EyeHighlight highlight in Highlights)
                highlight?.Update();

            Highlights.RemoveAll(hightlight => hightlight is null || hightlight.ShouldBeRemoved);

            //执行命令
            switch (MasterCommandReceiver)
            {
                case BehaviorCommand_ArenaEye.ShootBlood or BehaviorCommand_ArenaEye.ShootBlood2:
                    ShootBlood();
                    break;
            }

            void ShootBlood()
            {
                bool shouldIncreaseTimer = true;

                ShouldFaceTarget = true;
                if (Timer2 == 0)
                    Highlights.Add(new EyeHighlight(EoCHandler.NormalTeleportDuration + 10, 20, 10f));
                else if (Timer2 == EoCHandler.NormalTeleportDuration)
                {
                    Vector2 offsetCenter = OffsetCenter;
                    Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, (Target.Center - offsetCenter).ToCustomLength(15f), ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);
                    if (MasterCommandReceiver == BehaviorCommand_ArenaEye.ShootBlood2)
                        Projectile.NewProjectileAction(MasterProjectile.GetSource_FromAI(), offsetCenter, (Target.Center - offsetCenter).ToCustomLength(20f), ProjectileID.BloodShot, EyeofCthulhu_Anomaly.BloodDamage, 0f, action: p => p.timeLeft = 120);
                    shouldIncreaseTimer = false;
                    ShouldFaceTarget = false;
                    MasterCommandReceiver = BehaviorCommand_ArenaEye.None;
                }

                if (shouldIncreaseTimer)
                    Timer2++;
            }
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, Rectangle frame, Color color)
        {
            Vector2 drawPosition = Master.GetEyeCenter(Index) - Main.screenPosition;
            Color lightColor = Color.Lerp(color, Color.White, 0.1f);
            foreach (EyeHighlight highlight in Highlights)
                TODrawUtils.DrawBorderTextureFromCenter(spriteBatch, texture, drawPosition, frame, lightColor, Rotation, MasterProjectile.scale, borderWidth: highlight.BorderWidth);
            spriteBatch.DrawFromCenter(texture, drawPosition, frame, color * Math.Clamp((float)Timer / EyeSpawnGateValue, 0f, 1f), Rotation, MasterProjectile.scale);
        }
    }

    public sealed class EyeHighlight
    {
        public int LifeTime;
        public int AnimationTime;
        public float FinalBorderWidth;
        public int? FadeBeginTimer;
        public int Timer;

        public float Ratio
        {
            get
            {
                float ratio;

                if (FadeBeginTimer.HasValue)
                {
                    int fadeBeginTimer = FadeBeginTimer.Value;
                    int startDist = Math.Min(fadeBeginTimer, LifeTime - fadeBeginTimer);
                    float startRatio = Math.Clamp((float)startDist / AnimationTime, 0f, 1f);
                    int elapsed = Timer - fadeBeginTimer;
                    float progress = Math.Clamp((float)elapsed / AnimationTime, 0f, 1f);
                    ratio = startRatio * 1f - progress;
                }
                else
                {
                    int dist = Math.Min(Timer, LifeTime - Timer);
                    ratio = Math.Clamp((float)dist / AnimationTime, 0f, 1f);
                }

                return TOMathUtils.Interpolation.QuadraticEaseInOut(ratio);
            }
        }
        public float Opacity => Ratio;
        public float BorderWidth => FinalBorderWidth * Ratio;

        public bool ShouldBeRemoved => Timer >= LifeTime || Timer - FadeBeginTimer >= AnimationTime;

        public EyeHighlight(int lifeTime, int animationTime, float finalBorderWidth)
        {
            LifeTime = lifeTime;
            AnimationTime = animationTime;
            FinalBorderWidth = finalBorderWidth;
        }

        public void BeginFade() => FadeBeginTimer = Timer;

        public void Update() => Timer++;
    }

    public static int EyeSpawnGateValue => 15;
    public static float NormalRotationSpeed => 0.025f;

    public bool IsActivated;

    public float PreviousArenaRadius = EoCHandler.MaxArenaRadius1;
    public float TargetArenaRadius = EoCHandler.MaxArenaRadius1;
    public float ArenaRadius;
    public int ArenaRadiusChangeTime;
    public int ArenaRadiusChangeDuration;

    public float PreviousRotationSpeed;
    public float TargetRotationSpeed;
    public float RotationSpeed;
    public int RotationSpeedChangeTime;
    public int RotationSpeedChangeDuration;

    public readonly ArenaEye[] Eyes = new ArenaEye[32];

    public NPC Master
    {
        get
        {
            int temp = (int)Projectile.ai[1];
            return temp >= 0 && temp < Main.maxNPCs ? Main.npc[temp] : null;
        }
        set => Projectile.ai[1] = value?.whoAmI ?? Main.maxNPCs;
    }

    public Ring ArenaRing => new(Projectile.Center, ArenaRadius - 20f, ArenaRadius + 20f);

    public float ArenaRotation
    {
        get => Projectile.rotation;
        set => Projectile.rotation = value;
    }

    public override string Texture => TOTextures.InvisibleTexturePath;
    public override string LocalizationCategory => "Anomaly.EyeofCthulhu";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.friendly = false;
        Projectile.hostile = true;
        Projectile.timeLeft = 100;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public float GetEyeRotation(int index) => ArenaRotation + TOMathUtils.PiOver16 * index;
    public Vector2 GetEyeCenter(int index) => Projectile.Center + new PolarVector2(ArenaRadius, GetEyeRotation(index));

    public override void AI()
    {
        if (Master is null || !Master.active || Master.type != NPCID.EyeofCthulhu)
            Projectile.Kill();

        EyeofCthulhu_Anomaly masterBehavior = new() { _entity = Master };
        Projectile.Center = masterBehavior.Phase3ArenaCenter;
        Projectile.timeLeft = 100;

        if (IsActivated)
        {
            int timer1Copy = Timer1;
            Timer1++;

            if (PreviousArenaRadius != TargetArenaRadius)
            {
                int localTimer = Timer1 - ArenaRadiusChangeTime;
                ArenaRadius = TOMathUtils.Interpolation.QuadraticEaseInOut(PreviousArenaRadius, TargetArenaRadius, (float)localTimer / ArenaRadiusChangeDuration);
                if (localTimer == ArenaRadiusChangeDuration)
                    PreviousArenaRadius = TargetArenaRadius;
            }

            switch (timer1Copy)
            {
                case 0:
                    ChangeRotationSpeedTo(0.25f, 10);
                    break;
                case EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 - 10:
                    ChangeRotationSpeedTo(NormalRotationSpeed, 10);
                    break;
            }

            if (PreviousRotationSpeed != TargetRotationSpeed)
            {
                int localTimer = Timer1 - RotationSpeedChangeTime;
                RotationSpeed = TOMathUtils.Interpolation.QuadraticEaseInOut(PreviousRotationSpeed, TargetRotationSpeed, (float)localTimer / RotationSpeedChangeDuration);
                if (localTimer == RotationSpeedChangeDuration)
                    PreviousRotationSpeed = TargetRotationSpeed;
            }

            ArenaRotation += RotationSpeed;


            if (timer1Copy % EyeSpawnGateValue == 0) //生成眼睛
            {
                int num = timer1Copy / EyeSpawnGateValue;

                //num = 0: 第0、1层
                //num = 1: 第2层
                //num = 2: 第3层
                //num = 3: 第4层

                if (num <= 3)
                {
                    if (num == 0)
                        SpawnEyes(0);
                    SpawnEyes(num + 1);
                }
                else
                {
                    for (int i = 0; i < Eyes.Length; i++)
                    {
                        if (Eyes[i] is null)
                            Eyes[i] = new ArenaEye(this, i);
                    }
                }
            }

            foreach (ArenaEye eye in Eyes)
                eye?.AI();
        }

        void SpawnEyes(int num)
        {
            int eyesToSpawn = (int)Math.Pow(2, Math.Max(num, 1));
            int firstEyeIndex = num == 0 ? 0 : 16 / (int)Math.Pow(2, num);
            int numIncreasePerEye = 32 / eyesToSpawn;

            for (int j = 0; j < eyesToSpawn; j++)
            {
                int index = (firstEyeIndex + numIncreasePerEye * j) % 32;
                ArenaEye item = new(this, index);
                if (num == 0)
                    item.Timer = 15;
                Eyes[index] = item;
            }
        }
    }

    public void ChangeArenaRadiusTo(float targetArenaRadius, int duration)
    {
        TargetArenaRadius = targetArenaRadius;
        ArenaRadiusChangeTime = Timer1 - 1;
        ArenaRadiusChangeDuration = duration;
    }

    public void ChangeRotationSpeedTo(float targetRotationSpeed, int duration)
    {
        TargetRotationSpeed = targetRotationSpeed;
        RotationSpeedChangeTime = Timer1 - 1;
        RotationSpeedChangeDuration = duration;
    }

    public void ExecuteActionToArenaEye(int index, Action<ArenaEye> action)
    {
        ArenaEye eye = Eyes[index];
        if (eye is not null)
            action?.Invoke(eye);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (!IsActivated)
            return false;

        Texture2D texture = TOAssetUtils.GetNPCTexture(ModContent.NPCType<BloodlettingServant>());

        //计算帧
        Projectile.frameCounter++;
        int frameNum;
        switch (Projectile.frameCounter)
        {
            case < 8:
                frameNum = 0;
                break;
            case < 16:
                frameNum = 1;
                break;
            default:
                Projectile.frameCounter = 0;
                frameNum = 0;
                break;
        }
        Rectangle frame = texture.Frame(1, 4, 0, frameNum);

        Color color = Color.Red * 0.75f;

        foreach (ArenaEye eye in Eyes)
            eye?.Draw(Main.spriteBatch, texture, frame, color);

        return false;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Timer1 > EyeofCthulhu_Anomaly.PhaseChangeGateValue_2To3_1 && ArenaRing.Collides(targetHitbox);
}

public sealed class EyeofCthulhuArena_Player : CAPlayerBehavior
{
    public override void PostUpdate()
    {
        //黑视效果
        bool success = false;

        foreach (Projectile arenaProjectile in Projectile.ActiveProjectiles)
        {
            if (arenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.IsActivated)
            {
                success = true;

                float scaleMultiplier = 0.008f;

                EnhancedDarknessSystem.lights.Add(new EnhancedDarknessSystem.LightSource(arenaProjectile.Center, BloomParticle.BloomCircleLarge, scale: arena.ArenaRadius * scaleMultiplier));
                EnhancedDarknessSystem.lights.Add(new EnhancedDarknessSystem.LightSource(scale: 2f, opacity: MathHelper.Clamp(Main.LocalPlayer.Distance(arenaProjectile.Center) / 640f, 0, 1)));
            }
        }

        if (success)
            CalamityPlayer.darknessIntensity = Math.Clamp(CalamityPlayer.darknessIntensity + 0.065f, 0f, 1f);
    }

    public override void ModifyScreenPosition()
    {
        foreach (Projectile arenaProjectile in Projectile.ActiveProjectiles)
        {
            if (arenaProjectile.ModProjectile is EyeofCthulhuArena arena && arena.IsActivated && Main.ScreenSize.X > arena.ArenaRadius * 2.2f && Main.ScreenSize.Y > arena.ArenaRadius * 2.2f)
            {
                OceanPlayer.ScreenFocusCenter = arenaProjectile.Center;
                OceanPlayer.ScreenFocusInterpolant += 0.12f;
                OceanPlayer.ScreenFocusHoldInPlaceTime = 5;
                break;
            }
        }
    }
}