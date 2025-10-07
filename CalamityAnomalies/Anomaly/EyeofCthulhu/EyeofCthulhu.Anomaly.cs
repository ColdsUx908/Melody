namespace CalamityAnomalies.Anomaly.EyeofCthulhu;

public sealed class EyeofCthulhu_Anomaly : AnomalyNPCBehavior
{
    public override bool ShouldProcess => false; //暂时禁用

    public override int ApplyingType => NPCID.EyeofCthulhu;

    public static class Data
    {
        public const float DespawnDistance = 5000f;
    }
}
