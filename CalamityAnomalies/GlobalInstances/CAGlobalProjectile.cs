

namespace CalamityAnomalies.GlobalInstances;

public sealed class CAGlobalProjectile : GlobalProjectileWithBehavior<CAProjectileBehavior>
{
    public override SingleEntityBehaviorSet<Projectile, CAProjectileBehavior> BehaviorSet => CABehaviorHelper.ProjectileBehaviors;

    #region Data
    private const int AISlot = 33;
    private const int AISlot2 = 17;

    public DataUnion32[] AnomalyAI { get; } = new DataUnion32[AISlot];
    public DataUnion64[] AnomalyAI2 { get; } = new DataUnion64[AISlot2];

    public ref Bits32 AIChanged => ref AnomalyAI[^1].bits;
    public ref Bits64 AIChanged2 => ref AnomalyAI2[^1].bits;

    private DataUnion32[] InternalAnomalyAI { get; } = new DataUnion32[AISlot];
    private DataUnion64[] InternalAnomalyAI2 { get; } = new DataUnion64[AISlot2];

    private ref Bits32 InternalAIChanged => ref AnomalyAI[^1].bits;
    private ref Bits64 InternalAIChanged2 => ref AnomalyAI2[^1].bits;

    public override GlobalProjectile Clone(Projectile from, Projectile to)
    {
        CAGlobalProjectile clone = (CAGlobalProjectile)base.Clone(from, to);

        Array.Copy(AnomalyAI, clone.AnomalyAI, AISlot);
        Array.Copy(AnomalyAI2, clone.AnomalyAI2, AISlot2);
        Array.Copy(InternalAnomalyAI, clone.InternalAnomalyAI, AISlot);
        Array.Copy(InternalAnomalyAI, clone.InternalAnomalyAI, AISlot2);

        return clone;
    }
    #endregion Data

    #region Defaults
    public override void SetStaticDefaults() => base.SetStaticDefaults();

    public override void SetDefaults(Projectile projectile) => base.SetDefaults(projectile);
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(Projectile projectile, IEntitySource source) => base.OnSpawn(projectile, source);

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac) => base.TileCollideStyle(projectile, ref width, ref height, ref fallThrough, ref hitboxCenterFrac);

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity) => base.OnTileCollide(projectile, oldVelocity);

    public override bool PreKill(Projectile projectile, int timeLeft) => base.PreKill(projectile, timeLeft);

    public override void OnKill(Projectile projectile, int timeLeft) => base.OnKill(projectile, timeLeft);
    #endregion Lifetime

    #region AI
    public override bool PreAI(Projectile projectile) => base.PreAI(projectile);

    public override void AI(Projectile projectile) => base.AI(projectile);

    public override void PostAI(Projectile projectile) => base.PostAI(projectile);

    public override bool ShouldUpdatePosition(Projectile projectile) => base.ShouldUpdatePosition(projectile);
    #endregion AI

    #region Draw
    public override Color? GetAlpha(Projectile projectile, Color lightColor) => base.GetAlpha(projectile, lightColor);

    public override bool PreDrawExtras(Projectile projectile) => base.PreDrawExtras(projectile);

    public override bool PreDraw(Projectile projectile, ref Color lightColor) => base.PreDraw(projectile, ref lightColor);

    public override void PostDraw(Projectile projectile, Color lightColor) => base.PostDraw(projectile, lightColor);

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => base.DrawBehind(projectile, index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    #endregion Draw

    #region Hit
    public override bool? CanCutTiles(Projectile projectile) => base.CanCutTiles(projectile);

    public override void CutTiles(Projectile projectile) => base.CutTiles(projectile);

    public override bool? CanDamage(Projectile projectile) => base.CanDamage(projectile);

    public override bool MinionContactDamage(Projectile projectile) => base.MinionContactDamage(projectile);

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox) => base.ModifyDamageHitbox(projectile, ref hitbox);

    public override bool? CanHitNPC(Projectile projectile, NPC target) => base.CanHitNPC(projectile, target);

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers) => base.ModifyHitNPC(projectile, target, ref modifiers);

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone) => base.OnHitNPC(projectile, target, hit, damageDone);

    public override bool CanHitPvp(Projectile projectile, Player target) => base.CanHitPvp(projectile, target);

    public override bool CanHitPlayer(Projectile projectile, Player target) => base.CanHitPlayer(projectile, target);

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers) => base.ModifyHitPlayer(projectile, target, ref modifiers);

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info) => base.OnHitPlayer(projectile, target, info);

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox) => base.Colliding(projectile, projHitbox, targetHitbox);
    #endregion Hit

    #region SpecialEffects
    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks) => base.NumGrappleHooks(projectile, player, ref numHooks);

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed) => base.GrappleRetreatSpeed(projectile, player, ref speed);

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed) => base.GrapplePullSpeed(projectile, player, ref speed);

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY) => base.GrappleTargetPoint(projectile, player, ref grappleX, ref grappleY);

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y) => base.GrappleCanLatchOnTo(projectile, player, x, y);

    public override void PrepareBombToBlow(Projectile projectile) => base.PrepareBombToBlow(projectile);

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight) => base.EmitEnchantmentVisualsAt(projectile, boxPosition, boxWidth, boxHeight);
    #endregion SpecialEffects

    #region NotOverriden
    public override bool? CanUseGrapple(int type, Player player) => null;

    public override void UseGrapple(Player player, ref int type) { }
    #endregion NotOverriden

    #region Net
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        Dictionary<int, float> aiToSend = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (AIChanged[i])
                aiToSend[i] = AnomalyAI[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged = default;

        Dictionary<int, double> aiToSend2 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged2[i])
                aiToSend2[i] = AnomalyAI2[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend2)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        AIChanged2 = default;

        Dictionary<int, float> aiToSend3 = [];
        for (int i = 0; i < AISlot - 1; i++)
        {
            if (InternalAIChanged[i])
                aiToSend[i] = InternalAnomalyAI[i].f;
        }
        binaryWriter.Write(aiToSend.Count);
        foreach ((int index, float value) in aiToSend3)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged = default;

        Dictionary<int, double> aiToSend4 = [];
        for (int i = 0; i < AISlot2 - 1; i++)
        {
            if (AIChanged2[i])
                aiToSend2[i] = InternalAnomalyAI2[i].d;
        }
        binaryWriter.Write(aiToSend2.Count);
        foreach ((int index, double value) in aiToSend4)
        {
            binaryWriter.Write(index);
            binaryWriter.Write(value);
        }
        InternalAIChanged2 = default;
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        int recievedAICount = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount; i++)
            AnomalyAI[binaryReader.ReadInt32()].f = binaryReader.ReadSingle();

        int recievedAICount2 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount2; i++)
            AnomalyAI2[binaryReader.ReadInt32()].d = binaryReader.ReadDouble();

        int recievedAICount3 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount3; i++)
            InternalAnomalyAI[binaryReader.ReadInt32()].f = binaryReader.ReadSingle();

        int recievedAICount4 = binaryReader.ReadInt32();
        for (int i = 0; i < recievedAICount4; i++)
            InternalAnomalyAI2[binaryReader.ReadInt32()].d = binaryReader.ReadDouble();
    }
    #endregion Net
}
