namespace Transoceanic.Core;

public static class IteratorMatches
{
    public static readonly Func<Item, bool> Item_IsActive = i => i.active;
    public static readonly Func<Player, bool> Player_IsActive = p => p.active;
    public static readonly Func<Player, bool> Player_IsPVP = p => p.IsPvP;
    public static readonly Func<Projectile, bool> Projectile_IsActive = p => p.active;
    public static readonly Func<NPC, bool> NPC_IsActive = n => n.active;
    public static readonly Func<NPC, bool> NPC_IsEnemy = n => n.IsEnemy;
    public static readonly Func<NPC, bool> NPC_IsBossEnemy = n => n.IsBossEnemy;
}