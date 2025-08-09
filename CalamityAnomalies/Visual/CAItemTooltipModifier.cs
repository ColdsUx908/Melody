namespace CalamityAnomalies.Visual;

public class CAItemTooltipModifier : ItemTooltipModifier
{
    private int _nextCATooltipNum;
    private int _nextCATooltipIndex;

    public CAItemTooltipModifier(List<TooltipLine> tooltips) : base(tooltips) => _nextCATooltipIndex = _tooltipLast + 1;

    public override CAItemTooltipModifier Modify(int num, Action<TooltipLine> action) => (CAItemTooltipModifier)base.Modify(num, action);

    public override CAItemTooltipModifier Hide(int num) => (CAItemTooltipModifier)base.Hide(num);

    public CAItemTooltipModifier ModifyWithCATweakColor(int num, Action<TooltipLine> action) => Modify(num, l =>
    {
        l.OverrideColor = CAMain.GetGradientColor(0.25f);
        action(l);
    });

    public CAItemTooltipModifier ClearAllCATooltips()
    {
        Tooltips.RemoveAll(line => line.Mod == CAMain.ModName && line.Name.StartsWith("Tooltip"));
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(Action<TooltipLine> action, bool tweak = true)
    {
        Tooltips.Insert(_nextCATooltipIndex++, CAUtils.CreateNewTooltipLine(_nextCATooltipNum++, action, tweak));
        return this;
    }

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, l => l.Text = localizationPrefixProvider.GetTextValueWithPrefix($"Tooltip{num}"));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, l => l.Text = localizationPrefixProvider.GetTextFormatWithPrefix($"Tooltip{num}", args));

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider)
    {
        int num = _nextCATooltipIndex;
        return AddCATooltip(l => l.Text = localizationPrefixProvider.GetTextValueWithPrefix($"CATooltip{num}"));
    }

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args)
    {
        int num = _nextCATooltipIndex;
        return AddCATooltip(l => l.Text = localizationPrefixProvider.GetTextFormatWithPrefix($"CATooltip{num}", args));
    }
}
