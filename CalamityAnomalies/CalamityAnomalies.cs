global using System;
global using System.Collections.Generic;
global using System.Collections.ObjectModel;
global using System.IO;
global using System.Linq;
global using System.Reflection;
global using System.Runtime.CompilerServices;
global using CalamityAnomalies.Configs;
global using CalamityAnomalies.GlobalInstances;
global using CalamityMod;
global using CalamityMod.CalPlayer;
global using CalamityMod.Items;
global using CalamityMod.NPCs;
global using CalamityMod.Projectiles;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using ReLogic.Content;
global using ReLogic.Graphics;
global using ReLogic.Utilities;
global using Terraria;
global using Terraria.Audio;
global using Terraria.DataStructures;
global using Terraria.GameContent;
global using Terraria.ID;
global using Terraria.Localization;
global using Terraria.ModLoader;
global using Terraria.ModLoader.IO;
global using Terraria.Utilities;
global using Transoceanic;
global using Transoceanic.Extensions;
global using Transoceanic.ExtraGameData;
global using Transoceanic.ExtraMathData;
global using Transoceanic.GameData;
global using Transoceanic.IL;
global using Transoceanic.Localization;
global using Transoceanic.MathHelp;
global using Transoceanic.Net;
global using Transoceanic.Visual;
global using ZLinq;
global using CalamityMod_ = CalamityMod.CalamityMod;

namespace CalamityAnomalies;

public class CalamityAnomalies : Mod
{
    internal static CalamityAnomalies Instance { get; private set; }

    public override void Load()
    {
        Instance = this;
    }

    public override void Unload()
    {
        Instance = null;
    }
}

public static class CAMain
{
    /// <summary>
    /// 是否启用了平衡修改。
    /// <br/>不要直接使用 <see cref="CAServerConfig.TweaksEnabled"/>。
    /// </summary>
    public static bool Tweak => CAServerConfig.Instance?.TweaksEnabled ?? false;

    public static Assembly Assembly { get; } = CalamityAnomalies.Instance.Code;

    public const string ModLocalizationPrefix = "Mods.CalamityAnomalies.";

    public const string TexturePrefix = "CalamityAnomalies/Textures/";

    public static Type Type_CalamityMod { get; } = typeof(CalamityMod_);

    public static CalamityMod_ CalamityModInstance { get; internal set; } = null;

    public static Color AnomalyUltramundaneColor { get; } = new(0xE8, 0x97, 0xFF);

    public class Load : ITOLoader
    {
        void ITOLoader.PostSetupContent()
        {
            CalamityModInstance = (CalamityMod_)Type_CalamityMod.GetField("Instance", TOReflectionUtils.StaticBindingFlags).GetValue(null);
            TOMain.SyncEnabled = true;
        }

        void ITOLoader.OnModUnload()
        {
            CalamityModInstance = null;
        }
    }
}