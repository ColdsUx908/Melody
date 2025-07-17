namespace Transoceanic.Core.Extensions;

public static partial class TOExtensions
{
    extension(Chest chest)
    {
        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置。
        /// </summary>
        public Point Position => new(chest.x, chest.y);

        /// <summary>
        /// 获取箱子（左下角格子）在世界中的位置对应的向量。
        /// </summary>
        public Vector2 Coordinate => chest.Position.ToWorldCoordinates();

        public Vector2 Center => chest.Position.ToWorldCoordinates(0f, 16f);

        public bool HasItem(int itemType, out int index, [NotNullWhen(true)] out Item item)
        {
            for (int i = 0; i < chest.item.Length; i++)
            {
                Item current = chest.item[i];
                if (current.type == itemType)
                {
                    index = i;
                    item = current;
                    return true;
                }
            }
            index = -1;
            item = null;
            return false;
        }
    }

    extension(CommandCaller caller)
    {
        public void ReplyLocalizedText(string key, Color? textColor = null) => caller.Reply(Language.GetTextValue(key), textColor ?? Color.White);

        public void ReplyLocalizedTextWith(string key, Color? textColor = null, params object[] args) => caller.Reply(Language.GetTextFormat(key, args), textColor ?? Color.White);

        public void ReplyStringBuilder(StringBuilder builder, Color? textColor = null) => caller.Reply(builder.ToString(), textColor ?? Color.White);

        public void ReplyDebugErrorMessage(string key, params object[] args)
        {
            caller.Reply("[Transoceanic ERROR", TOMain.TODebugErrorColor);
            caller.ReplyLocalizedTextWith(key, TOMain.TODebugErrorColor, args);
            caller.ReplyLocalizedText(TOMain.DebugErrorMessageKey, TOMain.TODebugErrorColor);
        }
    }

    extension(Dust)
    {
        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustAction(Vector2 position, int width, int height, int type, Vector2 velocity = default, Action<Dust> action = null)
        {
            int index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type, velocity.X, velocity.Y);
            if (index < Main.maxDust)
                action?.Invoke(Main.dust[index]);
        }

        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustAction<T>(Vector2 position, int width, int height, Vector2 velocity = default, Action<Dust> action = null) where T : ModDust =>
            NewDustAction(position, width, height, ModContent.DustType<T>(), velocity, action);

        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustActionCheck(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int width, int height, int type, Vector2 velocity = default, Action<Dust> action = null)
        {
            index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type, velocity.X, velocity.Y);
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
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="width">X偏移最大值。</param>
        /// <param name="height">Y偏移最大值。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int width, int height, Vector2 velocity, Action<Dust> action = null) where T : ModDust =>
            NewDustActionCheck(out index, out dust, position, width, height, ModContent.DustType<T>(), velocity, action);

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
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="position">生成位置。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        public static void NewDustPerfectAction<T>(Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectAction(position, ModContent.DustType<T>(), action);

        /// <summary>
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustPerfectActionCheck(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int type, Action<Dust> action = null)
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

        /// <summary>
        /// 生成一个新的Dust（无随机偏移），并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustPerfectActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, Action<Dust> action = null) where T : ModDust =>
            NewDustPerfectActionCheck(out index, out dust, position, ModContent.DustType<T>(), action);
    }

    extension(Entity entity)
    {
        /// <summary>
        /// 尝试获取实体的 <c>type</c>。
        /// </summary>
        /// <returns>
        /// 获取的 <c>type</c> 值。
        /// <br/>对于 <see cref="NPC"/>，如果其 <see cref="NPC.netID"/> 小于0，则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        public int EntityType => entity switch
        {
            NPC npc => npc.netID < 0 ? npc.netID : npc.type,
            Projectile projectile => projectile.type,
            Item item => item.type,
            Player => throw new ArgumentException("Players do not have a type.", nameof(entity)),
            _ => throw new ArgumentException("Unknown Entity", nameof(entity)),
        };

        /// <summary>
        /// 使实体追踪指定目标（反物理规则）。
        /// </summary>
        /// <param name="target">追踪目标。</param>
        /// <param name="homingRatio">追踪强度。为1时强制追踪。</param>
        /// <param name="sightAngle">视野范围。</param>
        /// <param name="keepVelocity">是否在调整角度时保持速度大小不变。仅在追踪强度不为1时有效。</param>
        /// <remarks>须由具体实现决定目标锁定机制。</remarks>
        /// <returns>若追踪成功，true，否则，false。</returns>
        public bool Homing<T>(T target, float homingRatio = 1f, float sightAngle = MathHelper.TwoPi, bool keepVelocity = true)
            where T : Entity
        {
            if (target is not null && target.active)
            {
                Vector2 distanceVector = target.Center - entity.Center;
                float distance = distanceVector.Length();

                if (sightAngle != MathHelper.TwoPi && Vector2.IncludedAngle(entity.velocity, distanceVector) > sightAngle / 2f)
                    return false;

                float velocityLength = entity.velocity.Length();
                Vector2 distanceVector2 = distanceVector.ToCustomLength(velocityLength);
                if (homingRatio == 1f)
                    entity.velocity = distance < velocityLength ? distanceVector : distanceVector2;
                else
                {
                    entity.velocity = Vector2.SmoothStep(entity.velocity, distanceVector2, homingRatio);
                    if (keepVelocity)
                        entity.velocity.Modulus = velocityLength;
                }

                return true;
            }
            else
                return false;
        }
    }

    extension(Gore)
    {
        public static void NewGoreAction(IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            int index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
                action?.Invoke(Main.gore[index]);
        }

        public static void NewGoreAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreAction(source, position, velocity, ModContent.GoreType<T>(), action);

        public static bool NewGoreActionCheck(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Vector2 velocity, int type, Action<Gore> action = null)
        {
            index = Gore.NewGore(source, position, velocity, type);
            if (index < Main.maxGore)
            {
                gore = Main.gore[index];
                action?.Invoke(gore);
                return true;
            }
            else
            {
                gore = null;
                return false;
            }
        }

        public static bool NewGoreActionCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Vector2 velocity, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionCheck(out index, out gore, source, position, velocity, ModContent.GoreType<T>(), action);

        public static void NewGoreActionPerfect(IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreAction(source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        public static void NewGoreActionPerfect<T>(IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfect(source, position, ModContent.GoreType<T>(), action);

        public static bool NewGoreActionPerfectCheck(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, int type, Action<Gore> action = null) =>
            NewGoreActionCheck(out index, out gore, source, position, Vector2.Zero, type, g =>
            {
                g.position = position;
                g.velocity = Vector2.Zero;
                action?.Invoke(g);
            });

        public static bool NewGoreActionPerfectCheck<T>(out int index, [NotNullWhen(true)] out Gore gore, IEntitySource source, Vector2 position, Action<Gore> action = null) where T : ModGore =>
            NewGoreActionPerfectCheck(out index, out gore, source, position, ModContent.GoreType<T>(), action);
    }

    extension(Item item)
    {
        public TOGlobalItem Ocean() => item.GetGlobalItem<TOGlobalItem>();

        public T GetModItem<T>() where T : ModItem => item.ModItem as T;

        public bool TryGetModItem<T>([NotNullWhen(true)] out T result) where T : ModItem => (result = item.GetModItem<T>()) is not null;

        public void DrawInventoryWithBorder(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Vector2 origin, float scale,
            int way, float borderWidth, Color borderColor) =>
            TODrawUtils.DrawBorderTexture(spriteBatch, TextureAssets.Item[item.type].Value, position, frame, borderColor, 0f, origin, scale, way: way, borderWidth: borderWidth);
    }

    extension(Language)
    {
        public static string GetTextFormat(string key, params object[] args) => Language.GetText(key).Format(args);
    }

    extension(List<TooltipLine> tooltips)
    {
        public int FindFirstTerrariaTooltipIndex()
        {
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (line.Mod == "Terraria" && line.Name.StartsWith("Tooltip"))
                    return i;
            }
            return -1;
        }

        public int FindLastTerrariaTooltipIndex(out int num)
        {
            for (int i = tooltips.Count - 1; i >= 0; i--)
            {
                TooltipLine line = tooltips[i];
                if (line.Mod == "Terraria" && ItemTooltipModifier._tooltipRegex.TryMatch(line.Name, out Match match))
                {
                    num = int.Parse(match.Groups[1].Value);
                    return i;
                }
            }
            num = -1;
            return -1;
        }

        public void ModifyTooltip(Predicate<TooltipLine> predicate, Action<TooltipLine> action)
        {
            ArgumentNullException.ThrowIfNull(predicate);
            ArgumentNullException.ThrowIfNull(action);
            for (int i = 0; i < tooltips.Count; i++)
            {
                TooltipLine line = tooltips[i];
                if (predicate(line))
                {
                    action(line);
                    return;
                }
            }
        }

        public void ModifyVanillaTooltipByName(string name, Action<TooltipLine> action) =>
            tooltips.ModifyTooltip(l => l.Mod == "Terraria" && l.Name == name, action);

        public void ModifyTooltipByNum(int num, Action<TooltipLine> action) =>
            tooltips.ModifyVanillaTooltipByName($"Tooltip{num}", action);
    }

    extension(ModContent)
    {
        public static T GetModNPC<T>() where T : ModNPC => (T)ModContent.GetModNPC(ModContent.NPCType<T>());

        public static T GetModItem<T>() where T : ModItem => (T)ModContent.GetModItem(ModContent.ItemType<T>());

        public static T GetModDust<T>() where T : ModDust => (T)ModContent.GetModDust(ModContent.DustType<T>());

        public static T GetModProjectile<T>() where T : ModProjectile => (T)ModContent.GetModProjectile(ModContent.ProjectileType<T>());

        public static T GetModBuff<T>() where T : ModBuff => (T)ModContent.GetModBuff(ModContent.BuffType<T>());

        public static T GetModMount<T>() where T : ModMount => (T)ModContent.GetModMount(ModContent.MountType<T>());

        public static T GetModTile<T>() where T : ModTile => (T)ModContent.GetModTile(ModContent.TileType<T>());

        public static T GetModWall<T>() where T : ModWall => (T)ModContent.GetModWall(ModContent.WallType<T>());
    }

    extension(NPC npc)
    {
        public TOGlobalNPC Ocean() => npc.GetGlobalNPC<TOGlobalNPC>();

        public T GetModNPC<T>() where T : ModNPC => npc.ModNPC as T;

        public bool TryGetModNPC<T>([NotNullWhen(true)] out T result) where T : ModNPC => (result = npc.GetModNPC<T>()) is not null;

        public bool TOFriendly => npc.active && (npc.friendly || npc.townNPC || npc.lifeMax <= 5);

        public bool Enemy => !npc.friendly && npc.lifeMax >= 5;

        /// <summary>
        /// 检查NPC是否为Boss。
        /// </summary>
        /// <returns></returns>
        public bool TOBoss => npc.boss || npc.EoW || npc.type == NPCID.WallofFleshEye;

        public bool EoW => npc.type is NPCID.EaterofWorldsHead or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsTail;

        public bool Destroyer => npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail;

        public bool Twins => npc.type is NPCID.Retinazer or NPCID.Spazmatism;

        public bool SkeletronPrimeHand => npc.type is >= 128 and <= 131; //机械炮：128，机械锯：129，机械钳：130，机械：131

        public bool GolemFist => npc.type is NPCID.GolemFistLeft or NPCID.GolemFistRight;

        public bool CultistDragon => npc.type is >= 454 and <= 459; //幻影龙头部：454，幻影龙身体1：455，幻影龙身体2：456，幻影龙身体3：457，幻影龙身体4：458，幻影龙尾部：459

        public int TargetDirection => Math.Sign((npc.HasNPCTarget ? Main.projectile[npc.target - 300] : (Entity)Main.player[npc.target]).Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        public bool IsFacingTarget => npc.direction == npc.TargetDirection;

        public void FaceNPCTarget(Entity target) => npc.direction = Math.Sign(target.Center.X - npc.Center.X) switch
        {
            -1 => -1,
            _ => 1
        };

        /// <summary>
        /// 将NPC速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity">速度</param>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使NPC额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            npc.velocity = velocity;
            npc.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的NPC，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="NPC.rotation"/>，并应用于NPC。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使NPC额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        public void VelocityToRotation(float rotationOffset = 0f) => npc.rotation = npc.velocity.ToRotation() + rotationOffset;

        /// <summary>
        /// 如果现有目标无效，获取新的目标。
        /// </summary>
        /// <param name="faceTarget"></param>
        /// <param name="distanceThreshold"></param>
        /// <returns>获取目标后的目标是否有效。</returns>
        public bool TargetClosestIfInvalid(bool faceTarget = true, float distanceThreshold = 4000f)
        {
            if (!npc.HasValidTarget)
                npc.TargetClosest(faceTarget);

            Player target = Main.player[npc.target];

            if (distanceThreshold >= 0f && !npc.WithinRange(target.Center, distanceThreshold - target.aggro))
                npc.TargetClosest(faceTarget);

            return npc.HasValidTarget && npc.WithinRange(target.Center, distanceThreshold - target.aggro);
        }

        public void BetterChangeScale(int width, int height, float scale)
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

        public void ApplyDOT(int dot, int damageValue, ref int damage)
        {
            npc.lifeRegen = Math.Min(npc.lifeRegen, 0) - dot;
            damage = Math.Max(damage, damageValue);
        }
    }

    extension(NPC)
    {
        public static NPC DummyNPC => Main.npc[Main.maxNPCs];

        public static bool AnyNPCs<T>() where T : ModNPC => NPC.AnyNPCs(ModContent.NPCType<T>());

        public static bool AnyNPCs(int type, [NotNullWhen(true)] out NPC npc)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.type == type)
                    return true;
            }
            npc = null;
            return false;
        }

        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T)
                    return true;
            }
            npc = null;
            return false;
        }

        public static bool AnyNPCs<T>([NotNullWhen(true)] out NPC npc, [NotNullWhen(true)] out T modNPC) where T : ModNPC
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                npc = Main.npc[i];
                if (npc.active && npc.ModNPC is T t)
                {
                    modNPC = t;
                    return true;
                }
            }
            npc = null;
            modNPC = null;
            return false;
        }

        public static TOIterator<NPC> ActiveNPCs => TOIteratorFactory.NewActiveNPCIterator();

        public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(n => n.Enemy);

        public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(n => n.TOBoss);

        public static void SpawnOnPlayer<T>(int plr) where T : ModNPC => NPC.SpawnOnPlayer(plr, ModContent.NPCType<T>());

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
        public static bool NewNPCActionCheck(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int type, int start = 0, Action<NPC> action = null)
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
        /// <param name="index">输出的NPC索引。</param>
        /// <param name="npc">输出的NPC实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="start">生成NPC索引的最小值。</param>
        /// <param name="action">执行的行为。仅当成功生成NPC时生效。</param>
        /// <returns>生成NPC是否成功。</returns>
        public static void NewNPCActionCheck<T>(out int index, [NotNullWhen(true)] out NPC npc, IEntitySource source, Vector2 position, int start = 0, Action<NPC> action = null) where T : ModNPC =>
            NewNPCActionCheck(out index, out npc, source, position, ModContent.NPCType<T>(), start, action);
    }

    extension(ref NPC.HitModifiers modifiers)
    {
        public void SetInstantKillBetter(NPC target) => modifiers.FinalDamage += target.lifeMax;
    }

    extension(Player player)
    {
        public TOPlayer Ocean() => player.GetModPlayer<TOPlayer>();

        public bool Alive => player.active && !player.dead && !player.ghost;

        public bool PvP => player.Alive && player.hostile;

        public bool IsTeammateOf(Player other) => player.Alive && player.team != 0 && player.team == other.team;

        public TOExclusiveIterator<Player> OtherAlivePlayers => TOIteratorFactory.NewPlayerIterator(p => p.Alive, player);

        public TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewPlayerIterator(p => p.IsTeammateOf(player), player);

        /// <summary>
        /// 获取玩家的手持物品。
        /// </summary>
        /// <returns>若玩家光标持有物品，返回该物品；否则返回玩家物品栏中选中的物品。</returns>
        public Item CurrentItem => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
    }

    extension(Player)
    {
        public static Player Server => Main.player[Main.maxPlayers];

        public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

        public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewPlayerIterator(p => p.PvP);

        public static int ActivePlayerCount => Main.netMode == NetmodeID.SinglePlayer ? 1 : Main.CurrentFrameFlags.ActivePlayersCount;
    }

    extension(Projectile projectile)
    {
        public TOGlobalProjectile Ocean() => projectile.GetGlobalProjectile<TOGlobalProjectile>();

        public Player Owner => Main.player[projectile.owner];

        public T GetModProjectile<T>() where T : ModProjectile => projectile.ModProjectile as T;

        public bool TryGetModProjectile<T>([NotNullWhen(true)] out T result) where T : ModProjectile => (result = projectile.GetModProjectile<T>()) is not null;

        public bool OnOwnerClient => projectile.owner == Main.myPlayer;

        /// <summary>
        /// 将弹幕速度设置为指定值，同时更新旋转。
        /// <br>为性能考虑，不要在不改变方向的情况中重复调用该方法。</br>
        /// </summary>
        /// <param name="velocity">速度。</param>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        /// </param>
        public void SetVelocityandRotation(Vector2 velocity, float rotationOffset = 0f)
        {
            projectile.velocity = velocity;
            projectile.VelocityToRotation(rotationOffset);
        }

        /// <summary>
        /// 适用于贴图方向向上的弹幕，用于将 <see cref="Entity.velocity"/> 转换为 <see cref="Projectile.rotation"/>，并应用于弹幕。
        /// </summary>
        /// <param name="rotationOffset">旋转偏移值。
        /// <br/>在设置旋转时会加上该值，使弹幕额外顺时针旋转。
        /// <br/>例如，对于贴图方向向上的弹幕，应设置该值为 <see cref="MathHelper.PiOver2"/>。
        public void VelocityToRotation(float rotationOffset = 0f) => projectile.rotation = projectile.velocity.ToRotation() + rotationOffset;
    }

    extension(Projectile)
    {
        public static Projectile DummyProjectile => Main.projectile[Main.maxProjectiles];

        public static TOIterator<Projectile> ActiveProjectiles => TOIteratorFactory.NewActiveProjectileIterator();

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
            {
                action?.Invoke(Main.projectile[index]);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
            }
        }

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void NewProjectileAction<T>(IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileAction(source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

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
        /// <returns>生成Projectile是否成功。</returns>
        public static bool NewProjectileActionCheck(out int index, [NotNullWhen(true)] out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            index = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, owner);
            if (index < Main.maxProjectiles)
            {
                projectile = Main.projectile[index];
                action?.Invoke(projectile);
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, index);
                return true;
            }
            else
            {
                projectile = null;
                return false;
            }
        }

        /// <summary>
        /// 生成一个新的Projectile，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="index">输出的Projectile索引。</param>
        /// <param name="projectile">输出的Projectile实例。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>生成Projectile是否成功。</returns>
        public static bool NewProjectileActionCheck<T>(out int index, [NotNullWhen(true)] out Projectile projectile, IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null) where T : ModProjectile =>
            NewProjectileActionCheck(out index, out projectile, source, position, velocity, ModContent.ProjectileType<T>(), damage, knockback, owner, action);

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void RotatedProj(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            for (int i = 0; i < number; i++)
                NewProjectileAction(source, position, velocity.RotatedBy(radian * i), type, damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <typeparam name="T">ModProjectile所属类型。</typeparam>
        /// <param name="number">弹幕总数。</param>
        /// <param name="radian">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        public static void RotatedProj<T>(int number, float radian,
            IEntitySource source, Vector2 position, Vector2 velocity, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
            where T : ModProjectile
        {
            for (int i = 0; i < number; i++)
                NewProjectileAction(source, position, velocity.RotatedBy(radian * i), ModContent.ProjectileType<T>(), damage, knockback, owner, action);
        }

        /// <summary>
        /// 生成指定数量的Projectile，使用指定的旋转角度。
        /// </summary>
        /// <param name="indexes">输出的Projectile索引数组。</param>
        /// <param name="projectiles">输出的Projectile实例数组。</param>
        /// <param name="spawnedNumber">输出的实际生成的Projectile数量。</param>
        /// <param name="number">弹幕总数。</param>
        /// <param name="offset">单次旋转角度（顺时针）。</param>
        /// <param name="source">生成源。</param>
        /// <param name="position">生成位置。</param>
        /// <param name="velocity">速度。</param>
        /// <param name="type">类型。</param>
        /// <param name="damage">伤害。</param>
        /// <param name="knockback">击退。</param>
        /// <param name="owner">弹幕主人。</param>
        /// <param name="action">执行的行为。仅当成功生成Projectile时生效。</param>
        /// <returns>Projectile是否全部生成。</returns>
        public static bool RotatedProjCheck(out List<int> indexes, out List<Projectile> projectiles, out int spawnedNumber, int number, float offset,
            IEntitySource source, Vector2 position, Vector2 velocity, int type, int damage, float knockback, int owner = -1, Action<Projectile> action = null)
        {
            indexes = [];
            projectiles = [];
            spawnedNumber = 0;
            bool allSuccess = true;
            PolarVector2 temp = (PolarVector2)velocity;
            for (int i = 0; i < number; i++)
            {
                if (NewProjectileActionCheck(out int index, out Projectile projectile, source, position, temp.RotatedBy(offset * i), type, damage, knockback, owner, action))
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

    extension(Tile tile)
    {
        public void SetTileType(int type) => tile.TileType = (ushort)type;

        public void SetTileType<T>() where T : ModTile => tile.SetTileType(ModContent.TileType<T>());
    }
}
