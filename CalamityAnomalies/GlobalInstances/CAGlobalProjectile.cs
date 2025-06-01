using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Transoceanic.Net;

namespace CalamityAnomalies.GlobalInstances;

public partial class CAGlobalProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private const int MaxAISlots = 32;

    /// <summary>
    /// 额外的AI槽位，共32个。
    /// </summary>
    public float[] AnomalyAI { get; } = new float[MaxAISlots];

    public bool[] AIChanged { get; } = new bool[MaxAISlots];

    public void SetAnomalyAI(float value, Index index)
    {
        AnomalyAI[index] = value;
        AIChanged[index] = true;
    }

    #region Defaults
    public override void SetStaticDefaults()
    {
        foreach (CAProjectileOverride projectileOverride in CAOverrideHelper.ProjectileOverrides.Values)
            projectileOverride.SetStaticDefaults();
    }

    public override void SetDefaults(Projectile projectile)
    {
        Array.Fill(AnomalyAI, 0f);

        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.SetDefaults();
    }
    #endregion

    #region AI
    public override bool PreAI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreAI())
                return false;
        }

        return true;
    }

    public override void AI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.AI();
    }

    public override void PostAI(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.PostAI();
    }
    #endregion

    #region Draw
    public override Color? GetAlpha(Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            Color? result = projectileOverride.GetAlpha(lightColor);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDrawExtras())
                return false;
        }

        return true;
    }

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.PreDraw(ref lightColor))
                return false;
        }

        return true;
    }

    public override void PostDraw(Projectile projectile, Color lightColor)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.PostDraw(lightColor);
    }

    public override void DrawBehind(Projectile projectile, int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.DrawBehind(index, behindNPCsAndTiles, behindNPCs, behindProjectiles, overPlayers, overWiresUI);
    }
    #endregion

    #region Hit
    public override bool? CanCutTiles(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanCutTiles();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void CutTiles(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.CutTiles();
    }

    public override bool? CanDamage(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanDamage();
            if (result is not null)
                return result;
        }

        return null;
    }

    public override bool MinionContactDamage(Projectile projectile)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.MinionContactDamage())
                return false;
        }

        return false;
    }

    public override void ModifyDamageHitbox(Projectile projectile, ref Rectangle hitbox)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyDamageHitbox(ref hitbox);
    }

    public override bool? CanHitNPC(Projectile projectile, NPC target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.CanHitNPC(target);
            if (result is not null)
                return result;
        }

        return null;
    }

    public override void ModifyHitNPC(Projectile projectile, NPC target, ref NPC.HitModifiers modifiers)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyHitNPC(target, ref modifiers);
    }

    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.OnHitNPC(target, hit, damageDone);
    }

    public override bool CanHitPvp(Projectile projectile, Player target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.CanHitPvp(target))
                return false;
        }

        return true;
    }

    public override bool CanHitPlayer(Projectile projectile, Player target)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            if (!projectileOverride.CanHitPlayer(target))
                return false;
        }

        return true;
    }

    public override void ModifyHitPlayer(Projectile projectile, Player target, ref Player.HurtModifiers modifiers)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.ModifyHitPlayer(target, ref modifiers);
    }

    public override void OnHitPlayer(Projectile projectile, Player target, Player.HurtInfo info)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
            projectileOverride.OnHitPlayer(target, info);
    }

    public override bool? Colliding(Projectile projectile, Rectangle projHitbox, Rectangle targetHitbox)
    {
        if (projectile.TryGetOverride(out CAProjectileOverride projectileOverride))
        {
            bool? result = projectileOverride.Colliding(projHitbox, targetHitbox);
            if (result is not null)
                return result;
        }

        return null;
    }
    #endregion

    #region Net
    public override void SendExtraAI(Projectile projectile, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        TONetUtils.SendAI(AnomalyAI, AIChanged, binaryWriter);
    }

    public override void ReceiveExtraAI(Projectile projectile, BitReader bitReader, BinaryReader binaryReader)
    {
        TONetUtils.ReceiveAI(AnomalyAI, binaryReader);
    }
    #endregion
}
