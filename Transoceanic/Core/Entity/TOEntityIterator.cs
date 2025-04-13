using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Transoceanic.Data;

namespace Transoceanic.Core;

public readonly ref struct TOEntityIterator<T> where T : Entity
{
    private readonly ReadOnlySpan<T> _entities;
    private readonly Predicate<T> _predicate;
    private readonly bool _hasException = false;
    private readonly HashSet<int> _exceptions = [];

    /* 未使用的索引器
    public T this[int index]
    {
        get
        {
            int i = 0;
            foreach (T entity in this)
            {
                if (i++ == index)
                    return entity;
            }
            return null;
        }
    }
    */

    #region 构造器
    public TOEntityIterator(ReadOnlySpan<T> entities, Predicate<T> predicate)
    {
        _entities = entities;
        _predicate = predicate;
    }

    public TOEntityIterator(ReadOnlySpan<T> entities, Predicate<T> predicate, HashSet<int> exceptions) : this(entities, predicate)
    {
        _hasException = true;
        _exceptions = exceptions;
    }

    public TOEntityIterator(ReadOnlySpan<T> entities, Predicate<T> predicate, params int[] exceptionIndexes) : this(entities, predicate)
    {
        _hasException = true;
        _exceptions = [.. exceptionIndexes];
    }

    public TOEntityIterator(ReadOnlySpan<T> entities, Predicate<T> predicate, params T[] exceptionInstances) : this(entities, predicate)
    {
        _hasException = true;
        foreach (T exception in exceptionInstances)
            _exceptions.Add(exception.whoAmI);
    }
    #endregion

    #region 迭代器
    public Enumerator GetEnumerator() => new(_entities.GetEnumerator(), _predicate, _hasException , _exceptions);

    public ref struct Enumerator
    {
        private ReadOnlySpan<T>.Enumerator _enumerator;
        private readonly Predicate<T> _predicate;
        private readonly bool _hasException = false;
        private readonly HashSet<int> _exceptions = [];

        public Enumerator(ReadOnlySpan<T>.Enumerator enumerator, Predicate<T> predicate, bool hasException, HashSet<int> exceptions)
        {
            _enumerator = enumerator;
            _predicate = predicate;
            _hasException = hasException;
            _exceptions = exceptions;
        }

        public readonly T Current => _enumerator.Current;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            while (_enumerator.MoveNext())
            {
                if (_predicate(_enumerator.Current) && (!_hasException || !_exceptions.Contains(_enumerator.Current.whoAmI)))
                    return true;
            }
            return false;
        }
    }
    #endregion

    #region 查询方法
    public bool Any()
    {
        foreach (T _ in this)
            return true;
        return false;
    }

    public bool Any(Predicate<T> furtherPredicate)
    {
        foreach (T entity in this)
        {
            if (furtherPredicate(entity))
                return true;
        }
        return false;
    }

    public bool All(Predicate<T> furtherPredicate)
    {
        foreach (T entity in this)
        {
            if (!furtherPredicate(entity))
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
        foreach (T entity in this)
        {
            if (furtherPredicate(entity))
                count++;
        }
        return count;
    }

    public List<T> ToList() => [.. this];

    public T[] ToArray()
    {
        T[] array = new T[Count()];
        int index = 0;
        foreach (T entity in this)
            array[index++] = entity;
        return array;
    }

    public bool TryGetFirst(out T entityFound)
    {
        foreach (T entity in this)
        {
            entityFound = entity;
            return true;
        }
        entityFound = null;
        return false;
    }

    public bool TryGetFirst(Predicate<T> furtherPredicate, out T entityFound)
    {
        foreach (T entity in this)
        {
            if (furtherPredicate(entity))
            {
                entityFound = entity;
                return true;
            }
        }
        entityFound = null;
        return false;
    }

    public bool Min<TCompare>(Func<T, TCompare> selector, out TCompare value)
        where TCompare : notnull, IComparable<TCompare>
    {
        value = default;
        bool hasValue = false;
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (!hasValue || currentValue.CompareTo(value) < 0)
            {
                hasValue = true;
                value = currentValue;
            }
        }
        return hasValue;
    }

    public bool Min<TCompare>(Func<T, TCompare> selector, Predicate<T> priorityPredicate, out bool hasPriority, out TCompare value)
        where TCompare : notnull, IComparable<TCompare>
    {
        value = default;
        bool hasValue = false;
        hasPriority = false;
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    hasValue = true;
                    hasPriority = true;
                    value = selector(entity);
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (!hasValue || currentValue.CompareTo(value) < 0)
                    {
                        hasValue = true;
                        value = currentValue;
                    }
                    break;
            }
        }
        return hasValue;
    }

    public bool MinFrom<TCompare>(Func<T, TCompare> selector, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        TCompare temp = default;
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (targetEntity is null || currentValue.CompareTo(temp) < 0)
            {
                temp = currentValue;
                targetEntity = entity;
            }
        }
        return targetEntity is not null;
    }

    public bool MinFrom<TCompare>(Func<T, TCompare> selector, Predicate<T> priorityPredicate, out bool hasPriority, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        TCompare temp = default;
        hasPriority = false;
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    temp = selector(entity);
                    targetEntity = entity;
                    hasPriority = true;
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (targetEntity is null || currentValue.CompareTo(temp) < 0)
                    {
                        temp = currentValue;
                        targetEntity = entity;
                    }
                    break;
            }
        }
        return targetEntity is not null;
    }

    public bool MinFromPrecise<TCompare>(Func<T, TCompare> selector, Func<LinkedList<T>, T> furtherSelector, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        bool hasValue = false;
        TCompare temp = default;
        LinkedList<T> tempEntityList = [];
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (!hasValue)
            {
                hasValue = true;
                temp = currentValue;
                tempEntityList.AddLast(entity);
                continue;
            }
            switch (currentValue.CompareTo(temp))
            {
                case > 0:
                    break;
                case < 0:
                    temp = currentValue;
                    tempEntityList.Clear();
                    tempEntityList.AddLast(entity);
                    break;
                case 0:
                    tempEntityList.AddLast(entity);
                    break;
            }
        }
        targetEntity = tempEntityList.Count switch
        {
            0 => null,
            1 => tempEntityList.First.Value,
            _ => furtherSelector(tempEntityList)
        };
        return targetEntity is not null;
    }

    public bool MinFromPrecise<TCompare>(Func<T, TCompare> selector, Func<LinkedList<T>, T> furtherSelector, Predicate<T> priorityPredicate, out bool hasPriority, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        bool hasValue = false;
        hasPriority = false;
        TCompare temp = default;
        LinkedList<T> tempEntityList = [];
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    temp = selector(entity);
                    targetEntity = entity;
                    hasPriority = true;
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (!hasValue)
                    {
                        hasValue = true;
                        temp = currentValue;
                        tempEntityList.AddLast(entity);
                        continue;
                    }
                    switch (currentValue.CompareTo(temp))
                    {
                        case > 0:
                            break;
                        case < 0:
                            temp = currentValue;
                            tempEntityList.Clear();
                            tempEntityList.AddLast(entity);
                            break;
                        case 0:
                            tempEntityList.AddLast(entity);
                            break;
                    }
                    break;
            }
        }
        targetEntity = tempEntityList.Count switch
        {
            0 => null,
            1 => tempEntityList.First.Value,
            _ => furtherSelector(tempEntityList)
        };
        return targetEntity is not null;
    }

    public bool Max<TCompare>(Func<T, TCompare> selector, out TCompare value)
        where TCompare : notnull, IComparable<TCompare>
    {
        value = default;
        bool hasValue = false;
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (!hasValue || currentValue.CompareTo(value) > 0)
            {
                hasValue = true;
                value = currentValue;
            }
        }
        return hasValue;
    }

    public bool Max<TCompare>(Func<T, TCompare> selector, Predicate<T> priorityPredicate, out bool hasPriority, out TCompare value)
        where TCompare : notnull, IComparable<TCompare>
    {
        value = default;
        bool hasValue = false;
        hasPriority = false;
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    hasValue = true;
                    hasPriority = true;
                    value = selector(entity);
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (!hasValue || currentValue.CompareTo(value) > 0)
                    {
                        hasValue = true;
                        value = currentValue;
                    }
                    break;
            }
        }
        return hasValue;
    }

    public bool MaxFrom<TCompare>(Func<T, TCompare> selector, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        TCompare temp = default;
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (targetEntity is null || currentValue.CompareTo(temp) > 0)
            {
                temp = currentValue;
                targetEntity = entity;
            }
        }
        return targetEntity is not null;
    }

    public bool MaxFrom<TCompare>(Func<T, TCompare> selector, Predicate<T> priorityPredicate, out bool hasPriority, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        TCompare temp = default;
        hasPriority = false;
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    temp = selector(entity);
                    targetEntity = entity;
                    hasPriority = true;
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (targetEntity is null || currentValue.CompareTo(temp) > 0)
                    {
                        temp = currentValue;
                        targetEntity = entity;
                    }
                    break;
            }
        }
        return targetEntity is not null;
    }

    public bool MaxFromPrecise<TCompare>(Func<T, TCompare> selector, Func<LinkedList<T>, T> furtherSelector, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        bool hasValue = false;
        TCompare temp = default;
        LinkedList<T> tempEntityList = [];
        foreach (T entity in this)
        {
            TCompare currentValue = selector(entity);
            if (!hasValue)
            {
                hasValue = true;
                temp = currentValue;
                tempEntityList.AddLast(entity);
                continue;
            }
            switch (currentValue.CompareTo(temp))
            {
                case < 0:
                    break;
                case > 0:
                    temp = currentValue;
                    tempEntityList.Clear();
                    tempEntityList.AddLast(entity);
                    break;
                case 0:
                    tempEntityList.AddLast(entity);
                    break;
            }
        }
        targetEntity = tempEntityList.Count switch
        {
            0 => null,
            1 => tempEntityList.First.Value,
            _ => furtherSelector(tempEntityList)
        };
        return targetEntity is not null;
    }

    public bool MaxFromPrecise<TCompare>(Func<T, TCompare> selector, Func<LinkedList<T>, T> furtherSelector, Predicate<T> priorityPredicate, out bool hasPriority, out T targetEntity)
        where TCompare : notnull, IComparable<TCompare>
    {
        targetEntity = null;
        bool hasValue = false;
        hasPriority = false;
        TCompare temp = default;
        LinkedList<T> tempEntityList = [];
        foreach (T entity in this)
        {
            switch (TOMathHelper.GetTwoBooleanStatus(hasPriority, priorityPredicate(entity)))
            {
                case TwoBooleanStatus.ATrue:
                    break;
                case TwoBooleanStatus.BTrue:
                    temp = selector(entity);
                    targetEntity = entity;
                    hasPriority = true;
                    break;
                case TwoBooleanStatus.Both:
                case TwoBooleanStatus.Neither:
                    TCompare currentValue = selector(entity);
                    if (!hasValue)
                    {
                        hasValue = true;
                        temp = currentValue;
                        tempEntityList.AddLast(entity);
                        continue;
                    }
                    switch (currentValue.CompareTo(temp))
                    {
                        case < 0:
                            break;
                        case > 0:
                            temp = currentValue;
                            tempEntityList.Clear();
                            tempEntityList.AddLast(entity);
                            break;
                        case 0:
                            tempEntityList.AddLast(entity);
                            break;
                    }
                    break;
            }
        }
        targetEntity = tempEntityList.Count switch
        {
            0 => null,
            1 => tempEntityList.First.Value,
            _ => furtherSelector(tempEntityList)
        };
        return targetEntity is not null;
    }
    #endregion
}

public static class TOEntityIteratorCreator
{
    public static ReadOnlySpan<NPC> NPCSpan => Main.npc.AsSpan(0, Main.maxNPCs);
    public static ReadOnlySpan<Projectile> ProjectileSpan => Main.projectile.AsSpan(0, Main.maxProjectiles);
    public static ReadOnlySpan<Player> PlayerSpan => Main.player.AsSpan(0, Main.maxProjectiles);
    public static ReadOnlySpan<Item> ItemSpan => Main.item.AsSpan(0, Main.maxProjectiles);

    public static TOEntityIterator<NPC> NewNPCIterator(Predicate<NPC> predicate) => new(NPCSpan, predicate);
    public static TOEntityIterator<NPC> NewNPCIterator(Predicate<NPC> predicate, HashSet<int> exceptions) => new(NPCSpan, predicate, exceptions);
    public static TOEntityIterator<NPC> NewNPCIterator(Predicate<NPC> predicate, params int[] exceptionIndexes) => new(NPCSpan, predicate, exceptionIndexes);
    public static TOEntityIterator<NPC> NewNPCIterator(Predicate<NPC> predicate, params NPC[] exceptionInstances) => new(NPCSpan, predicate, exceptionInstances);
    public static TOEntityIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate) => new(ProjectileSpan, predicate);
    public static TOEntityIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate, HashSet<int> exceptions) => new(ProjectileSpan, predicate, exceptions);
    public static TOEntityIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate, params int[] exceptionIndexes) => new(ProjectileSpan, predicate, exceptionIndexes);
    public static TOEntityIterator<Projectile> NewProjectileIterator(Predicate<Projectile> predicate, params Projectile[] exceptionInstances) => new(ProjectileSpan, predicate, exceptionInstances);
    public static TOEntityIterator<Player> NewPlayerIterator(Predicate<Player> predicate) => new(PlayerSpan, predicate);
    public static TOEntityIterator<Player> NewPlayerIterator(Predicate<Player> predicate, HashSet<int> exceptions) => new(PlayerSpan, predicate, exceptions);
    public static TOEntityIterator<Player> NewPlayerIterator(Predicate<Player> predicate, params int[] exceptionIndexes) => new(PlayerSpan, predicate, exceptionIndexes);
    public static TOEntityIterator<Player> NewPlayerIterator(Predicate<Player> predicate, params Player[] exceptionInstances) => new(PlayerSpan, predicate, exceptionInstances);
    public static TOEntityIterator<Item> NewItemIterator(Predicate<Item> predicate) => new(ItemSpan, predicate);
    public static TOEntityIterator<Item> NewItemIterator(Predicate<Item> predicate, HashSet<int> exceptions) => new(ItemSpan, predicate, exceptions);
    public static TOEntityIterator<Item> NewItemIterator(Predicate<Item> predicate, params int[] exceptionIndexes) => new(ItemSpan, predicate, exceptionIndexes);
    public static TOEntityIterator<Item> NewItemIterator(Predicate<Item> predicate, params Item[] exceptionInstances) => new(ItemSpan, predicate, exceptionInstances);
}
