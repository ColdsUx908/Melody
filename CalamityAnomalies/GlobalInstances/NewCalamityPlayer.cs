
namespace CalamityAnomalies.GlobalInstances;

/// <summary>
/// 仅作存放 <see cref="CalamityPlayer"/> 类变量之用。
/// <br/>变量按字母顺序排列。
/// </summary>
public class NewCalamityPlayer : ModPlayer
{
    public override ModPlayer Clone(Player newEntity)
    {
        NewCalamityPlayer clone = (NewCalamityPlayer)base.Clone(newEntity);

        return clone;
    }

    public override void ResetEffects()
    {
    }
}
