using System;
using CalamityAnomalies;
using CalamityMod.CalPlayer;
using CalamityMod.Events;
using CalamityMod.Projectiles.Boss;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityAnomalies.Anomaly.KingSlime;

public class KingSlimeJewelSapphire : CAModNPC
{
    public const float DespawnDistance = 5000f;

    public bool HasEnteredPhase2
    {
        get => NPC.ai[2] == 1f;
        set => NPC.ai[2] = value.ToInt();
    }

    public bool CanAttack
    {
        get => NPC.ai[3] != 1f;
        set => NPC.ai[3] = (!value).ToInt();
    }

    public override string Texture => JewelHandler.JewelTexturePath;
    public override string LocalizationCategory => "Anomaly.KingSlime";

    public override void SetStaticDefaults() => NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, new NPCID.Sets.NPCBestiaryDrawModifiers() { Hide = true });

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        AIType = -1;
        NPC.damage = 10;
        NPC.width = 22;
        NPC.height = 22;
        NPC.defense = 5;
        NPC.DR_NERD(0.05f);

        NPC.lifeMax = 180;
        NPC.ApplyCalamityBossHealthBoost();

        NPC.knockBackResist = 0.9f;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.HitSound = SoundID.NPCHit5;
        NPC.DeathSound = SoundID.NPCDeath15;
        NPC.Calamity().VulnerableToSickness = false;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment) => NPC.lifeMax = (int)(NPC.lifeMax * balance);

    public override void AI()
    {
        if (!OceanNPC.TryGetMaster(NPCID.KingSlime, out NPC master))
        {
            JewelHandler.Despawn(NPC);
            return;
        }

        if (!NPC.TargetClosestIfInvalid(true, DespawnDistance))
        {
            NPC.Center = master.Top - new Vector2(0, master.height);
            return;
        }

        NPC.damage = 0;
        Lighting.AddLight(NPC.Center, 0f, 0f, 1f);

        JewelHandler.Move(NPC, master.Center, 15f, 15f, 0.2f, 0.5f, 150f, -150f, 50f, -250f);

        NPC.netUpdate = true;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        JewelHandler.DrawJewel(spriteBatch, screenPos, NPC, 0f);
        return false;
    }

    public override bool CheckActive() => false;

    public override bool CheckDead()
    {
        if (CASharedData.AnomalyUltramundane)
        {
            NPC.life = 1;
            NPC.active = true;
            if (!HasEnteredPhase2)
                JewelHandler.EnterPhase2(NPC);
            return false;
        }
        return true;
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        int dust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, hit.HitDirection, -1f, 0, default, 1f);
        Main.dust[dust].noGravity = true;

        if (NPC.life <= 0)
        {
            NPC.position = NPC.Center;
            NPC.width = NPC.height = 45;
            NPC.position.X = NPC.position.X - (NPC.width / 2);
            NPC.position.Y = NPC.position.Y - (NPC.height / 2);

            for (int i = 0; i < 2; i++)
            {
                int rubyDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                Main.dust[rubyDust].noGravity = true;
                Main.dust[rubyDust].velocity *= 3f;
                if (Main.rand.NextBool())
                {
                    Main.dust[rubyDust].scale = 0.5f;
                    Main.dust[rubyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }

            for (int j = 0; j < 10; j++)
            {
                int rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 3f);
                Main.dust[rubyDust2].noGravity = true;
                Main.dust[rubyDust2].velocity *= 5f;
                rubyDust2 = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GemSapphire, 0f, 0f, 100, default, 2f);
                Main.dust[rubyDust2].noGravity = true;
                Main.dust[rubyDust2].velocity *= 2f;
            }
        }
    }
}
