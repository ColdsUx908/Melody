using MonoMod.Utils;

namespace Transoceanic.Framework.Helpers.AbstractionHandlers;

public sealed class ParticleHandler : ModSystem, IResourceLoader
{
    public const string BaseParticleTexturePath = "Transoceanic/DataStructures/Particles/";

    /// <summary>
    /// 粒子数量限制。
    /// <br/>当 <see cref="_particles"/> 中的粒子数量达到该值时，除非新生成的粒子被标记为重要粒子，否则将不会被生成。
    /// </summary>
    public static int ParticleLimit { get; set; } = 5000;

    private sealed record ParticleDataCache
    {
        public static int _nextID = 0;

        public readonly Type Type;
        public readonly Particle TemplateInstance;
        public readonly int ID;
        public readonly Asset<Texture2D> Asset;
        public Texture2D Texture => Asset.Value;

        private ParticleDataCache(Type type, Particle templateInstance)
        {
            Type = type;
            TemplateInstance = templateInstance;
            ID = _nextID++;
            if (templateInstance.AutoLoadTexture)
            {
                string texturePath = type.Namespace.Replace('.', '/') + "/" + type.Name;
                if (templateInstance.TexturePath != "")
                    texturePath = templateInstance.TexturePath;
                Asset<Texture2D> asset = ModContent.Request<Texture2D>(texturePath);
                Asset = asset;
                FieldInfo field = type.GetFields(TOReflectionUtils.StaticBindingFlags).AsValueEnumerable().FirstOrDefault(f => f.FieldType == typeof(Asset<Texture2D>) && f.HasAttribute<ParticleTextureAssetAttribute>() && !f.IsInitOnly && !f.IsLiteral);
                field?.SetValue(null, asset);
            }
        }

        public static ParticleDataCache Create(Type type, Particle templateInstance)
        {
            if (_particleTypes.TryGetValue(type, out int existingId) && _particleCache.TryGetValue(existingId, out ParticleDataCache existingCache))
                return existingCache;

            ParticleDataCache newCache = new(type, templateInstance);

            _particleCache[newCache.ID] = newCache;
            _particleTypes[type] = newCache.ID;

            return newCache;
        }
    }

    private static Dictionary<int, ParticleDataCache> _particleCache;
    internal static Dictionary<Type, int> _particleTypes;

    private static List<Particle> _particles;
    private static List<Particle> _particlesToKill;

    private static List<Particle> _particlesToDraw_AlphaBlend;
    private static List<Particle> _particlesToDraw_NonPremultiplied;
    private static List<Particle> _particlesToDraw_AdditiveBlend;

    public static void Draw(SpriteBatch spriteBatch)
    {
        if (Main.dedServ)
            return;

        if (_particles.Count == 0)
            return;

        //提前分类粒子以减少spriteBatch状态切换次数
        foreach (Particle particle in _particles)
        {
            if (particle is null)
                continue;

            switch (particle.BlendMode)
            {
                case Particle.DrawBlendMode.AlphaBlend:
                    _particlesToDraw_AlphaBlend.Add(particle);
                    break;
                case Particle.DrawBlendMode.NonPremultiplied:
                    _particlesToDraw_NonPremultiplied.Add(particle);
                    break;
                case Particle.DrawBlendMode.AdditiveBlend:
                    _particlesToDraw_AdditiveBlend.Add(particle);
                    break;
            }
        }

        if (_particlesToDraw_AlphaBlend.Count > 0)
        {
            EnterDrawRegion_AlphaBlend(spriteBatch);

            foreach (Particle particle in _particlesToDraw_AlphaBlend)
                DrawParticle(spriteBatch, particle);
        }

        if (_particlesToDraw_NonPremultiplied.Count > 0)
        {
            EnterDrawRegion_NonPremultiplied(spriteBatch);

            foreach (Particle particle in _particlesToDraw_NonPremultiplied)
                DrawParticle(spriteBatch, particle);
        }

        if (_particlesToDraw_AdditiveBlend.Count > 0)
        {
            EnterDrawRegion_AdditiveBlend(spriteBatch);

            foreach (Particle particle in _particlesToDraw_AdditiveBlend)
                DrawParticle(spriteBatch, particle);
        }

        _particlesToDraw_AlphaBlend.Clear();
        _particlesToDraw_NonPremultiplied.Clear();
        _particlesToDraw_AdditiveBlend.Clear();

        ExitParticleDrawRegion(spriteBatch);

        static void DrawParticle(SpriteBatch spriteBatch, Particle particle)
        {
            if (particle.PreDraw(spriteBatch))
            {
                Texture2D texture = _particleCache[particle.Type].Texture;
                Rectangle? frame = particle.GetFrame();
                spriteBatch.DrawFromCenter(texture, particle.Center - Main.screenPosition, particle.Color, frame, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
            }

            particle.PostDraw(spriteBatch);
        }
    }

    public static void EnterDrawRegion_AlphaBlend(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public static void EnterDrawRegion_NonPremultiplied(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public static void EnterDrawRegion_AdditiveBlend(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        Main.Rasterizer.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
        Main.instance.GraphicsDevice.ScissorRectangle = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
    }

    public static void ExitParticleDrawRegion(SpriteBatch spriteBatch)
    {
        spriteBatch.End();
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
    }

    public override void PostUpdateEverything()
    {
        if (Main.dedServ)
            return;

        foreach (Particle particle in _particles)
        {
            if (particle is null)
                continue;
            particle.Timer++;
            particle.Update();
            particle.Center += particle.Velocity;
        }

        _particles.RemoveAll(particle => particle is null || (particle.Timer >= particle.Lifetime && particle.AutoKillByLifeTime) || _particlesToKill.Contains(particle));
        _particlesToKill.Clear();
    }

    void IResourceLoader.PostSetupContent()
    {
        _particleCache = [];
        _particleTypes = [];
        _particles = [];
        _particlesToKill = [];
        _particlesToDraw_AlphaBlend = [];
        _particlesToDraw_NonPremultiplied = [];
        _particlesToDraw_AdditiveBlend = [];

        ParticleDataCache._nextID = 0;

        foreach ((Type type, Particle instance) in TOReflectionUtils.GetTypesAndInstancesDerivedFrom<Particle>())
            ParticleDataCache.Create(type, instance);

        //在绘制狱火药水效果前绘制粒子
        On_Main.DrawInfernoRings += (orig, self) =>
        {
            Draw(Main.spriteBatch);
            orig(self);
        };
    }

    void IResourceLoader.OnModUnload()
    {
        ParticleDataCache._nextID = 0;

        if (_particleTypes is not null)
        {
            foreach (Type type in _particleTypes.Keys)
            {
                FieldInfo field = type.GetFields(TOReflectionUtils.StaticBindingFlags).AsValueEnumerable().FirstOrDefault(f => f.FieldType == typeof(Asset<Texture2D>) && f.HasAttribute<ParticleTextureAssetAttribute>() && !f.IsInitOnly && !f.IsLiteral);
                field?.SetValue(null, null);
            }
        }

        _particleCache = null;
        _particleTypes = null;
        _particles = null;
        _particlesToKill = null;
        _particlesToDraw_AlphaBlend = null;
        _particlesToDraw_NonPremultiplied = null;
        _particlesToDraw_AdditiveBlend = null;

    }

    /// <summary>
    /// 向 <see cref="_particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static void SpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false, out _);

    /// <summary>
    /// 尝试向 <see cref="_particles"/> 中添加一个粒子实例以生成该粒子。
    /// </summary>
    public static bool TrySpawnParticle(Particle particle)
    {
        SpawnParticle_Inner(particle, false, out bool success);
        return success;
    }

    /// <summary>
    /// 向 <see cref="_particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static void SpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough) => SpawnParticles_Inner(particles, onlySpawnWhenSpaceEnough, out _);

    /// <summary>
    /// 尝试向 <see cref="_particles"/> 中添加一组粒子实例以生成这些粒子。
    /// <br/>若需生成由多个粒子组成的效果，而不希望在粒子数量过多时生成部分粒子而破坏效果完整性，请使用该方法并将 <paramref name="onlySpawnWhenSpaceEnough"/> 设置为 true。
    /// </summary>
    public static bool TrySpawnParticles(List<Particle> particles, bool onlySpawnWhenSpaceEnough)
    {
        SpawnParticles_Inner(particles, onlySpawnWhenSpaceEnough, out bool success);
        return success;
    }

    private static void SpawnParticle_Inner(Particle particle, bool forceSpawn, out bool success)
    {
        success = false;

        if (Main.gamePaused || Main.dedServ || _particles is null)
            return;

        if (_particles.Count >= ParticleLimit && !particle.Important && !forceSpawn)
            return;

        _particles.Add(particle);
        success = true;
    }

    private static void SpawnParticles_Inner(List<Particle> particles, bool onlySpawnWhenSpaceEnough, out bool success)
    {
        success = false;

        if (Main.gamePaused || Main.dedServ || _particles is null)
            return;

        int newParticlesCount = particles.Count;
        if (onlySpawnWhenSpaceEnough && _particles.Count + newParticlesCount > ParticleLimit)
            return;

        _particles.AddRange(particles);

        success = true;
    }

    public static void AddToRemoveList(Particle particle)
    {
        if (Main.dedServ)
            return;

        _particlesToKill.Add(particle);
    }
}
