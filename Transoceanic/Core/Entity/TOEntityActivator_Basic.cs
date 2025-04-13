using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;

namespace Transoceanic.Core;

/// <summary>
/// 提供更方便的实体生成方法。
/// </summary>
public static partial class TOEntityActivator
{
    /// <summary>
    /// 生成一个新的NPC，并在生成后执行一个Action。
    /// </summary>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="Type">类型。</param>
    /// <param name="Start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    public static void NewNPCAction(IEntitySource source, Vector2 position, int Type, int Start = 0, Action<NPC> action = null)
    {
        int index = NPC.NewNPC(source, (int)position.X, (int)position.Y, Type, Start);
        if (index < Main.maxNPCs)
            action?.Invoke(Main.npc[index]);
    }

    /// <summary>
    /// 生成一个新的NPC，并在生成后执行一个Action。
    /// </summary>
    /// <param name="index">输出的NPC索引。</param>
    /// <param name="npc">输出的NPC实例。</param>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="type">类型。</param>
    /// <param name="Start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    /// <returns>生成NPC是否成功。</returns>
    public static bool NewNPCActionCheck(out int index, out NPC npc, IEntitySource source, Vector2 position, int type, int Start = 0, Action<NPC> action = null)
    {
        index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, Start);
        if (index < Main.maxNPCs)
        {
            npc = Main.npc[index];
            action?.Invoke(npc);
            return true;
        }
        else
        {
            npc = null;
            return false;
        }
    }

    /// <summary>
    /// 生成一个新的Projectile，并在生成后执行一个Action。
    /// </summary>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="velocity">速度。</param>
    /// <param name="type">类型。</param>
    /// <param name="damage">伤害。</param>
    /// <param name="knockback">击退。</param>
    /// <param name="owner">弹幕主人。</param>
    /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
    public static void NewProjectileAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
    {
        int index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
        if (index < Main.maxProjectiles)
            action?.Invoke(Main.projectile[index]);
    }

    /// <summary>
    /// 生成一个新的Projectile，并在生成后执行一个Action。
    /// </summary>
    /// <param name="index">输出的Projectile索引。</param>
    /// <param name="projectile">输出的Projectile实例。</param>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="velocity">速度。</param>
    /// <param name="type">类型。</param>
    /// <param name="damage">伤害。</param>
    /// <param name="knockback">击退。</param>
    /// <param name="owner">弹幕主人。</param>
    /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
    /// <returns></returns>
    public static bool NewProjectileActionCheck(out int index, out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
    {
        index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
        if (index < Main.maxProjectiles)
        {
            projectile = Main.projectile[index];
            action?.Invoke(projectile);
            return true;
        }
        else
        {
            projectile = null;
            return false;
        }
    }

    /// <summary>
    /// 生成一个新的Dust，并在生成后执行一个Action。
    /// </summary>
    /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
    /// <param name="offsetX">X偏移最大值。</param>
    /// <param name="offsetY">Y偏移最大值。</param>
    /// <param name="type">类型。</param>
    /// <param name="alpha">透明度。</param>
    /// <param name="newColor">覆盖颜色。</param>
    /// <param name="scale">尺寸。</param>
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    public static void NewDustAction(Vector2 position, int offsetX, int offsetY, int type, int alpha = 0, Color newColor = default, Action<Dust> action = null)
    {
        int index = Dust.NewDust(position - new Vector2(offsetX, offsetY), offsetX * 2, offsetY * 2, type, Alpha: alpha, newColor: newColor);
        if (index < Main.maxDust)
            action?.Invoke(Main.dust[index]);
    }

    /// <summary>
    /// 生成一个新的Dust，并在生成后执行一个Action。
    /// </summary>
    /// <param name="index">输出的Dust索引。</param>
    /// <param name="dust">输出的Dust实例。</param>
    /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
    /// <param name="offsetX">X偏移最大值。</param>
    /// <param name="offsetY">Y偏移最大值。</param>
    /// <param name="type">类型。</param>
    /// <param name="alpha">透明度。</param>
    /// <param name="newColor">覆盖颜色。</param>
    /// <param name="scale">尺寸。</param>
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    /// <returns></returns>
    public static bool NewDustActionCheck(out int index, out Dust dust, Vector2 position, int offsetX, int offsetY, int type, int alpha = 0, Color newColor = default, Action<Dust> action = null)
    {
        index = Dust.NewDust(position - new Vector2(offsetX, offsetY), offsetX * 2, offsetY * 2, type, Alpha: alpha, newColor: newColor);
        if (index < Main.maxDust)
        {
            dust = Main.dust[index];
            action?.Invoke(dust);
            return true;
        }
        else
        {
            dust = null;
            return false;
        }
    }

    /// <summary>
    /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
    /// </summary>
    /// <param name="position">生成位置。</param>
    /// <param name="type">类型。</param>
    /// <param name="velocity">速度。</param>
    /// <param name="alpha">透明度。</param>
    /// <param name="newColor">覆盖颜色。</param>
    /// <param name="scale">尺寸。</param>
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    public static void NewDustPerfectAction(Vector2 position, int type, Vector2 velocity, int alpha = 0, Color newColor = default, float scale = 1f, Action<Dust> action = null)
    {
        Dust dustSpawned = Dust.NewDustPerfect(position, type, velocity, alpha, newColor, scale);
        if (dustSpawned.dustIndex < Main.maxDust)
            action?.Invoke(dustSpawned);
    }

    /// <summary>
    /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
    /// </summary>
    /// <param name="index">输出的Dust索引。</param>
    /// <param name="dust">输出的Dust实例。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="type">类型。</param>
    /// <param name="velocity">速度。</param>
    /// <param name="alpha">透明度。</param>
    /// <param name="newColor">覆盖颜色。</param>
    /// <param name="scale">尺寸。</param>
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    /// <returns></returns>
    public static bool NewDustPerfectActionCheck(out int index, out Dust dust, Vector2 position, int type, Vector2 velocity, int alpha = 0, Color newColor = default, float scale = 1f, Action<Dust> action = null)
    {
        Dust dustSpawned = Dust.NewDustPerfect(position, type, velocity, alpha, newColor, scale);
        index = dustSpawned.dustIndex;
        if (index < Main.maxDust)
        {
            dust = dustSpawned;
            action?.Invoke(dust);
            return true;
        }
        else
        {
            dust = null;
            return false;
        }
    }
}