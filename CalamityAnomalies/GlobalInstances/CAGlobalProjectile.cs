
namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    #region Data
    private const int AISlot = 32;
    private const int AISlot2 = 16;

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
    public override void SetStaticDefaults()
    {
        foreach (CAProjectileBehavior projectileBehavior in CABehaviorHelper.ProjectileBehaviors)
            projectileBehavior.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.SetDefaults();
    }
    #endregion Defaults

    #region Lifetime
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.OnSpawn(source);
    }

    public override bool TileCollideStyle(Projectile projectile, ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac))
                return false;
        }

        return true;
    }

    public override bool OnTileCollide(Projectile projectile, Vector2 oldVelocity)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.OnTileCollide(oldVelocity))
                return false;
        }

        return true;
    }

    public override bool PreKill(Projectile projectile, int timeLeft)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreKill(timeLeft))
                return false;
        }

        return true;
    }

    public override void OnKill(Projectile projectile, int timeLeft)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.OnKill(timeLeft);
    }
    #endregion Lifetime

    #region AI
    public override bool PreAI(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreAI())
                return false;
        }

        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.AI();
    }

    public override void PostAI(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.PostAI();
    }

    public override bool ShouldUpdatePosition(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.ShouldUpdatePosition())
                return false;
        }
        return true;
    }
    #endregion AI

    #region Draw
    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            Color? result = projectileBehavior.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDrawExtras())
                return false;
        }

        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.PreDraw(ref lightColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    #endregion Draw

    #region Hit
    public override bool? CanCutTiles(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanCutTiles();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.CutTiles();
    }

    public override bool? CanDamage(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanDamage();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.MinionContactDamage())
                return false;
        }

        return false;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyDamageHitbox(ref hitbox);
    }

    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.CanHitNPC(target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitNPC(target, hit, damageDone);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPvp(target))
                return false;
        }

        return true;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            if (!projectileBehavior.CanHitPlayer(target))
                return false;
        }

        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.OnHitPlayer(target, info);
    }

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.Colliding(projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }

        return null;
    }
    #endregion Hit

    #region SpecialEffects
    public override void NumGrappleHooks(Projectile projectile, Player player, ref int numHooks)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.NumGrappleHooks(player, ref numHooks);
    }

    public override void GrappleRetreatSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleRetreatSpeed(player, ref speed);
    }

    public override void GrapplePullSpeed(Projectile projectile, Player player, ref float speed)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.GrapplePullSpeed(player, ref speed);
    }

    public override void GrappleTargetPoint(Projectile projectile, Player player, ref float grappleX, ref float grappleY)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.GrappleTargetPoint(player, ref grappleX, ref grappleY);
    }

    public override bool? GrappleCanLatchOnTo(Projectile projectile, Player player, int x, int y)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
        {
            bool? result = projectileBehavior.GrappleCanLatchOnTo(player, x, y);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void PrepareBombToBlow(Projectile projectile)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.PrepareBombToBlow();
    }

    public override void EmitEnchantmentVisualsAt(Projectile projectile, Vector2 boxPosition, int boxWidth, int boxHeight)
    {
        if (projectile.TryGetBehavior(out CAProjectileBehavior projectileBehavior))
            projectileBehavior.EmitEnchantmentVisualsAt(boxPosition, boxWidth, boxHeight);
    }
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
