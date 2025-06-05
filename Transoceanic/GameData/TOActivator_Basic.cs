namespace Transoceanic.GameData;

public static partial class TOActivator
{
    /// <summary>
    /// 生成一个新的NPC，并在生成后执行一个Action。
    /// </summary>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="type">类型。</param>
    /// <param name="start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    public static void NewNPCAction(IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
    {
        int index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
        if (index < Main.maxNPCs)
        {
            action?.Invoke(Main.npc[index]);
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
        }
    }

    /// <summary>
    /// 生成一个新的NPC，并在生成后执行一个Action。
    /// </summary>
    /// <param name="index">输出的NPC索引。</param>
    /// <param name="npc">输出的NPC实例。</param>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="type">类型。</param>
    /// <param name="start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    /// <returns>生成NPC是否成功。</returns>
    public static bool NewNPCActionCheck(out int index, out NPC npc, IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
    {
        index = NPC.NewNPC(source, (int)position.X, (int)position.Y, type, start);
        if (index < Main.maxNPCs)
        {
            npc = Main.npc[index];
            action?.Invoke(npc);
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, index);
            return true;
        }
        else
        {
            npc = null;
            return false;
        }
    }

    /// <summary>
    /// 生成一个新的ModNPC，并在生成后执行一个Action。
    /// </summary>
    /// <typeparam name="T">ModNPC所属类型。</typeparam>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    public static void NewNPCAction<T>(IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
        NewNPCAction(source, position, ModContent.NPCType<T>(), start, action);

    /// <summary>
    /// 生成一个新的ModNPC，并在生成后执行一个Action。
    /// </summary>
    /// <typeparam name="T">ModNPC所属类型。</typeparam>
    /// <param name="index">输出的NPC索引。</param>
    /// <param name="npc">输出的NPC实例。</param>
    /// <param name="source">生成源。</param>
    /// <param name="position">生成位置。</param>
    /// <param name="start">生成NPC索引的最小值。</param>
    /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
    /// <returns>生成NPC是否成功。</returns>
    public static void NewNPCActionCheck<T>(out int index, out NPC npc, IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
        NewNPCActionCheck(out index, out npc, source, position, ModContent.NPCType<T>(), start, action);

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
    /// <param name="width">X偏移最大值。</param>
    /// <param name="height">Y偏移最大值。</param>
    /// <param name="type">类型。</param>
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    public static void NewDustAction(Vector2 position, int width, int height, int type, Action<Dust> action = null)
    {
        int index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type);
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
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    /// <returns></returns>
    public static bool NewDustActionCheck(out int index, out Dust dust, Vector2 position, int offsetX, int offsetY, int type, Action<Dust> action = null)
    {
        index = Dust.NewDust(position - new Vector2(offsetX, offsetY), offsetX * 2, offsetY * 2, type);
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
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    public static void NewDustPerfectAction(Vector2 position, int type, Action<Dust> action = null)
    {
        Dust dustSpawned = Dust.NewDustPerfect(position, type);
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
    /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
    /// <returns></returns>
    public static bool NewDustPerfectActionCheck(out int index, out Dust dust, Vector2 position, int type, Action<Dust> action = null)
    {
        Dust dustSpawned = Dust.NewDustPerfect(position, type);
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