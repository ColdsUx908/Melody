namespace CalamityAnomalies.Core;

public sealed class CASet : ModSystem
{
    public static bool[] TweakedNPCs { get; private set; }
    public static bool[] TweakedProjectiles { get; private set; }
    public static bool[] TweakedItems { get; private set; }

    public override void ResizeArrays()
    {
        TweakedNPCs = NPCID.Sets.Factory.CreateBoolSet(false);
        TweakedProjectiles = ProjectileID.Sets.Factory.CreateBoolSet(false);
        TweakedItems = ItemID.Sets.Factory.CreateBoolSet(false);
    }
}
