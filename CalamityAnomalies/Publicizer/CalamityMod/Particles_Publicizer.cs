using CalamityMod.Particles;
using Transoceanic.Publicizer;

namespace CalamityAnomalies.Publicizer.CalamityMod;

#pragma warning disable IDE1006
public record GeneralParticleHandler_Publicizer : IPublicizer
{
    public static Type C_type => typeof(GeneralParticleHandler);

    // particles (static field)
    public static readonly FieldInfo s_f_particles = C_type.GetField("particles", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> particles
    {
        get => (List<Particle>)s_f_particles.GetValue(null);
        set => s_f_particles.SetValue(null, value);
    }

    // particlesToKill (static field)
    public static readonly FieldInfo s_f_particlesToKill = C_type.GetField("particlesToKill", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> particlesToKill
    {
        get => (List<Particle>)s_f_particlesToKill.GetValue(null);
        set => s_f_particlesToKill.SetValue(null, value);
    }

    // particleTypes (static field)
    public static readonly FieldInfo s_f_particleTypes = C_type.GetField("particleTypes", TOReflectionUtils.StaticBindingFlags);
    public static Dictionary<Type, int> particleTypes
    {
        get => (Dictionary<Type, int>)s_f_particleTypes.GetValue(null);
        set => s_f_particleTypes.SetValue(null, value);
    }

    // particleTextures (static field)
    public static readonly FieldInfo s_f_particleTextures = C_type.GetField("particleTextures", TOReflectionUtils.StaticBindingFlags);
    public static Dictionary<int, Texture2D> particleTextures
    {
        get => (Dictionary<int, Texture2D>)s_f_particleTextures.GetValue(null);
        set => s_f_particleTextures.SetValue(null, value);
    }

    // particleInstances (static field)
    public static readonly FieldInfo s_f_particleInstances = C_type.GetField("particleInstances", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> particleInstances
    {
        get => (List<Particle>)s_f_particleInstances.GetValue(null);
        set => s_f_particleInstances.SetValue(null, value);
    }

    // batchedAlphaBlendParticles (static field)
    public static readonly FieldInfo s_f_batchedAlphaBlendParticles = C_type.GetField("batchedAlphaBlendParticles", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> batchedAlphaBlendParticles
    {
        get => (List<Particle>)s_f_batchedAlphaBlendParticles.GetValue(null);
        set => s_f_batchedAlphaBlendParticles.SetValue(null, value);
    }

    // batchedNonPremultipliedParticles (static field)
    public static readonly FieldInfo s_f_batchedNonPremultipliedParticles = C_type.GetField("batchedNonPremultipliedParticles", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> batchedNonPremultipliedParticles
    {
        get => (List<Particle>)s_f_batchedNonPremultipliedParticles.GetValue(null);
        set => s_f_batchedNonPremultipliedParticles.SetValue(null, value);
    }

    // batchedAdditiveBlendParticles (static field)
    public static readonly FieldInfo s_f_batchedAdditiveBlendParticles = C_type.GetField("batchedAdditiveBlendParticles", TOReflectionUtils.StaticBindingFlags);
    public static List<Particle> batchedAdditiveBlendParticles
    {
        get => (List<Particle>)s_f_batchedAdditiveBlendParticles.GetValue(null);
        set => s_f_batchedAdditiveBlendParticles.SetValue(null, value);
    }

    // noteToEveryone (static field)
    public static readonly FieldInfo s_f_noteToEveryone = C_type.GetField("noteToEveryone", TOReflectionUtils.StaticBindingFlags);
    public static string noteToEveryone
    {
        get => (string)s_f_noteToEveryone.GetValue(null);
        set => s_f_noteToEveryone.SetValue(null, value);
    }

    // Load (static method)
    public static readonly MethodInfo s_m_Load = C_type.GetMethod("Load", TOReflectionUtils.StaticBindingFlags);
    public delegate void Orig_Load();
    public static readonly Orig_Load s_d_Load = s_m_Load.CreateDelegate<Orig_Load>();
    public static void Load() => s_d_Load();

    // Unload (static method)
    public static readonly MethodInfo s_m_Unload = C_type.GetMethod("Unload", TOReflectionUtils.StaticBindingFlags);
    public delegate void Orig_Unload();
    public static readonly Orig_Unload s_d_Unload = s_m_Unload.CreateDelegate<Orig_Unload>();
    public static void Unload() => s_d_Unload();
}
