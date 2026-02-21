using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.World;

namespace CalamityAnomalies;

public sealed class CASharedData : ModSystem
{
    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";
    public const string AnomalyLocalizationPrefix = ModLocalizationPrefix + "Anomaly.";
    public const string TweakLocalizationPrefix = ModLocalizationPrefix + "Tweaks.";
    public const string CalamityModLocalizationPrefix = "Mods.CalamityMod.";
    public const string CalamityInvisibleProj = "CalamityMod/Projectiles/InvisibleProj";
    public const string CATexturePath = "CalamityAnomalies/Assets/Textures/";

    public static readonly Color MainColor = Color.HotPink;

    public static readonly Color SecondaryColor = Color.Pink;

    public static readonly List<Color> ColorList = [MainColor, SecondaryColor, MainColor];

    public static readonly Color AnomalyUltramundaneColor = new(0xE8, 0x97, 0xFF);

    public static Assembly Assembly => field ??= CAMain.Instance.Code;

    public static string ModName => field ??= CAMain.Instance.Name;

    public static Color GetGradientColor(float ratio = 0.5f) => Color.LerpMany(ColorList, TOMathUtils.GetTimeSin(ratio / 2f, unsigned: true));

    public static SoundStyle MetalPipeFalling = new("CalamityMod/Sounds/Custom/MetalPipeFalling");

    #region Sets
    public static bool[] TweakedNPCs { get; private set; }
    public static bool[] TweakedProjectiles { get; private set; }
    public static bool[] TweakedItems { get; private set; }

    public override void ResizeArrays()
    {
        TweakedNPCs = NPCID.Sets.Factory.CreateBoolSet(false);
        TweakedProjectiles = ProjectileID.Sets.Factory.CreateBoolSet(false);
        TweakedItems = ItemID.Sets.Factory.CreateBoolSet(false);
    }
    #endregion

    #region World
    /// <summary>
    /// 异象模式。
    /// </summary>
    public static bool Anomaly { get; internal set; }

    /// <summary>
    /// 异象超凡。
    /// </summary>
    public static bool AnomalyUltramundane { get; internal set; }

    /// <summary>
    /// 传奇复仇。
    /// </summary>
    public static bool LR => CalamityWorld.LegendaryMode && CalamityWorld.revenge;

    /// <summary>
    /// 传奇死亡。
    /// </summary>
    public static bool LD => CalamityWorld.LegendaryMode && CalamityWorld.death;

    /// <summary>
    /// 传奇复仇GFB。
    /// </summary>
    public static bool LRG => CalamityWorld.LegendaryMode && CalamityWorld.revenge && Main.zenithWorld;

    /// <summary>
    /// 传奇死亡GFB。
    /// </summary>
    public static bool LDG => CalamityWorld.LegendaryMode && CalamityWorld.death && Main.zenithWorld;

    public static bool PermaFrostActive
    {
        get
        {
            if (CalamityGlobalNPC.SCal != -1)
            {
                NPC supremeCalamitas = Main.npc[CalamityGlobalNPC.SCal];
                return supremeCalamitas.active && supremeCalamitas.GetModNPC<SupremeCalamitas>().permafrost;
            }
            return false;
        }
    }

    public override void OnWorldLoad()
    {
        AnomalyUltramundane = false;
    }

    public override void OnWorldUnload()
    {
        AnomalyUltramundane = false;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        tag["Anomaly"] = Anomaly;
    }

    public override void LoadWorldData(TagCompound tag)
    {
        Anomaly = tag.GetBool("Anomaly");
    }
    #endregion World
}


