namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(NPC npc)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TOGlobalNPC Ocean() => npc.GetGlobalNPC<TOGlobalNPC>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModNPC<T>() where T : ModNPC => npc.ModNPC as T;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetModNPCThrow<T>() where T : ModNPC => npc.GetModNPC<T>() ?? throw new ArgumentException($"NPC {npc.FullName} ({npc.type}) does not have a ModNPC of type {typeof(T).FullName}.", nameof(npc));
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetModNPC<T>([NotNullWhen(true)] out T result) where T : ModNPC => (result = npc.GetModNPC<T>()) is not null;

        public Texture2D Texture => TextureAssets.Npc[npc.type].Value;

        public Player PlayerTarget => npc.HasPlayerTarget ? Main.player[npc.target] : null;
        public NPC NPCTarget => npc.HasNPCTarget ? Main.npc[npc.target - 300] : null;

        public float LifeRatio => npc.lifeMax <= 0 ? 0f : (float)npc.life / npc.lifeMax;
        public float MissingLifeRatio => 1f - npc.LifeRatio;

        /// <summary>
        /// 符合直观习惯的NPC透明度。0表示完全透明，255表示完全不透明。
        /// </summary>
        public byte GraphicAlpha
        {
            get => (byte)(255 - npc.alpha);
            set => npc.alpha = Math.Clamp(255 - value, 0, 255);
        }

        public bool IsFriendly => npc.active && (npc.friendly || npc.townNPC || npc.lifeMax <= 5);

        public bool IsEnemy => npc.active && (!npc.friendly && npc.lifeMax >= 5);

        /// <summary>
        /// 检查NPC是否为Boss。
        /// </summary>
        /// <returns></returns>
        public bool IsBossEnemy => npc.active && (npc.boss || npc.EoW || npc.type == NPCID.WallofFleshEye);

        public bool EoW => npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

        public bool Destroyer => npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;

        public bool Twins => npc.type is NPCID.Retinazer or NPCID.Spazmatism;

        public bool SkeletronPrimeHand => npc.type is >= 128 and <= 131; //机械炮，机械锯，机械钳，机械激光

        public bool GolemFist => npc.type is NPCID.GolemFistLeft or NPCID.GolemFistRight;

        public bool CultistDragon => npc.type is >= 454 and <= 459; //幻影龙头部，幻影龙身体1，幻影龙身体2，幻影龙身体3，幻影龙身体4，幻影龙尾部

        public bool FrostMoonEnemy => npc.type is >= 338 and <= 352; //僵尸精灵，胡子僵尸精灵，女孩僵尸精灵, 礼物宝箱怪, 姜饼人，雪兽，常绿尖叫怪，冰雪女王，圣诞坦克，精灵直升机，胡桃夹士，旋转胡桃夹士，精灵弓箭手，坎卜斯，雪花怪

        public int TargetDirection => Math.Sign((npc.HasNPCTarget ? Main.projectile[npc.target - 300] : (Entity)Main.player[npc.target]).Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        public bool IsFacingTarget => npc.direction == npc.TargetDirection;

        public void FaceNPCTarget(Entity target) => npc.direction = Math.Sign(target.Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        /// <summary>
        /// 将NPC速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity">速度。</param>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使NPC额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。</param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            npc.velocity = velocity;
            npc.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的NPC，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="NPC.rotation"/>，并应用于NPC。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使NPC额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。</param>
        public void VelocityToRotation(float rotationOffset = 0f) => npc.rotation = npc.velocity.ToRotation() + rotationOffset;

        /// <summary>
        /// 如果现有目标无效，获取新的目标。
        /// </summary>
        /// <param name="faceTarget"></param>
        /// <param name="distanceThreshold"></param>
        /// <returns>获取目标后的目标是否有效。</returns>
        public bool TargetClosestIfInvalid(bool faceTarget = true, float distanceThreshold = 4000f)
        {
            if (!npc.HasValidTarget)
                npc.TargetClosest(faceTarget);

            Player target = Main.player[npc.target];

            if (distanceThreshold >= 0f && !npc.WithinRange(target.Center, distanceThreshold - target.aggro))
                npc.TargetClosest(faceTarget);

            return npc.HasValidTarget && npc.WithinRange(target.Center, distanceThreshold - target.aggro);
        }

        public void ChangeScaleFixBottom(int width, int height, float newScale)
        {
            if (npc.scale == newScale)
                return;

            Vector2 oldBottom = npc.Bottom;
            npc.width = (int)(width * newScale);
            npc.height = (int)(height * newScale);
            npc.scale = newScale;
            npc.Bottom = oldBottom;
        }

        public void ApplyDOT(int dot, int damageValue, ref int damage)
        {
            if (npc.lifeRegen > 0)
                npc.lifeRegen = 0;
            npc.lifeRegen -= dot;
            if (damage < damageValue)
                damage = damageValue;
        }

        public void AddBuff<T>(int time, bool quiet = false) where T : ModBuff => npc.AddBuff(ModContent.BuffType<T>(), time, quiet);
    }

    extension(NPC)
    {
        public static NPC DummyNPC => Main.npc[Main.maxNPCs];

        public static bool AnyNPCs<T>() where T : ModNPC => NPC.AnyNPCs(ModContent.NPCType<T>());

        public static bool AnyNPCs(int type, [NotNullWhen(true)] out NPC npc)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.type == type)
                    return true;
            }
            npc = null;
            return false;
        }

        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T)
                    return true;
            }
            npc = null;
            return false;
        }

        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc, [NotNullWhen(true)] out T modNPC) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T t)
                {
                    modNPC = t;
                    return true;
                }
            }
            npc = null;
            modNPC = null;
            return false;
        }

        public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsActive);

        public static TOIterator<NPC> Enemies => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsEnemy);

        public static TOIterator<NPC> Bosses => TOIteratorFactory.NewNPCIterator(IteratorMatches.NPC_IsBossEnemy);

        public static NPC CreateNPC(int type)
        {
            NPC npc = new();
            npc.SetDefaults(type);
            return npc;
        }

        public static NPC CreateNPC<T>() where T : ModNPC => CreateNPC(ModContent.NPCType<T>());

        public static NPC CreateNPC(int type, Action<NPC> action)
        {
            NPC npc = CreateNPC(type);
            action?.Invoke(npc);
            return npc;
        }

        public static NPC CreateNPC<T>(Action<NPC> action) where T : ModNPC
        {
            NPC npc = CreateNPC<T>();
            action?.Invoke(npc);
            return npc;
        }

        public static void SpawnOnPlayer<T>(int plr) where T : ModNPC => NPC.SpawnOnPlayer(plr, ModContent.NPCType<T>());

        /// <summary>
        /// 生成一个新的NPC，并在生成后执行一个Action。
        /// </summary>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">类型。</param>
        /// <param name="start">生成NPC索引的最小值。</param>
        /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
        public static void NewNPCAction(IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
        {
            int index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
            if (index < Main.maxNPCs)
            {
                action?.Invoke(Main.npc[index]);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
            }
        }

        /// <summary>
        /// 生成一个新的ModNPC，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModNPC所属类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="start">生成NPC索引的最小值。</param>
        /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
        public static void NewNPCAction<T>(IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
            NewNPCAction(source, position, ModContent.NPCType<T>(), start, action);

        /// <summary>
        /// 生成一个新的NPC，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的NPC索引。</param>
        /// <param name="npc">输出的NPC实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">类型。</param>
        /// <param name="start">生成NPC索引的最小值。</param>
        /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
        /// <returns>生成NPC是否成功。</returns>
        public static bool NewNPCActionCheck(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
        {
            index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
            if (index < Main.maxNPCs)
            {
                npc = Main.npc[index];
                action?.Invoke(npc);
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
                return true;
            }
            else
            {
                npc = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的ModNPC，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModNPC所属类型。</typeparam>
        /// <param name="index">输出的NPC索引。</param>
        /// <param name="npc">输出的NPC实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="start">生成NPC索引的最小值。</param>
        /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
        /// <returns>生成NPC是否成功。</returns>
        public static void NewNPCActionCheck<T>(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
            NewNPCActionCheck(out index, out npc, source, position, ModContent.NPCType<T>(), start, action);
    }
}