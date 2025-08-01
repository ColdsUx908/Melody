﻿namespace Transoceanic.Core;

/// <summary>
/// 对象迭代器。
/// <para/>示例：
/// <code>
/// foreach (NPC kingSlime in TOIteratorFactory.NewActiveIterator(n => n.type = NPCID.KingSlime)) //也可以使用预定义的NPC.ActiveNPCs_TO
/// {
///     //代码
/// }
/// </code>
/// </summary>
public readonly ref struct TOIterator<T> where T : class
{
    private readonly ReadOnlySpan<T> _span;
    private readonly Predicate<T> _predicate;

    public TOIterator(ReadOnlySpan<T> span, Predicate<T> predicate)
    {
        _span = span;
        _predicate = predicate;
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Predicate<T> _predicate;

        public Enumerator(TOIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _predicate = iterator._predicate;
        }

        public ref readonly T Current => ref _enumerator.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_predicate(Current))
                    return true;
            }
            return false;
        }
    }

    private T this[int index]
    {
        get
        {
            int i = 0;
            foreach (T data in this)
            {
                if (i++ == index)
                    return data;
            }
            return null;
        }
    }

    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    public bool Any(Predicate<T> furtherPredicate)
    {
        foreach (T data in this)
        {
            if (furtherPredicate(data))
                return true;
        }
        return false;
    }

    public bool All(Predicate<T> furtherPredicate)
    {
        foreach (T data in this)
        {
            if (!furtherPredicate(data))
                return false;
        }
        return true;
    }

    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

    public int Count(Predicate<T> furtherPredicate)
    {
        int count = 0;
        foreach (T data in this)
        {
            if (furtherPredicate(data))
                count++;
        }
        return count;
    }

    public List<T> ToList() => [.. this];

    public T[] ToArray() => [.. ToList()];

    public bool TryGetFirst(out T found) => (found = this[0]) is not null;

    public bool TryGetFirst(Predicate<T> furtherPredicate, out T found)
    {
        foreach (T data in this)
        {
            if (furtherPredicate(data))
            {
                found = data;
                return true;
            }
        }
        found = null;
        return false;
    }
}

/// <summary>
/// 含排除对象的对象迭代器。
/// <para/>示例：
/// <code>
/// public override void AI(NPC npc) //GlobalNPC类的重写方法
/// {
///     foreach (NPC kingSlime in TOIteratorFactory.NewActiveNPCIterator(n => n.type == NPCID.KingSlime), npc)
///     {
///         //代码
///     }
/// }
/// </code>
/// </summary>
public readonly ref struct TOExclusiveIterator<T> where T : class
{
    private readonly ReadOnlySpan<T> _span;
    private readonly Predicate<T> _predicate;
    private readonly HashSet<T> _exclusions;

    public TOExclusiveIterator(ReadOnlySpan<T> span, Predicate<T> predicate, HashSet<T> exceptions)
    {
        _span = span;
        _predicate = predicate;
        _exclusions = exceptions;
    }

    public TOExclusiveIterator(ReadOnlySpan<T> span, Predicate<T> predicate, params ReadOnlySpan<T> exceptionIndexes)
    {
        _span = span;
        _predicate = predicate;
        _exclusions = [.. exceptionIndexes];
    }

    public Enumerator GetEnumerator() => new(this);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Predicate<T> _predicate;
        private readonly HashSet<T> _exceptions;

        public Enumerator(TOExclusiveIterator<T> iterator)
        {
            _enumerator = iterator._span.GetEnumerator();
            _predicate = iterator._predicate;
            _exceptions = iterator._exclusions;
        }

        public ref readonly T Current => ref _enumerator.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_predicate(Current) && !_exceptions.Contains(Current))
                    return true;
            }
            return false;
        }
    }

    private T this[int index]
    {
        get
        {
            int i = 0;
            foreach (T data in this)
            {
                if (i++ == index)
                    return data;
            }
            return null;
        }
    }

    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    public bool Any(Predicate<T> furtherPredicate)
    {
        foreach (T data in this)
        {
            if (furtherPredicate(data))
                return true;
        }
        return false;
    }

    public bool All(Predicate<T> furtherPredicate)
    {
        foreach (T data in this)
        {
            if (!furtherPredicate(data))
                return false;
        }
        return true;
    }

    public int Count()
    {
        int count = 0;
        foreach (T _ in this)
            count++;
        return count;
    }

    public int Count(Predicate<T> furtherPredicate)
    {
        int count = 0;
        foreach (T data in this)
        {
            if (furtherPredicate(data))
                count++;
        }
        return count;
    }

    public List<T> ToList() => [.. this];

    public T[] ToArray() => [.. ToList()];

    public bool TryGetFirst(out T found) => (found = this[0]) is not null;
}

/// <summary>
/// 对象迭代器工厂。
/// </summary>
public static class TOIteratorFactory
{
    public static ReadOnlySpan<NPC> NPCSpan => Main.npc.AsSpan(0, Main.maxNPCs);

    public static ReadOnlySpan<Projectile> ProjectileSpan => Main.projectile.AsSpan(0, Main.maxProjectiles);

    public static ReadOnlySpan<Player> PlayerSpan => Main.player.AsSpan(0, Main.netMode != NetmodeID.SinglePlayer ? Main.maxPlayers : 1);

    public static ReadOnlySpan<Item> ItemSpan => Main.item.AsSpan(0, Main.maxItems);

    public static ReadOnlySpan<Dust> DustSpan => Main.dust.AsSpan(0, Main.maxDust);


    public static TOIterator<NPC> NewNPCIterator(Predicate<NPC> predicate) => new(NPCSpan, predicate);

    public static TOExclusiveIterator<NPC> NewNPCIterator(Predicate<NPC> predicate, params ReadOnlySpan<NPC> exclusions) => new(NPCSpan, predicate, exclusions);

    public static TOIterator<NPC> NewActiveNPCIterator() => new(NPCSpan, n => n.active);

    public static TOIterator<NPC> NewActiveNPCIterator(Predicate<NPC> predicate) => new(NPCSpan, n => n.active && predicate(n));

    public static TOExclusiveIterator<NPC> NewActiveNPCIterator(Predicate<NPC> predicate, params ReadOnlySpan<NPC> exclusions) => new(NPCSpan, n => n.active && predicate(n), exclusions);

    public static TOIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate) => new(ProjectileSpan, predicate);

    public static TOExclusiveIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate, params ReadOnlySpan<Projectile> exclusions) => new(ProjectileSpan, predicate, exclusions);

    public static TOIterator<Projectile> NewActiveProjectileIterator() => new(ProjectileSpan, p => p.active);

    public static TOIterator<Projectile> NewActiveProjectileIterator(Predicate<Projectile> predicate) => new(ProjectileSpan, p => p.active && predicate(p));

    public static TOExclusiveIterator<Projectile> NewActiveProjectileIterator(Predicate<Projectile> predicate, params ReadOnlySpan<Projectile> exclusions) => new(ProjectileSpan, p => p.active && predicate(p), exclusions);

    public static TOIterator<Player> NewPlayerIterator(Predicate<Player> predicate) => new(PlayerSpan, predicate);

    public static TOExclusiveIterator<Player> NewPlayerIterator(Predicate<Player> predicate, params ReadOnlySpan<Player> exclusions) => new(PlayerSpan, predicate, exclusions);

    public static TOIterator<Player> NewActivePlayerIterator() => new(PlayerSpan, p => p.active);

    public static TOIterator<Player> NewActivePlayerIterator(Predicate<Player> predicate) => new(PlayerSpan, p => p.active && predicate(p));

    public static TOExclusiveIterator<Player> NewActivePlayerIterator(Predicate<Player> predicate, params ReadOnlySpan<Player> exclusions) => new(PlayerSpan, p => p.active && predicate(p), exclusions);

    public static TOIterator<Dust> NewDustIterator(Predicate<Dust> predicate) => new(DustSpan, predicate);

    public static TOExclusiveIterator<Dust> NewDustIterator(Predicate<Dust> predicate, params ReadOnlySpan<Dust> exclusions) => new(DustSpan, predicate, exclusions);

    public static TOIterator<Item> NewItemIterator(Predicate<Item> predicate) => new(ItemSpan, predicate);

    public static TOExclusiveIterator<Item> NewItemIterator(Predicate<Item> predicate, params ReadOnlySpan<Item> exclusions) => new(ItemSpan, predicate, exclusions);

    public static TOIterator<Item> NewActiveItemIterator(Predicate<Item> predicate) => new(ItemSpan, i => i.active && predicate(i));

    public static TOExclusiveIterator<Item> NewActiveItemIterator(Predicate<Item> predicate, params ReadOnlySpan<Item> exclusions) => new(ItemSpan, i => i.active && predicate(i), exclusions);
}
