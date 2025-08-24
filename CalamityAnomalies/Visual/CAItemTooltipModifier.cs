namespace CalamityAnomalies.Visual;

public class CAItemTooltipModifier : ItemTooltipModifier
{
    internal static new readonly CAItemTooltipModifier Instance = new();

    private int _nextCATooltipNum;
    private int _nextCATooltipIndex;

    public override CAItemTooltipModifier Update(List<TooltipLine> tooltips) => (CAItemTooltipModifier)base.Update(tooltips);

    protected override void UpdateInner(List<TooltipLine> tooltips)
    {
        base.UpdateInner(tooltips);
        _nextCATooltipIndex = _tooltipLast + 1;
    }

    public override CAItemTooltipModifier Modify(int num, Action<TooltipLine> action) => (CAItemTooltipModifier)base.Modify(num, action);

    public override CAItemTooltipModifier Hide(int num) => (CAItemTooltipModifier)base.Hide(num);

    public CAItemTooltipModifier ModifyWithCATweakColor(int num, Action<TooltipLine> action) => Modify(num, l =>
    {
        l.OverrideColor = CAMain.GetGradientColor(0.25f);
        action(l);
    });

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, l => l.Text = localizationPrefixProvider.GetTextValueWithPrefix($"Tooltip{num}"));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, l => l.Text = localizationPrefixProvider.GetTextFormatWithPrefix($"Tooltip{num}", args));

    public CAItemTooltipModifier ClearAllCATooltips()
    {
        Tooltips.RemoveAll(line => line.Mod == CAMain.ModName && line.Name.StartsWith("Tooltip"));
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(Action<TooltipLine> action)
    {
        Tooltips.Insert(_nextCATooltipIndex++, CAUtils.CreateNewTooltipLine(_nextCATooltipNum++, action));
        return this;
    }

    public CAItemTooltipModifier AddCATweakTooltip(Action<TooltipLine> action) =>
        AddCATooltip(l =>
        {
            l.OverrideColor = CAMain.GetGradientColor(0.25f);
            action?.Invoke(l);
        });

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider)
    {
        int num = _nextCATooltipIndex;
        return AddCATweakTooltip(l => l.Text = localizationPrefixProvider.GetTextValueWithPrefix($"CATooltip{num}"));
    }

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args)
    {
        int num = _nextCATooltipIndex;
        return AddCATweakTooltip(l => l.Text = localizationPrefixProvider.GetTextFormatWithPrefix($"CATooltip{num}", args));
    }

    public CAItemTooltipModifier AddExpendedDisplayLine() => AddCATooltip(l =>
    {
        l.Text = CalamityUtils.GetTextValue("Misc.ShiftToExpand");
        l.OverrideColor = new Color(0xBE, 0xBE, 0xBE);
    });
}
