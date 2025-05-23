using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace Transoceanic.Core.GameData;

public static class TONPCUtils
{
    public static bool IsFriendly(this NPC npc) => npc.active && (npc.friendly || npc.townNPC || npc.lifeMax <= 5);

    public static bool IsEnemy(this NPC npc) => !npc.friendly && npc.lifeMax >= 5;

    /// <summary>
    /// 检查NPC是否为Boss。
    /// </summary>
    /// <param name="npc"></param>
    /// <returns></returns>
    public static bool IsBossTO(this NPC npc) => npc.boss || npc.IsEoW() || npc.IsWallOfFleshEye();

    public static bool IsEoW(this NPC npc) => npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

    public static bool IsWallOfFleshEye(this NPC npc) => npc.type == NPCID.WallofFleshEye;

    public static bool IsDestroyer(this NPC npc) => npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;

    public static bool IsRetinazer(this NPC npc) => npc.type == NPCID.Retinazer;

    public static bool IsSpazmatism(this NPC npc) => npc.type == NPCID.Spazmatism;

    public static bool IsDefeatingTwins(NPC npc) => npc.type switch
    {
        NPCID.Retinazer => !TOMain.ActiveNPCs.Any(IsSpazmatism),
        NPCID.Spazmatism => !TOMain.ActiveNPCs.Any(IsRetinazer),
        _ => false
    };

    public static int GetTargetDirection(this NPC npc) => Math.Sign((npc.HasNPCTarget ? Main.projectile[npc.target - 300] : (Entity)Main.player[npc.target]).Center.X - npc.Center.X) switch
    {
        -1 => -1,
        _ => 1
    };

    public static bool IsFacingTarget(this NPC npc) => npc.direction == npc.GetTargetDirection();

    public static void FaceTargetTO(this NPC npc, Entity target)
    {
        npc.direction = Math.Sign(target.Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };
    }

    /// <summary>
    /// 将NPC速度设置为指定值，同时更新旋转。
    /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="velocity"></param>
    public static void SetVelocityandRotation(this NPC npc, Vector2 velocity)
    {
        npc.velocity = velocity;
        npc.VelocityToRotation();
    }

    /// <summary>
    /// 适用于贴图方向向上的NPC，用于将 <see cref="GameData.velocity"/> 转换为 <see cref="NPC.rotation"/>，并应用于NPC。
    /// </summary>
    public static void VelocityToRotation(this NPC npc)
    {
        float temp = npc.velocity.ToRotation();

        npc.rotation = temp switch
        {
            0f => 0f,
            _ => MathHelper.PiOver2 + temp
        };
    }

    /// <summary>
    /// 如果现有目标无效，获取新的目标。
    /// </summary>
    /// <param name="npc"></param>
    /// <param name="faceTarget"></param>
    /// <param name="distanceThreshold"></param>
    /// <returns>获取目标后的目标是否有效。</returns>
    public static bool TargetClosestIfInvalid(this NPC npc, bool faceTarget = true, float distanceThreshold = 4000f)
    {
        if (!npc.HasValidTarget)
            npc.TargetClosest(faceTarget);

        Player target = Main.player[npc.target];

        if (distanceThreshold >= 0f && !npc.WithinRange(target.Center, distanceThreshold - target.aggro))
            npc.TargetClosest(faceTarget);

        return npc.HasValidTarget && npc.WithinRange(target.Center, distanceThreshold - target.aggro);
    }

    public static void SafeChangeScale(this NPC npc, int width, int height, float scale)
    {
        if (npc.scale == scale)
            return;

        npc.position.X += npc.width / 2;
        npc.position.Y += npc.height;
        npc.scale = scale;
        npc.width = (int)(width * npc.scale);
        npc.height = (int)(height * npc.scale);
        npc.position.X -= npc.width / 2;
        npc.position.Y -= npc.height;
    }
}
