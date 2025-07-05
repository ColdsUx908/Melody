namespace CalamityAnomalies.Core;

public class CAKeybinds : ModSystem
{
    public static ModKeybind ChangeYharimsGiftBuff { get; private set; }

    public override void Load()
    {
        ChangeYharimsGiftBuff = KeybindLoader.RegisterKeybind(Mod, nameof(ChangeYharimsGiftBuff), "NumPad0");
    }

    public override void Unload()
    {
        ChangeYharimsGiftBuff = null;
    }
}
