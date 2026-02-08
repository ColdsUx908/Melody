using CalamityAnomalies.Assets.Textures;
using CalamityAnomalies.Publicizers.CalamityMod;
using CalamityMod.NPCs.NormalNPCs;
using Transoceanic.DataStructures.Particles;
using Transoceanic.Framework.Helpers.AbstractionHelpers;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class JewelHandler : IResourceLoader
{
    public const string JewelTexturePath = "CalamityMod/NPCs/NormalNPCs/KingSlimeJewelRuby";

    public static Color EmeraldColor => Main.zenithWorld ? Color.Purple : Color.FullGreen;
    public static Color RubyColor => Main.zenithWorld ? Color.Cyan : Color.Red;
    public static Color SapphireColor => Main.zenithWorld ? Color.Yellow : Color.Blue;
    public static Color RainbowColor => Main.DiscoColor;
    public static Color EmeraldFinalColor => Main.zenithWorld ? new(255, 175, 255) : new(175, 255, 175);
    public static Color RubyFinalColor => Main.zenithWorld ? new(175, 255, 255) : new(255, 175, 175);
    public static Color SapphireFinalColor => Main.zenithWorld ? new(255, 255, 175) : new(175, 175, 255);
    public static Color RainbowFinalColor => Color.Lerp(Main.DiscoColor, Color.White, 0.7f);

    public static Color GetColor(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelEmerald => EmeraldColor,
        KingSlimeJewelRuby => RubyColor,
        KingSlimeJewelSapphire => SapphireColor,
        KingSlimeJewelRainbow => RainbowColor,
        _ => Color.White
    };

    public static Color GetFinalColor(NPC jewel) => jewel.ModNPC switch
    {
        KingSlimeJewelEmerald => EmeraldFinalColor,
        KingSlimeJewelRuby => RubyFinalColor,
        KingSlimeJewelSapphire => SapphireFinalColor,
        KingSlimeJewelRainbow => RainbowFinalColor,
        _ => Color.White
    };

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
    /// <param name="destination">目标点。</param>
    /// <param name="maxVelocityX">水平速度最大值。</param>
    /// <param name="maxVelocityY">竖直速度最大值。</param>
    /// <param name="accelerationX">水平加速度。</param>
    /// <param name="accelerationY">竖直加速度。</param>
    /// <param name="safeDistanceXMax">水平安全距离（宝石中心X坐标减目标中心X坐标）最大值。当水平距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceXMin">水平安全距离（宝石中心X坐标减目标中心X坐标）最小值。当水平距离小于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMax">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最大值。当竖直距离大于此值时触发反向移动。</param>
    /// <param name="safeDistanceYMin">竖直安全距离（宝石中心Y坐标减目标中心Y坐标）最小值。当竖直距离小于此值时触发反向移动。</param>
    public static void Move(NPC jewel, Vector2 destination, float maxVelocityX, float maxVelocityY, float accelerationX, float accelerationY, float safeDistanceXMax, float safeDistanceXMin, float safeDistanceYMax, float safeDistanceYMin)
    {
        jewel.damage = 0;
        jewel.knockBackResist = 0.7f;
        jewel.rotation = jewel.velocity.X / 15f;
        Vector2 distance = jewel.Center - destination;
        float adjustedX = distance.X - (safeDistanceXMax + safeDistanceXMin) / 2f;
        float safeX = (safeDistanceXMax - safeDistanceXMin) / 2f;
        float adjustedY = distance.Y - (safeDistanceYMax + safeDistanceYMin) / 2f;
        float safeY = (safeDistanceYMax - safeDistanceYMin) / 2f;
        maxVelocityX = Utils.Remap(Math.Abs(adjustedX), safeX, safeX * 7f, maxVelocityX * 0.65f, maxVelocityX);
        maxVelocityY = Utils.Remap(Math.Abs(adjustedY), safeY, safeY * 7f, maxVelocityY * 0.65f, maxVelocityY, true);
        if (adjustedX > safeX)
        {
            if (jewel.velocity.X > 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X - accelerationX, -maxVelocityX, maxVelocityX);
        }
        else if (adjustedX < -safeX)
        {
            if (jewel.velocity.X < 0f)
                jewel.velocity.X *= 0.98f;
            jewel.velocity.X = Math.Clamp(jewel.velocity.X + accelerationX, -maxVelocityX, maxVelocityX);
        }
        if (adjustedY > safeY)
        {
            if (jewel.velocity.Y > 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y - accelerationY, -maxVelocityY, maxVelocityY);
        }
        else if (adjustedY < -safeY)
        {
            if (jewel.velocity.Y < 0f)
                jewel.velocity.Y *= 0.98f;
            jewel.velocity.Y = Math.Clamp(jewel.velocity.Y + accelerationY, -maxVelocityY, maxVelocityY);
        }
    }

    /// <summary>
    /// 宝石绘制通用逻辑。
    /// </summary>
    /// <param name="spriteBatch"><c>PreDraw</c> 方法中的 <c>spriteBatch</c> 参数。</param>
    /// <param name="screenPos"><c>PreDraw</c> 方法中的 <c>screenPos</c> 参数。</param>
    /// <param name="jewel">宝石。</param>
    /// <param name="lerpValue">插值比例。</param>
    public static void DrawJewel(SpriteBatch spriteBatch, Vector2 screenPos, NPC jewel, float lerpValue) =>
        spriteBatch.DrawFromCenter(jewel.Texture, jewel.Center - screenPos, (Color.Lerp(GetColor(jewel), GetFinalColor(jewel), lerpValue) * (CheckIfPhase2(jewel) ? 0.5f : 1f)) with { A = jewel.GraphicAlpha }, null, jewel.rotation, jewel.scale);

    public static void DrawAttackEffect(SpriteBatch spriteBatch, Vector2 screenPos, NPC jewel, float ratio, float radius, float scale)
    {
        bool isRainbowJewel = jewel.ModNPC is KingSlimeJewelRainbow;
        Color color = GetColor(jewel) with { A = 0 } * ratio * 1.5f;
        float interpolation = TOMathHelper.QuadraticEaseOut(1f - ratio);
        float interpolation2 = TOMathHelper.QuadraticEaseOut(Math.Clamp(1f - ratio, 0f, 0.2f) * 5f);
        for (int i = 0; i < 300; i++)
        {
            if (isRainbowJewel)
                color = Color.LerpMany(Color.RainbowColors, (ratio + i / 300f + TOMathHelper.GetTimeSin(0.5f, 0.5f, 0f, true)) % 1f) with { A = 0 } * ratio * 1.5f;
            spriteBatch.DrawFromCenter(OrbParticle.Texture, jewel.Center + new PolarVector2(radius * interpolation, MathHelper.TwoPi / 300 * i) - screenPos, color, null, 0f, scale * interpolation2);
        }
    }

    /// <summary>
    /// 宝石粒子生成通用逻辑。
    /// </summary>
    /// <param name="jewel">宝石。</param>
    /// <param name="velocity">粒子速度大小。</param>
    /// <param name="lifetime">粒子存在时长。</param>
    /// <param name="scale">粒子大小。</param>
    public static void SpawnParticle(NPC jewel, float velocity, int lifetime, float scale)
    {
        Color color = jewel.ModNPC switch
        {
            KingSlimeJewelRainbow => Color.GetRandomRainbowColor(),
            _ => GetColor(jewel)
        };
        Transoceanic.Framework.Helpers.AbstractionHelpers.ParticleHelper.SpawnParticle(new OrbParticle(jewel.Center, Main.rand.NextPolarVector2(velocity), lifetime, scale, color, lifeEndRatio: 0.925f));
    }

    public static void CreateDustFromJewelTo(NPC jewel, Vector2 destination, int type)
    {
        int maxDustIterations = (int)Vector2.Distance(jewel.Center, destination); //distance
        int maxDust = 100;
        int dustDivisor = Math.Max(maxDustIterations / maxDust, 2);

        Vector2 start = jewel.Center;
        Vector2 spinningpoint = new Vector2(0f, -1f).RotatedByRandom();
        for (int i = 0; i < maxDustIterations; i++)
        {
            if (i % dustDivisor == 0)
            {
                Vector2 position = Vector2.Lerp(start, destination, i / (float)maxDustIterations);
                Dust.NewDustAction(position, 0, 0, type, Vector2.Zero, d =>
                {
                    d.position = position;
                    d.velocity = spinningpoint.RotatedBy(MathHelper.TwoPi * i / maxDustIterations) * (0.9f + Main.rand.NextFloat() * 0.2f);
                    d.noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        d.scale = 0.5f;
                        d.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                    Dust d2 = Dust.CloneDust(d);
                    d2.scale *= 0.5f;
                    d2.fadeIn *= 0.5f;
                });
            }
        }
    }

    public static bool CheckIfPhase2(NPC jewel) => jewel.ai[2] > 0f;

    public static void EnterPhase2(NPC jewel)
    {
        for (int i = 0; i < 20; i++)
            SpawnParticle(jewel, Main.rand.NextFloat(2f, 2.5f), Main.rand.Next(20, 30), Main.rand.NextFloat(0.4f, 0.7f));
        SoundEngine.PlaySound(jewel.DeathSound, jewel.Center);
        jewel.ai[2] = 1f;
        jewel.dontTakeDamage = true;
        jewel.netUpdate = true;
    }

    public static void DisableAttack(NPC jewel)
    {
        jewel.ai[3] = 1f;
    }

    public static int GetRandomDustID() => Main.rand.Next(7) switch
    {
        0 => DustID.GemAmethyst,
        1 => DustID.GemTopaz,
        2 => DustID.GemSapphire,
        3 => DustID.GemEmerald,
        4 => DustID.GemRuby,
        5 => DustID.GemDiamond,
        6 => DustID.GemAmber,
        _ => DustID.TintableDust
    };
}