namespace CalamityAnomalies.Tweaks;

public enum CATweakPhase
{
    Beginning,
    PostEvil1,
    PostEvil2,
    PostSkeletron,
    Hardmode,
    PostMechanics,
    PostPlantera,
    PostGolem,
    PostCultist,
    PostMoonlord,
    PostProvidence,
    PostPolterghast,
    PostDoG,
    PostYharon,
    Focus,
}

internal interface ICATweakLocalizationPrefix : ILocalizationPrefix
{
    public abstract CATweakPhase Phase { get; }
    public abstract string Name { get; }

    string ILocalizationPrefix.LocalizationPrefix => CAMain.TweakLocalizationPrefix + Phase switch
    {
        CATweakPhase.Beginning => "1.1.",
        CATweakPhase.PostEvil1 => "1.2.",
        CATweakPhase.PostEvil2 => "1.3.",
        CATweakPhase.PostSkeletron => "1.4.",
        CATweakPhase.Hardmode => "2.1.",
        CATweakPhase.PostMechanics => "2.2.",
        CATweakPhase.PostPlantera => "3.1.",
        CATweakPhase.PostGolem => "3.2.",
        CATweakPhase.PostCultist => "3.3.",
        CATweakPhase.PostMoonlord => "4.1.",
        CATweakPhase.PostProvidence => "4.2.",
        CATweakPhase.PostPolterghast => "4.3.",
        CATweakPhase.PostDoG => "5.1.",
        CATweakPhase.PostYharon => "5.2.",
        CATweakPhase.Focus => "6.",
        _ => ""
    } + Name + ".";
}
