using CalamityMod.ILEditing;
using MonoMod.Cil;

namespace CalamityAnomalies.CalamityEditing;

public sealed class CalamityILChangesDetour : ICALoader
{
    public static void Detour_NerfOverpoweredRunAccelerationSources(ILContext.Manipulator orig, ILContext il)
    {
        orig(il);
        ILCursor cursor = new(il);

        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("empressBrooch")))
        {
            CAUtils.LogILFailure("Run Acceleration Nerf Balance", "Could not locate the Soaring Insignia bool.");
            return;
        }
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(1.1f))) //BalancingConstants.SoaringInsigniaRunAccelerationMultiplier
        {
            CAUtils.LogILFailure("Run Acceleration Nerf Balance", "Could not locate the nerfed Soaring Insignia run acceleration multiplier.");
            return;
        }
        cursor.Next.Operand = 1.3f;
    }

    void ICALoader.Load()
    {
        if (CAServerConfig.Instance.Contents)
            TODetourUtils.ApplyAllStaticMethodDetoursOfType(GetType(), typeof(ILChanges));
    }
}
