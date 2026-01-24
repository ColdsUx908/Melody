using System.Diagnostics.CodeAnalysis;
using CalamityAnomalies.Publicizer.CalamityMod;
using CalamityMod.Particles;

namespace CalamityAnomalies.Core;

public static class CAUtils
{
    public static Dictionary<string, Type[]> CalamityTypes => field ??= CalamityMod_Publicizer.Instance.Code.GetTypes().GroupBy(t => t.Name).ToDictionary(g => g.Key, g => g.ToArray());
    public static Dictionary<string, Type> CalamityTypesByFullName => field ??= CalamityMod_Publicizer.Instance.Code.GetTypes().ToDictionary(t => t.FullName, t => t);

    public static Type GetCalamityType(string typeName)
    {
        if (CalamityTypes.TryGetValue(typeName, out Type[] types))
        {
            if (types.Length == 1)
                return types[0];
            else
                throw new ArgumentException($"More than one Calamity types '{typeName}' has been found.", nameof(typeName));
        }
        else
            throw new ArgumentException($"Type '{typeName}' not found in Calamity types.", nameof(typeName));
    }

    public static Type GetCalamityTypeByFullName(string fullTypeName)
    {
        if (CalamityTypesByFullName.TryGetValue(fullTypeName, out Type type))
            return type;
        else
            throw new ArgumentException($"Type '{fullTypeName}' not found in Calamity types.", nameof(fullTypeName));
    }

    public static void StopRain(bool force = false)
    {
        if (CalamityServerConfig.Instance.BossesStopWeather || force)
            CalamityMod_.StopRain();
    }

    public static bool IsDefeatingLeviathan(NPC npc) => npc.LeviathanBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.LeviathanBoss, npc).Any();

    public static bool IsDefeatingProfanedGuardians(NPC npc) => npc.ProfanedGuardianBoss && !TOIteratorFactory.NewActiveNPCIterator(n => n.ProfanedGuardianBoss, npc).Any();

    public static bool IsDefeatingExoMechs(NPC npc) =>
        npc.Ares && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Thanatos)
        || npc.ExoTwins && !NPC.ActiveNPCs.Any(n => !n.Ares && !n.Thanatos)
        || npc.ThanatosHead && !NPC.ActiveNPCs.Any(n => !n.ExoTwins && !n.Ares);

    public static bool DownedEvilBossT2 => DownedBossSystem.downedHiveMind || DownedBossSystem.downedPerforator;

    public static void ILFailure(string name, string reason, [DoesNotReturnIf(true)] bool exception = false)
    {
        string message = $"""[CA IL Editing] IL edit "{name}" failed! {reason}""";
        CAMain.Instance.Logger.Warn(message);
        if (exception)
            throw new InvalidOperationException(message);
    }

    public static TooltipLine CreateNewTooltipLine(int num, string text) => new(CAMain.Instance, $"Tooltip{num}", text);

    public static TooltipLine CreateNewTooltipLine(int num, string text, Color color) => new(CAMain.Instance, $"Tooltip{num}", text) { OverrideColor = color };

    public static TooltipLine CreateNewTooltipLine(int num, Action<TooltipLine> action)
    {
        TooltipLine newLine = new(CAMain.Instance, $"Tooltip{num}", "");
        action?.Invoke(newLine);
        return newLine;
    }

    public static Asset<Texture2D> RequestTexture(string suffix, AssetRequestMode mode = AssetRequestMode.AsyncLoad) =>
        CAMain.Instance.Assets.Request<Texture2D>("Assets/Textures/" + suffix, mode);

    public static bool Focus => DownedBossSystem.downedExoMechs && DownedBossSystem.downedCalamitas;

    public static void ForceSpawnParticle(Particle particle)
    {
        if (!Main.gamePaused && !Main.dedServ && GeneralParticleHandler_Publicizer.particles is not null)
        {
            GeneralParticleHandler_Publicizer.particles.Add(particle);
            particle.Type = GeneralParticleHandler_Publicizer.particleTypes[particle.GetType()];
        }
    }
}
