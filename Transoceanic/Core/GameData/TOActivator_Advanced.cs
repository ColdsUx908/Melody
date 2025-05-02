using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Transoceanic.Core.ExtraData.Maths;

namespace Transoceanic.Core.GameData;

public static partial class TOActivator
{
    /// <summary>
    /// 生成指定数量的Projectile，使用指定的旋转角度。
    /// </summary>
    /// <param name="number">弹幕总数。</param>
    /// <param name="radian">单次旋转角度（顺时针）。</param>
    public static void RotatedProj(int number, float radian,
        IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
    {
        for (int i = 0; i < number; i++)
            NewProjectileAction(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
    }

    /// <summary>
    /// 生成指定数量的Projectile，使用指定的旋转角度。
    /// </summary>
    /// <param name="indexes">输出的Projectile索引数组。</param>
    /// <param name="projectiles">输出的Projectile实例数组。</param>
    /// <param name="number">弹幕总数。</param>
    /// <param name="radian">单次旋转角度（顺时针）。</param>
    /// <returns>Projectile是否全部生成。</returns>
    public static bool RotatedProjCheck(out int[] indexes, out Projectile[] projectiles, int number, float radian,
        IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1)
    {
        indexes = new int[number];
        projectiles = new Projectile[number];
        PolarVector2 temp = (PolarVector2)velocity;
        for (int i = 0; i < number; i++)
        {
            if (NewProjectileActionCheck(out int index, out Projectile projectile, source, position, temp.RotatedBy(radian * i), type, damage, knockback, owner))
            {
                indexes[i] = index;
                projectiles[i] = projectile;
            }
            else
            {
                indexes = null;
                projectiles = null;
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// 生成指定数量的Projectile，使用指定的旋转角度。
    /// </summary>
    /// <param name="indexes">输出的Projectile索引数组。</param>
    /// <param name="projectiles">输出的Projectile实例数组。</param>
    /// <param name="spawnedNumber">输出的实际生成的Projectile数量。</param>
    /// <param name="number">弹幕总数。</param>
    /// <param name="offset">单次旋转角度（顺时针）。</param>
    /// <returns>Projectile是否全部生成。</returns>
    public static bool RotatedProjCheck(out List<int> indexes, out List<Projectile> projectiles, out int spawnedNumber, int number, float offset,
        IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1)
    {
        indexes = [];
        projectiles = [];
        spawnedNumber = 0;
        bool allSuccess = true;
        PolarVector2 temp = (PolarVector2)velocity;
        for (int i = 0; i < number; i++)
        {
            if (NewProjectileActionCheck(out int index, out Projectile projectile, source, position, temp.RotatedBy(offset * i), type, damage, knockback, owner))
            {
                indexes.Add(index);
                projectiles.Add(projectile);
                spawnedNumber++;
            }
            else
                allSuccess = false;
        }
        return allSuccess;
    }
}