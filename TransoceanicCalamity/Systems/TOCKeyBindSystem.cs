using Terraria.ModLoader;

namespace TransoceanicCalamity.Systems;

public class TOCKeyBindSystem : ModSystem
{
    public static ModKeybind AntiEPB { get; private set; }

    public override void Load()
    {
        AntiEPB = KeybindLoader.RegisterKeybind(Mod, "AntiEPB", "NumPad6");
    }

    public override void Unload()
    {
        AntiEPB = null;
    }
}
