namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(NPC npc)
    {
        public TOGlobalNPC Ocean() => npc.GetGlobalNPC<TOGlobalNPC>();

        public T GetModNPC<T>() where T : ModNPC => npc.ModNPC as T;

        public bool TOFriendly => npc.active && (npc.friendly || npc.townNPC || npc.lifeMax <= 5);

        public bool Enemy => !npc.friendly && npc.lifeMax >= 5;

        /// <summary>
        /// 检查NPC是否为Boss。
        /// </summary>
        /// <returns></returns>
        public bool TOBoss => npc.boss || npc.EoW || npc.type == NPCID.WallofFleshEye;

        public bool EoW => npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

        public bool Destroyer => npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;

        public bool Twins => npc.type is NPCID.Retinazer or NPCID.Spazmatism;

        public bool SkeletronPrimeHand => npc.type is >= 128 and <= 131; //机械炮：128，机械锯：129，机械钳：130，机械：131

        public bool GolemFist => npc.type is NPCID.GolemFistLeft or NPCID.GolemFistRight;

        public bool CultistDragon => npc.type is >= 454 and <= 459; //幻影龙头部：454，幻影龙身体1：455，幻影龙身体2：456，幻影龙身体3：457，幻影龙身体4：458，幻影龙尾部：459

        public int TargetDirection => Math.Sign((npc.HasNPCTarget ? Main.projectile[npc.target - 300] : (Entity)Main.player[npc.target]).Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        public bool IsFacingTarget => npc.direction == npc.TargetDirection;

        public void FaceNPCTarget(Entity target)
        {
            npc.direction = Math.Sign(target.Center.X - npc.Center.X) switch
            {
                -1 => -1,
                _ => 1
            };
        }

        /// <summary>
        /// 将NPC速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity"></param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            npc.velocity = velocity;
            npc.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的NPC，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="NPC.rotation"/>，并应用于NPC。
        /// </summary>
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

        public void BetterChangeScale(int width, int height, float scale)
        {
            if (npc.scale == scale)
                return;

            npc.position.X += npc.width / 2;
            npc.position.Y += npc.height;
            npc.scale = scale;
            npc.width = (int)(width * npc.scale);
            npc.height = (int)(height * npc.scale);
            npc.position.X -= npc.width / 2;
            npc.position.Y -= npc.height;
        }
    }

    extension(ref NPC.HitModifiers modifiers)
    {
        public void SetInstantKill2(NPC target) => modifiers.FinalDamage += target.lifeMax;
    }
}
