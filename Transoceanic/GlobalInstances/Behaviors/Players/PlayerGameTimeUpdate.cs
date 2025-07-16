namespace Transoceanic.GlobalInstances.Behaviors.Players;

public sealed class PlayerGameTimeUpdate : TOPlayerBehavior
{
    public override void PreUpdate() => OceanPlayer.GameTime++;

    public override void OnEnterWorld() => OceanPlayer.GameTime = 0;
}
