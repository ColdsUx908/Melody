using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalamityAnomalies.Systems;

public class CAKeybindSystem : ModSystem
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
