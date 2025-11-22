using Terraria.Graphics.Effects;

namespace CalamityAnomalies.Anomaly.KingSlime;

public sealed class RainbowShield : CAModProjectile
{
    public NPC NPC;

    public override string Texture => CAMain.CalamityInvisibleProj;
    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.penetrate = -1;
        Projectile.hostile = true;
        Projectile.timeLeft = 30;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        if (NPC is not null && NPC.active && NPC.ModNPC is KingSlimeJewelRainbow)
        {
            Timer1 = Math.Clamp(Timer1 + 1, 0, 30);
            Projectile.timeLeft = 30;
            Projectile.Center = NPC.Center;
        }
        else
            Timer1 = Math.Clamp(Timer1 - 1, 0, 30);
    }

    //待修改
    public override bool PreDraw(ref Color lightColor)
    {
        /*
        SpriteBatch spriteBatch = Main.spriteBatch;
        Vector2 screenPos = Main.screenPosition;

        float maxOscillation = 60f;
        float minScale = 0.9f;
        float maxPulseScale = 1f - minScale;
        float minOpacity = 0.5f;
        float maxOpacityScale = 1f - minOpacity;
        float currentOscillation = MathHelper.Lerp(0f, maxOscillation, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi) + 1f) * 0.5f);
        float shieldOpacity = minOpacity + maxOpacityScale * Utils.Remap(currentOscillation, 0f, maxOscillation, 1f, 0f);
        float oscillationRatio = currentOscillation / maxOscillation;
        float invertedOscillationRatio = 1f - (1f - oscillationRatio) * (1f - oscillationRatio);
        float oscillationScale = 1f - (1f - invertedOscillationRatio) * (1f - invertedOscillationRatio);
        float remappedOscillation = Utils.Remap(currentOscillation, maxOscillation - 15f, maxOscillation, 0f, 1f);
        float twoOscillationsMultipliedTogetherForScaleCalculation = remappedOscillation * remappedOscillation;
        float invertedOscillationUsedForScale = MathHelper.Lerp(minScale, 1f, 1f - twoOscillationsMultipliedTogetherForScaleCalculation);
        float shieldScale = (minScale + maxPulseScale * oscillationScale) * invertedOscillationUsedForScale;
        float smallerRemappedOscillation = Utils.Remap(currentOscillation, 20f, maxOscillation, 0f, 1f);
        float invertedSmallerOscillationRatio = 1f - (1f - smallerRemappedOscillation) * (1f - smallerRemappedOscillation);
        float smallerOscillationScale = 1f - (1f - invertedSmallerOscillationRatio) * (1f - invertedSmallerOscillationRatio);
        float shieldScale2 = (minScale + maxPulseScale * smallerOscillationScale) * invertedOscillationUsedForScale;
        Texture2D shieldTexture = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleOpenCircleButBigger").Value;
        Rectangle shieldFrame = shieldTexture.Frame();
        Vector2 origin = shieldFrame.Size() * 0.5f;
        Vector2 shieldDrawPos = NPC.Center - screenPos;
        shieldDrawPos -= new Vector2(shieldTexture.Width, shieldTexture.Height) * NPC.scale / 2f;
        shieldDrawPos += origin * NPC.scale + new Vector2(0f, NPC.gfxOffY);
        float minHue = 0.06f;
        float maxHue = 0.18f;
        float opacityScaleDuringShieldDespawn = Timer1 / 30f;
        float scaleDuringShieldDespawnScale = 1.8f;
        float scaleDuringShieldDespawn = (1f - opacityScaleDuringShieldDespawn) * scaleDuringShieldDespawnScale;
        float colorScale = MathHelper.Lerp(0f, shieldOpacity, opacityScaleDuringShieldDespawn);
        Color color = Main.hslToRgb(MathHelper.Lerp(maxHue - minHue, maxHue, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.TwoPi) + 1f) * 0.5f), 1f, 0.5f) * colorScale;
        Color color2 = Main.hslToRgb(MathHelper.Lerp(minHue, maxHue - minHue, ((float)Math.Sin(Main.GlobalTimeWrappedHourly * MathHelper.Pi * 3f) + 1f) * 0.5f), 1f, 0.5f) * colorScale;
        color2.A = 0;
        color *= 0.6f;
        color2 *= 0.6f;
        float scaleMult = 2.75f + scaleDuringShieldDespawn;
        spriteBatch.Draw(shieldTexture, shieldDrawPos, shieldFrame, color2, NPC.rotation, origin, shieldScale2 * scaleMult * 0.45f, SpriteEffects.None, 0f);
        spriteBatch.Draw(shieldTexture, shieldDrawPos, shieldFrame, color2, NPC.rotation, origin, shieldScale2 * scaleMult * 0.5f, SpriteEffects.None, 0f);

        // The shield for the border MUST be drawn before the main shield, it becomes incredibly visually obnoxious otherwise.

        // The scale used for the noise overlay polygons also grows and shrinks
        // This is intentionally out of sync with the shield, and intentionally desynced per player
        // Don't put this anywhere less than 0.25f or higher than 1f. The higher it is, the denser / more zoomed out the noise overlay is.
        float noiseScale = MathHelper.Lerp(0.4f, 0.8f, (float)Math.Sin(Main.GlobalTimeWrappedHourly * 0.3f) * 0.5f + 0.5f);

        // Define shader parameters
        Effect shieldEffect = Filters.Scene["CalamityMod:RoverDriveShield"].GetShader().Shader;
        shieldEffect.Parameters["time"].SetValue(Main.GlobalTimeWrappedHourly * 0.058f); // Scrolling speed of polygonal overlay
        shieldEffect.Parameters["blowUpPower"].SetValue(2.8f);
        shieldEffect.Parameters["blowUpSize"].SetValue(0.4f);
        shieldEffect.Parameters["noiseScale"].SetValue(noiseScale);

        shieldEffect.Parameters["shieldOpacity"].SetValue(opacityScaleDuringShieldDespawn);
        shieldEffect.Parameters["shieldEdgeBlendStrenght"].SetValue(4f);

        Color edgeColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly * 0.2f, color, color2);

        // Define shader parameters for shield color
        shieldEffect.Parameters["shieldColor"].SetValue(color.ToVector3());
        shieldEffect.Parameters["shieldEdgeColor"].SetValue(edgeColor.ToVector3());

        Main.spriteBatch.End();
        Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shieldEffect, Main.GameViewMatrix.TransformationMatrix);

        // Fetch shield heat overlay texture (this is the neutrons fed to the shader)
        Texture2D heatTex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons2").Value;
        Vector2 pos = NPC.Center + NPC.gfxOffY * Vector2.UnitY - Main.screenPosition;
        Main.spriteBatch.Draw(heatTex, shieldDrawPos, null, Color.White, 0, heatTex.Size() / 2f, shieldScale * scaleMult * 0.5f, 0, 0);

        */

        return false;
    }
}
