using System.Reflection;

namespace TransoceanicCalamity;

public partial class TOCMain
{
    public static Assembly Assembly => TransoceanicCalamity.Instance.Code;

    public const string ModLocalizationPrefix = "Mods.TransoceanicCalamity.";
}
