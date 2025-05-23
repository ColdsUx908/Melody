using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Transoceanic.Core.GameData;
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

    public bool TryGetValue(TEntity entity, out TOverride overrideInstance)
    {
        int type = entity.GetEntityType();
        if (TryGetValue(type, out List<TOverride> overrideList))
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