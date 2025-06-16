using Ionic.Zlib;
using Terraria;

namespace Transoceanic;

public static class TOExtensions
{
    extension(ArgumentException)
    {
        public static void ThrowIfNullOrEmpty<T>(IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            if (argument.Count == 0)
                throw new ArgumentException($"Argument {paramName} cannot be empty.", paramName);
        }

        public static void ThrowIfNullOrEmptyOrAnyNull<T>(IList<T> argument, [CallerArgumentExpression(nameof(argument))] string paramName = null!) where T : class
        {
            ArgumentException.ThrowIfNullOrEmpty(argument, paramName);
            for (int i = 0; i < argument.Count; i++)
                _ = argument[i] ?? throw new ArgumentException($"Argument {paramName} has a null element at [{i}].", paramName);
        }
    }

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

        public void ReplyLocalizedTextWith(string key, Color? textColor = null, params object[] args) => caller.Reply(TOLocalizationUtils.GetTextFormat(key, args), textColor ?? Color.White);

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
        public static void NewDustAction(Vector2 position, int width, int height, int type, Action<Dust> action = null)
        {
            int index = Dust.NewDust(position - new Vector2(width / 2f, height / 2f), width, height, type);
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
        public static void NewDustAction<T>(Vector2 position, int width, int height, Action<Dust> action = null) where T : ModDust =>
            NewDustAction(position, width, height, ModContent.DustType<T>(), action);

        /// <summary>
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="offsetX">X偏移最大值。</param>
        /// <param name="offsetY">Y偏移最大值。</param>
        /// <param name="type">类型。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustActionCheck(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int offsetX, int offsetY, int type, Action<Dust> action = null)
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
        /// 生成一个新的Dust，并在生成后执行一个Action。
        /// </summary>
        /// <typeparam name="T">ModDust所属类型。</typeparam>
        /// <param name="index">输出的Dust索引。</param>
        /// <param name="dust">输出的Dust实例。</param>
        /// <param name="position">生成位置。<br>注意：该参数表示生成中心，而不是左上角。</br></param>
        /// <param name="offsetX">X偏移最大值。</param>
        /// <param name="offsetY">Y偏移最大值。</param>
        /// <param name="action">执行的行为。仅当成功生成Dust时生效。</param>
        /// <returns>生成Dust是否成功。</returns>
        public static bool NewDustActionCheck<T>(out int index, [NotNullWhen(true)] out Dust dust, Vector2 position, int offsetX, int offsetY, Action<Dust> action = null) where T : ModDust =>
            NewDustActionCheck(out index, out dust, position, offsetX, offsetY, ModContent.DustType<T>(), action);

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
        /// <returns>获取的 <c>type</c> 值。对于 <see cref="NPC"/>，如果其 <see cref="NPC.netID"/> 小于0，则返回 <see cref="NPC.netID"/>，否则返回 <see cref="NPC.type"/>。</returns>
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

    extension(IList<Color> colors)
    {
        public Color LerpMany(float ratio)
        {
            ArgumentNullException.ThrowIfNull(colors, nameof(colors));

            ratio = Math.Clamp(ratio, 0f, colors.Count - 1);

            switch (colors.Count)
            {
                case 0:
                    return Color.White;
                case 1:
                    return colors[0];
                case 2:
                    return Color.Lerp(colors[0], colors[1], ratio);
                default:
                    if (ratio <= 0f)
                        return colors[0];
                    if (ratio >= 1)
                        return colors[^1];
                    (int index, float localRatio) = TOMathHelper.SplitFloat(Math.Clamp(ratio * (colors.Count - 1), 0f, colors.Count - 1));
                    return Color.Lerp(colors[index], colors[index + 1], localRatio);
            }
        }
    }

    extension(Item item)
    {
        public TOGlobalItem Ocean() => item.GetGlobalItem<TOGlobalItem>();

        public T GetModItem<T>() where T : ModItem => item.ModItem as T;

        public bool TryGetModItem<T>([NotNullWhen(true)] out T result) where T : ModItem => (result = item.GetModItem<T>()) is not null;
    }

    extension(MethodBase method)
    {
        public bool HasAttribute<T>() where T : Attribute => method.GetAttribute<T>() is not null;
    }

    extension(MethodInfo method)
    {
        public string FullName => method.IsGenericMethod
            ? method.Name + "<" + string.Join(",", method.GetGenericArguments().Select(k => k.Name)) + ">"
            : method.Name;

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public bool IsOverrideOf(Type baseType)
        {
            MethodInfo baseDefinition = method.GetBaseDefinition();
            return baseDefinition.DeclaringType == baseType &&
                   !baseDefinition.DeclaringType.IsInterface;
        }

        /// <summary>
        /// 判定指定方法是否为指定基类型的重写方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsOverrideOf<T>() => method.IsOverrideOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否为某个基类型的重写方法。
        /// </summary>
        /// <returns></returns>
        public bool IsOverride
        {
            get
            {
                MethodInfo baseDefinition = method.GetBaseDefinition();
                return baseDefinition.DeclaringType != method.DeclaringType &&
                       !baseDefinition.DeclaringType.IsInterface;
            }
        }

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <returns></returns>
        public bool IsInterfaceImplementationOf(Type interfaceType)
        {
            InterfaceMapping map;
            try
            {
                map = method.DeclaringType.GetInterfaceMap(interfaceType);
            }
            catch (NotSupportedException)
            {
                return false;
            }

            for (int i = 0; i < map.InterfaceMethods.Length; i++)
            {
                if (map.TargetMethods[i] == method)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 判定指定方法是否实现了指定接口类型。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsInterfaceImplementationOf<T>() => method.IsInterfaceImplementationOf(typeof(T));

        /// <summary>
        /// 判定指定方法是否实现了某个接口类型。
        /// </summary>
        /// <returns></returns>
        public bool IsInterfaceImplementation => method.DeclaringType.GetInterfaces().Any(method.IsInterfaceImplementationOf);
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
    }

    extension(NPC)
    {
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

        public static TOIterator<NPC> Enemies => TOIteratorFactory.NewActiveNPCIterator(k => k.Enemy);

        public static TOIterator<NPC> Bosses => TOIteratorFactory.NewActiveNPCIterator(k => k.TOBoss);

        public void SpawnOnPlayer<T>(int plr) where T : ModNPC => NPC.SpawnOnPlayer(plr, ModContent.NPCType<T>());

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

        public TOExclusiveIterator<Player> Teammates => TOIteratorFactory.NewActivePlayerIterator(k => k.IsTeammateOf(player), Main.player);

        /// <summary>
        /// 获取玩家的手持物品。
        /// </summary>
        /// <returns>若玩家光标持有物品，返回该物品；否则返回玩家物品栏中选中的物品。</returns>
        public Item ActiveItem => Main.mouseItem.IsAir ? player.HeldItem : Main.mouseItem;
    }

    extension(Player)
    {
        public static TOIterator<Player> ActivePlayers => TOIteratorFactory.NewActivePlayerIterator();

        public static TOIterator<Player> PVPPlayers => TOIteratorFactory.NewPlayerIterator(k => k.PvP);

        public static int ActivePlayerCount => Main.netMode == NetmodeID.SinglePlayer ? 1 : Main.CurrentFrameFlags.ActivePlayersCount;
    }

    extension(Projectile projectile)
    {
        public TOGlobalProjectile Ocean() => projectile.GetGlobalProjectile<TOGlobalProjectile>();

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

    extension(StringBuilder builder)
    {
        public void AppendLocalizedLine(string key) => builder.AppendLine(Language.GetTextValue(key));

        public void AppendLocalizedLineWith(string key, params object[] args) => builder.AppendLine(TOLocalizationUtils.GetTextFormat(key, args));

        public void AppendTODebugErrorMessage()
        {
            builder.Append(Environment.NewLine);
            builder.AppendLocalizedLine(TOMain.DebugErrorMessageKey);
        }
    }

    extension(Tile tile)
    {
        public void SetTileType(int type) => tile.TileType = (ushort)type;

        public void SetTileType<T>() where T : ModTile => tile.SetTileType(ModContent.TileType<T>());
    }

    extension(Type type)
    {
        /// <summary>
        /// 获取指定类型中所有由该类型声明的方法。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetRealMethods(BindingFlags flags) => type.GetMethods(flags).Where(k => k.DeclaringType == type);

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags, out MethodInfo methodInfo) =>
            (methodInfo = type.GetRealMethods(flags).FirstOrDefault(k => k.Name == methodName)) is not null;

        /// <summary>
        /// 检查指定类型是否声明了对应方法。
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public bool HasRealMethod(string methodName, BindingFlags flags) =>
            type.HasRealMethod(methodName, flags, out _);

        /// <summary>
        /// 适用于 <paramref name="mainMethod"/> 依赖于 <paramref name="requiredMethod"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethod"/> 存在而 <paramref name="requiredMethod"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags flags, out MethodInfo mainMethod, out MethodInfo requiredMethod) =>
            (type.HasRealMethod(mainMethodName, flags, out mainMethod), type.HasRealMethod(requiredMethodName, flags, out requiredMethod)) is not (true, false);

        /// <summary>
        /// 适用于 <paramref name="mainMethodName"/> 依赖于 <paramref name="requiredMethodName"/> 的情况，判定是否有正确的方法关系。
        /// </summary>
        /// <returns>仅在 <paramref name="mainMethodName"/> 存在而 <paramref name="requiredMethodName"/> 不存在时返回 <see langword="false"/>。</returns>
        public bool MustHaveRealMethodWith(string mainMethodName, string requiredMethodName, BindingFlags flags) =>
            type.MustHaveRealMethodWith(mainMethodName, requiredMethodName, flags, out _, out _);

        /// <summary>
        /// 获取指定类型中所有重写方法。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<MethodInfo> GetOverrideMethods(BindingFlags flags) => type.GetRealMethods(flags).Where(k => k.IsOverride);

        /// <summary>
        /// 获取指定类型中所有重写方法的名称。
        /// </summary>
        /// <param name="flags"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOverrideMethodNames(BindingFlags flags) =>
            from MethodInfo method in type.GetOverrideMethods(flags)
            select method.FullName;
    }

    extension(Vector2 vector)
    {
        public void Deconstruct(out float x, out float y)
        {
            x = vector.X;
            y = vector.Y;
        }

        /// <summary>
        /// 获取向量的顺时针旋转角。
        /// </summary>
        /// <returns>零向量返回0，否则返回 [0, 2π) 范围内的浮点值。</returns>
        public float Angle => vector.Y switch
        {
            > 0f => MathF.Atan2(vector.Y, vector.X),
            0f => vector.X switch
            {
                >= 0f => 0f, //零向量返回0，方向为x轴正方向返回0
                _ => MathHelper.Pi //方向为x轴负方向返回Pi
            },
            _ => MathHelper.TwoPi + MathF.Atan2(vector.Y, vector.X), //将Atan2方法返回的负值转换为正值
        };

        /// <summary>
        /// 安全地将向量化为单位向量。
        /// </summary>
        /// <returns>零向量返回零向量，否则返回单位向量。</returns>
        public Vector2 SafelyNormalized => vector == Vector2.Zero ? Vector2.Zero : Vector2.Normalize(vector);

        /// <summary>
        /// 获取模为特定值的原向量同向向量。不改变原向量值。
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public Vector2 ToCustomLength(float length) => vector.SafelyNormalized * length;
    }

    extension(ref Vector2 vector)
    {
        public void CopyFrom(Vector2 other)
        {
            vector.X = other.X;
            vector.Y = other.Y;
        }

        public float Modulus
        {
            get => vector.Length();
            set => vector.CopyFrom(vector.ToCustomLength(value));
        }
    }

    extension(Vector2)
    {
        /// <summary>
        /// 计算两个向量的夹角。
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static float IncludedAngle(Vector2 value1, Vector2 value2) => (float)Math.Acos(Vector2.Dot(value1, value2) / (value1.Modulus * value2.Modulus));

        /// <summary>
        /// 获取两个向量角平分线的单位方向向量。
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <returns></returns>
        public static Vector2 UnitAngleBisector(Vector2 value1, Vector2 value2) => new PolarVector2((value1.Angle + value2.Angle) / 2);
    }
}
