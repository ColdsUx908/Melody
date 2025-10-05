namespace CalamityAnomalies.Visual;

public sealed class CAItemTooltipModifier : ItemTooltipModifier
{
    public const string CATooltip = "CATooltip";

    public int _nextCATooltipNum;
    public int _nextCATooltipIndex;
    public bool _haveClearedCATooltip;
    public bool _haveAddedCATooltip;

    public bool Valid => _nextCATooltipIndex != -1;
    public bool ShouldUpdateBeforeModify => _haveClearedCATooltip || _haveAddedCATooltip;
    public bool ShouldUpdateBeforeAdd => _haveClearedCATooltip;

    public CAItemTooltipModifier(Item item, List<TooltipLine> tooltips) : base(item, tooltips) => UpdateCA();

    public void UpdateCA()
    {
        for (int i = _tooltips.Count - 1; i >= 0; i--)
        {
            TooltipLine line = _tooltips[i];
            if (line.Mod == CAMain.ModName && line.Name.StartsWith(CATooltip) && int.TryParse(line.Name[CATooltip.Length..], out int index))
            {
                _nextCATooltipIndex = i + 1;
                _nextCATooltipNum = index + 1;
                return;
            }
            if (line.Mod == "Terraria" && line.Name.StartsWith(Tooltip))
            {
                _nextCATooltipIndex = i + 1;
                _nextCATooltipNum = 0;
                return;
            }
        }
        _nextCATooltipIndex = -1;
        return;
    }

    public void Update()
    {
        UpdateDictionary();
        UpdateCA();
        _haveClearedCATooltip = _haveAddedCATooltip = false;
    }

    public override CAItemTooltipModifier Modify(string mod, string name, string newText)
    {
        if (ShouldUpdateBeforeModify)
            Update();
        Modify_Inner(mod, name, newText);
        return this;
    }

    public override CAItemTooltipModifier Modify(string mod, string name, string newText, Color newColor)
    {
        if (ShouldUpdateBeforeModify)
            Update();
        Modify_Inner(mod, name, newText, newColor);
        return this;
    }

    public override CAItemTooltipModifier Modify(string mod, string name, Action<TooltipLine> action)
    {
        if (ShouldUpdateBeforeModify)
            Update();
        Modify_Inner(mod, name, action);
        return this;
    }

    public override CAItemTooltipModifier ModifyTooltip(int num, string newText) => Modify(null, $"{Tooltip}{num}", newText);

    public override CAItemTooltipModifier ModifyTooltip(int num, string newText, Color newColor) => Modify(null, $"{Tooltip}{num}", newText, newColor);

    public override CAItemTooltipModifier ModifyTooltip(int num, Action<TooltipLine> action) => Modify(null, $"{Tooltip}{num}", action);

    public CAItemTooltipModifier ModifyWithCATweakColor(int num, string newText) => ModifyTooltip(num, newText, CAMain.GetGradientColor(0.25f));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextValue($"{Tooltip}{num}"));

    public CAItemTooltipModifier ModifyWithCATweakColorDefault(ILocalizationPrefix localizationPrefixProvider, int num, params object[] args) =>
        ModifyWithCATweakColor(num, localizationPrefixProvider.GetTextFormat($"{Tooltip}{num}", args));

    public CAItemTooltipModifier ClearAllCATooltips()
    {
        _tooltips.RemoveAll(line => line.Mod == CAMain.ModName && line.Name.StartsWith(CATooltip));
        _haveClearedCATooltip = true;
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(string text)
    {
        if (Valid)
        {
            if (ShouldUpdateBeforeAdd)
                Update();
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, text));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
            _haveAddedCATooltip = true;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(string text, Color color)
    {
        if (Valid)
        {
            if (ShouldUpdateBeforeAdd)
                Update();
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, text, color));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
            _haveAddedCATooltip = true;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltip(Action<TooltipLine> action)
    {
        if (Valid)
        {
            if (ShouldUpdateBeforeAdd)
                Update();
            _tooltips.Insert(_nextCATooltipIndex, CAUtils.CreateNewTooltipLine(_nextCATooltipNum, action));
            _nextCATooltipIndex++;
            _nextCATooltipNum++;
            _haveAddedCATooltip = true;
        }
        return this;
    }

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextFormat($"{CATooltip}{_nextCATooltipIndex}", args));

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor) => AddCATooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"), newColor);

    public CAItemTooltipModifier AddCATooltipDefault(ILocalizationPrefix localizationPrefixProvider, Color newColor, params object[] args) => AddCATooltip(localizationPrefixProvider.GetTextFormat($"{CATooltip}{_nextCATooltipIndex}", args), newColor);

    public CAItemTooltipModifier AddCATweakTooltip(string text) => AddCATooltip(text, CAMain.GetGradientColor(0.25f));

    public CAItemTooltipModifier AddCATweakTooltip(Action<TooltipLine> action) =>
        AddCATooltip(l =>
        {
            l.OverrideColor = CAMain.GetGradientColor(0.25f);
            action?.Invoke(l);
        });

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider) => AddCATweakTooltip(localizationPrefixProvider.GetTextValue($"{CATooltip}{_nextCATooltipIndex}"));

    public CAItemTooltipModifier AddCATweakTooltipDefault(ILocalizationPrefix localizationPrefixProvider, params object[] args) => AddCATweakTooltip(localizationPrefixProvider.GetTextFormat($"{CATooltip}{_nextCATooltipIndex}", args));

    public CAItemTooltipModifier AddExpendedDisplayLine() => AddCATooltip(CalamityUtils.GetTextValue("Misc.ShiftToExpand"), new Color(0xBE, 0xBE, 0xBE));
}
