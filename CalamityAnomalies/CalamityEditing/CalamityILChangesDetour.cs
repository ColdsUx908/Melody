using CalamityMod.ILEditing;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace CalamityAnomalies.CalamityEditing;

public class CalamityILChangesDetour : ICALoader
{
    public delegate void Orig_FixJumpHeightBoosts(ILContext il);

    public static void Detour_FixJumpHeightBoosts(Orig_FixJumpHeightBoosts orig, ILContext il)
    {
        ILCursor cursor = new(il);

        // Remove the code that makes Shiny Red Balloon SET jump height to a specific value to make balancing jump speed easier.
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(20)))
        {
            LogFailure("Jump Height Boost Fixes", "Could not locate Shiny Red Balloon jump height assignment value.");
            return;
        }

        // Delete both the ldc.i4 20 AND the store that assigns it to Player.jumpHeight.
        cursor.RemoveRange(2);

        // Change the jump speed from Shiny Red Balloon to be an actual boost instead of a hardcoded replacement.
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdcR4(6.51f)))
        {
            LogFailure("Jump Height Boost Fixes", "Could not locate Shiny Red Balloon jump speed assignment value.");
            return;
        }

        // Replace the hardcoded 6.51 with a balanceable value in CalamityPlayer.
        cursor.Prev.Operand = 0.75f; // BalancingConstants.BalloonJumpSpeedBoost
        // Load the player's current jumpSpeed onto the stack and add the boost to it.
        cursor.Emit(OpCodes.Ldsfld, typeof(Player).GetField("jumpSpeed"));
        cursor.Emit(OpCodes.Add);

        //禁用！

        // Find the Soaring Insignia jump speed bonus and reduce it to 0.5f.
        /*
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(1.8f)))
        {
            LogFailure("Jump Height Boost Fixes", "Could not locate Soaring Insignia jump speed boost value.");
            return;
        }
        cursor.Remove();
        cursor.Emit(OpCodes.Ldc_R4, 0.5f); // Decrease to 0.5f.
        */

        // Find the Frog Leg jump speed bonus and reduce it to 1.2f.
        // I don't know if this fucking does anything anymore, but I'm leaving it in just in case.
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(2.4f)))
        {
            LogFailure("Jump Height Boost Fixes", "Could not locate Frog Leg jump speed boost value.");
            return;
        }
        cursor.Remove();
        cursor.Emit(OpCodes.Ldc_R4, 1.2f); // Decrease to 1.2f.

        // Remove the jump height addition from the Werewolf buff (Moon Charm).
        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(2)))
        {
            LogFailure("Jump Height Boost Fixes", "Could not locate Moon Charm jump height boost value.");
            return;
        }
        cursor.Next.Operand = 0;
    }

    public delegate void Orig_NerfOverpoweredRunAccelerationSources(ILContext il);

    public static void Detour_NerfOverpoweredRunAccelerationSources(Orig_NerfOverpoweredRunAccelerationSources orig, ILContext il)
    {
        // First: Soaring Insignia. Find the check for whether it's equipped for run speeds.
        ILCursor cursor = new(il);
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("empressBrooch")))
        {
            LogFailure("Run Acceleration Nerfs", "Could not locate the Soaring Insignia bool.");
            return;
        }

        if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(1.75f)))
        {
            LogFailure("Run Acceleration Nerfs", "Could not locate the Soaring Insignia run acceleration multiplier.");
            return;
        }
        cursor.Next.Operand = 1.3f; //原灾厄：BalancingConstants.SoaringInsigniaRunAccelerationMultiplier (1.1f)

        // Second: Magiluminescence. Find the check for whether it's equipped for run speeds.
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("hasMagiluminescence")))
        {
            LogFailure("Run Acceleration Nerfs", "Could not locate the Magiluminescence bool.");
            return;
        }

        //
        // Don't actually do anything. Magiluminescence is not intended to be nerfed by Calamity.
        //

        // Third: Shadow Armor. Find the check for whether it's equipped for run speeds.
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("shadowArmor")))
        {
            LogFailure("Run Acceleration Nerfs", "Could not locate the Shadow Armor bool.");
            return;
        }

        // Load the player onto the stack as an argument to the following delegate.
        // Emit a delegate which consumes the Shadow Armor bool, performs Calamity effects, then always returns false.
        // Returning false ensures vanilla Shadow Armor code never runs.
        cursor.Emit(OpCodes.Ldarg_0);

        cursor.EmitDelegate((bool shadowArmor, Player p) =>
        {
            // If you don't even have Shadow Armor equipped, do nothing.
            if (!shadowArmor)
                return 0;

            // Shadow Armor does not stack with Magiluminescence if you are on the ground.
            if (p.hasMagiluminescence && p.velocity.Y == 0)
                return 0;

            // Shadow Armor grants reduced movement bonuses if in the air, or on the ground WITHOUT Magiluminescence.
            p.runAcceleration *= 1.25f; // BalancingConstants.ShadowArmorRunAccelerationMultiplier
            p.maxRunSpeed *= 1.05f; // BalancingConstants.ShadowArmorMaxRunSpeedMultiplier
            p.accRunSpeed *= 1.05f; // BalancingConstants.ShadowArmorAccRunSpeedMultiplier
            p.runSlowdown *= 1.5f; // BalancingConstants.ShadowArmorRunSlowdownMultiplier

            // Vanilla Shadow Armor behavior should still always be skipped.
            return 0;
        });


        // Finally: Back to Soaring Insignia. Prevent the rocket boots infinite flight effect, since it's in the same function.
        if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("empressBrooch")))
        {
            LogFailure("Run Acceleration Nerfs", "Could not locate the Soaring Insignia bool.");
            return;
        }

        // AND with 0 (false) so that the Soaring Insignia is never considered equipped and thus infinite rocket boots never triggers.
        cursor.Emit(OpCodes.Ldc_I4_0);
        cursor.Emit(OpCodes.And);
    }

    private static void LogFailure(string name, string reason) => CAUtils.LogILFailure(name, reason);

    void ICALoader.Load()
    {
        TODetourUtils.ApplyAllStaticMethodDetoursOfType(GetType(), typeof(ILChanges));
    }
}
