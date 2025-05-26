using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Terraria;
using Transoceanic.Core.GameData.Utilities;
using Transoceanic.Core.IL;

namespace Transoceanic.Core.ExtraGameData;

public abstract class EntityOverride<TEntity> where TEntity : Entity
{
    public abstract int OverrideType { get; }

    /// <summary>
    /// 优先级，越大越先应用。
    /// </summary>
    public virtual decimal Priority { get; } = 0m;

    public virtual bool ShouldProcess => true;

    public abstract void Connect(TEntity entity);

    public abstract void Disconnect();

    public virtual void SetStaticDefaults() { }

    public virtual void SetDefaults() { }
}

public class EntityOverrideDictionary<TEntity, TOverride> : Dictionary<int, List<TOverride>>
    where TEntity : Entity
    where TOverride : EntityOverride<TEntity>
{
    public new IEnumerable<TOverride> Values =>
        from overrides in base.Values
        from overrideInstance in overrides
        select overrideInstance;

    /// <summary>
    /// 尝试获取指定实体的Override实例。
    /// <br/>按照 <see cref="EntityOverride{TEntity}.Priority"/> 降序依次尝试获取通过 <see cref="EntityOverride{TEntity}.ShouldProcess"/> 检测的Override实例。
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="overrideInstance"></param>
    /// <returns></returns>
    public bool TryGetOverride(TEntity entity, [NotNullWhen(true)] out TOverride overrideInstance)
    {
        if (TryGetValue(entity.GetEntityType(), out List<TOverride> overrideList))
        {
            foreach (TOverride temp in overrideList)
            {
                temp.Disconnect();
                temp.Connect(entity);
                if (temp.ShouldProcess)
                {
                    overrideInstance = temp;
                    return true;
                }
            }
        }
        overrideInstance = null;
        return false;
    }

    public void FillOverrides(Assembly assemblyToSearch)
    {
        Clear();
        foreach ((int type, List<TOverride> overrides) in
            TOReflectionUtils.GetTypeInstancesDerivedFrom<TOverride>(assemblyToSearch)
            .GroupBy(k => k.OverrideType)
            .ToDictionary(
                keySelector: k => k.Key,
                elementSelector: k => k.OrderByDescending(k => k.Priority).ToList()))
        {
            this[type] = overrides;
        }
    }
}