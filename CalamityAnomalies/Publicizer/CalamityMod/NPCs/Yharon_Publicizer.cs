using CalamityMod.NPCs.Yharon;

namespace CalamityAnomalies.Publicizer.CalamityMod.NPCs;

#pragma warning disable IDE1006
public record Yharon_Publicizer(Yharon Yharon)
{
    public static readonly Type c_type = typeof(Yharon);

    public static readonly FieldInfo i_f_safeBox = c_type.GetField("safeBox", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_enraged = c_type.GetField("enraged", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_protectionBoost = c_type.GetField("protectionBoost", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_moveCloser = c_type.GetField("moveCloser", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_useTornado = c_type.GetField("useTornado", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_secondPhasePhase = c_type.GetField("secondPhasePhase", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_teleportLocation = c_type.GetField("teleportLocation", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_startSecondAI = c_type.GetField("startSecondAI", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_spawnArena = c_type.GetField("spawnArena", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_invincibilityCounter = c_type.GetField("invincibilityCounter", TOReflectionUtils.InstanceBindingFlags);
    public static readonly FieldInfo i_f_fastChargeTelegraphTime = c_type.GetField("fastChargeTelegraphTime", TOReflectionUtils.InstanceBindingFlags);

    public Rectangle safeBox
    {
        get => (Rectangle)i_f_safeBox.GetValue(Yharon);
        set => i_f_safeBox.SetValue(Yharon, value);
    }

    public bool enraged
    {
        get => (bool)i_f_enraged.GetValue(Yharon);
        set => i_f_enraged.SetValue(Yharon, value);
    }

    public bool protectionBoost
    {
        get => (bool)i_f_protectionBoost.GetValue(Yharon);
        set => i_f_protectionBoost.SetValue(Yharon, value);
    }

    public bool moveCloser
    {
        get => (bool)i_f_moveCloser.GetValue(Yharon);
        set => i_f_moveCloser.SetValue(Yharon, value);
    }

    public bool useTornado
    {
        get => (bool)i_f_useTornado.GetValue(Yharon);
        set => i_f_useTornado.SetValue(Yharon, value);
    }

    public int secondPhasePhase
    {
        get => (int)i_f_secondPhasePhase.GetValue(Yharon);
        set => i_f_secondPhasePhase.SetValue(Yharon, value);
    }

    public int teleportLocation
    {
        get => (int)i_f_teleportLocation.GetValue(Yharon);
        set => i_f_teleportLocation.SetValue(Yharon, value);
    }

    public bool startSecondAI
    {
        get => (bool)i_f_startSecondAI.GetValue(Yharon);
        set => i_f_startSecondAI.SetValue(Yharon, value);
    }

    public bool spawnArena
    {
        get => (bool)i_f_spawnArena.GetValue(Yharon);
        set => i_f_spawnArena.SetValue(Yharon, value);
    }

    public int invincibilityCounter
    {
        get => (int)i_f_invincibilityCounter.GetValue(Yharon);
        set => i_f_invincibilityCounter.SetValue(Yharon, value);
    }

    public int fastChargeTelegraphTime
    {
        get => (int)i_f_fastChargeTelegraphTime.GetValue(Yharon);
        set => i_f_fastChargeTelegraphTime.SetValue(Yharon, value);
    }
}
