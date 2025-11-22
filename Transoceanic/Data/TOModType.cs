namespace Transoceanic.Data;

public interface ITOModNPC
{
}

public abstract class TOModNPC : ModNPC, ITOModNPC
{
    public TOGlobalNPC OceanNPC => NPC.Ocean();

    public Player Target => NPC.PlayerTarget;

    public int Timer1
    {
        get => OceanNPC.Timer1;
        set => OceanNPC.Timer1 = value;
    }
    public int Timer2
    {
        get => OceanNPC.Timer2;
        set => OceanNPC.Timer2 = value;
    }
    public int Timer3
    {
        get => OceanNPC.Timer3;
        set => OceanNPC.Timer3 = value;
    }
    public float Timer4
    {
        get => OceanNPC.Timer4;
        set => OceanNPC.Timer4 = value;
    }
    public float Timer5
    {
        get => OceanNPC.Timer5;
        set => OceanNPC.Timer5 = value;
    }
}

public interface ITOModProjectile
{
}

public abstract class TOModProjectile : ModProjectile, ITOModProjectile
{
    public TOGlobalProjectile OceanProjectile => Projectile.Ocean();

    public Player Owner => Projectile.Owner;

    public int Timer1
    {
        get => OceanProjectile.Timer1;
        set => OceanProjectile.Timer1 = value;
    }
    public int Timer2
    {
        get => OceanProjectile.Timer2;
        set => OceanProjectile.Timer2 = value;
    }
    public int Timer3
    {
        get => OceanProjectile.Timer3;
        set => OceanProjectile.Timer3 = value;
    }
    public float Timer4
    {
        get => OceanProjectile.Timer4;
        set => OceanProjectile.Timer4 = value;
    }
    public float Timer5
    {
        get => OceanProjectile.Timer5;
        set => OceanProjectile.Timer5 = value;
    }
}

public interface ITOModItem
{
}

public abstract class TOModItem : ModItem, ITOModItem
{
    public TOGlobalItem OceanItem => Item.Ocean();
}
