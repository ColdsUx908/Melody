namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(List<TooltipLine> tooltips)
    {
        public void ModifyTooltip(Func<TooltipLine, bool> match, Action<TooltipLine> action)
        {
            ArgumentNullException.ThrowIfNull(match);
            ArgumentNullException.ThrowIfNull(action);
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (match(line))
                {
                    action(line);
                    return;
                }
            }
        }

        public void ModifyVanillaTooltipByName(string name, Action<TooltipLine> action) =>
            tooltips.ModifyTooltip(l => l.Mod == "Terraria" && l.Name == name, action);

        public void ModifyTooltipByNum(int num, Action<TooltipLine> action) =>
            tooltips.ModifyVanillaTooltipByName($"Tooltip{num}", action);
    }
}