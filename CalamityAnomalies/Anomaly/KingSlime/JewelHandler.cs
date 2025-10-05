using CalamityAnomalies.GameContents;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;

namespace CalamityAnomalies.Anomaly.KingSlime;

public static class JewelHandler
{
    public const string JewelTexturePath = "CalamityMod/NPCs/NormalNPCs/KingSlimeJewel";

    /// <summary>
    /// 宝石脱战通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    public static void Despawn(NPC jewel)
    {
        jewel.life = 0;
        jewel.HitEffect();
        jewel.active = false;
        jewel.netUpdate = true;
    }

    /// <summary>
    /// 宝石跟随目标通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    /// <param name="target">目标。</param>
    /// <param name="maxVelocityX">水平速度最大值。</param>
    /// <param name="maxVelocityY">竖直速度最大值。</param>
    /// <param name="acceleration">加速度。</param>
    /// <param name="safeDistanceXMax">水平安全距离（宝石中心X坐标减目标中心X坐标）最大值。当水平距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceXMin">水平安全距离（宝石中心X坐标减目标中心X坐标）最小值。当水平距离小于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMax">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最大值。当竖直距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMin">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最小值。当竖直距离小于此值时触发反向移动。</param>
    public static void Movement(NPC jewel, Vector2 destination, float maxVelocityX, float maxVelocityY, float acceleration, float safeDistanceXMax, float safeDistanceXMin, float safeDistanceYMax, float safeDistanceYMin)
    {
        jewel.damage = 0;
        jewel.knockBackResist = 0.7f;
        jewel.rotation = jewel.velocity.X / 15f;
        Vector2 distance = jewel.Center - destination;
        if (distance.X > safeDistanceXMax)
        {
            if (jewel.velocity.X > 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X - acceleration, -maxVelocityX, maxVelocityX);
        }
        else if (distance.X < safeDistanceXMin)
        {
            if (jewel.velocity.X < 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X + acceleration, -maxVelocityX, maxVelocityX);
        }
        if (distance.Y > safeDistanceYMax)
        {

            if (jewel.velocity.Y > 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y - acceleration, -maxVelocityY, maxVelocityY);
        }
        else if (distance.Y < safeDistanceYMin)
        {
            if (jewel.velocity.Y < 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y + acceleration, -maxVelocityY, maxVelocityY);
        }
    }

    /// <summary>
    /// 宝石绘制通用逻辑。
    /// </summary>
    /// <param name="spriteBatch"><c>PreDraw</c> 方法中的 <c>spriteBatch</c> 参数。</param>
    /// <param name="screenPos"><c>PreDraw</c> 方法中的 <c>screenPos</c> 参数。</param>
    /// <param name="jewel">宝石。</param>
    /// <param name="initialColor">正常颜色。</param>
    /// <param name="finalColor">转变颜色。</param>
    /// <param name="lerpValue">插值比例。</param>
    public static void DrawJewel(SpriteBatch spriteBatch, Vector2 screenPos, NPC jewel, Color initialColor, Color finalColor, float lerpValue) =>
        spriteBatch.DrawFromCenter(jewel.Texture, jewel.Center - screenPos, Color.Lerp(initialColor, finalColor, lerpValue) with { A = jewel.GraphicAlpha }, null, jewel.rotation, jewel.scale);

    /// <summary>
    /// 宝石粒子生成通用逻辑。
    /// </summary>
    /// <param name="amount">生成粒子数量。</param>
    /// <param name="jewel">宝石。</param>
    /// <param name="velocity">粒子速度大小。</param>
    /// <param name="lifetime">粒子存在时长。</param>
    /// <param name="scale">粒子大小。</param>
    public static void SpawnParticle(NPC jewel, float velocity, int lifetime, float scale)
    {
        Color color = jewel.ModNPC switch
        {
            KingSlimeJewelEmerald => Main.zenithWorld ? Color.Purple : Color.RealGreen,
            KingSlimeJewelRuby => Main.zenithWorld ? Color.Cyan : Color.Red,
            KingSlimeJewelSapphire => Main.zenithWorld ? Color.Yellow : Color.Blue,
            _ => Color.White
        };

        GeneralParticleHandler.SpawnParticle(new FadingGlowOrbParticle(jewel.Center, Main.rand.NextPolarVector2(velocity), 0f, lifetime, 0.925f, scale, color));
    }
}
