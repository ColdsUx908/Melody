namespace CalamityAnomalies.Visual;

public class CAItemTooltipModifier : ItemTooltipModifier
{
    private int _nextCATooltipNum = 0;
    private int _nextCATooltipIndex;

    public CAItemTooltipModifier(List<TooltipLine> tooltips) : base(tooltips) => _nextCATooltipIndex = _tooltipLast + 1;

    public override CAItemTooltipModifier Modify(int num, Action<TooltipLine> action) => (CAItemTooltipModifier)base.Modify(num, action);

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
}
