namespace CalamityAnomalies.Core;

public interface ICAModNPC
{
}

public interface ICAModProjectile
{
    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}

public interface ICAModItem
{
    /// <summary>
    /// 编辑受击NPC的DR。
    /// </summary>
    /// <param name="baseDR">由灾厄方法计算出的基础DR。</param>
    public virtual void ModifyHitNPC_DR(NPC target, Player player, ref NPC.HitModifiers modifiers, float baseDR, ref StatModifier baseDRModifier, ref StatModifier standardDRModifier, ref StatModifier timedDRModifier) { }
}
