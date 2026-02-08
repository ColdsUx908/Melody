namespace Transoceanic.Framework.Helpers.AbstractionHelpers;

public sealed class ParticleHelper : ModSystem, IResourceLoader
{
    public const string BaseParticleTexturePath = "Transoceanic/DataStructures/Particles/";

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
            string texturePath = type.Namespace.Replace('.', '/') + "/" + type.Name;
            if (templateInstance.TexturePath != "")
                texturePath = templateInstance.TexturePath;
            Asset<Texture2D> asset = ModContent.Request<Texture2D>(texturePath);
            Asset = asset;
            FieldInfo field = type.GetFields(TOReflectionUtils.StaticBindingFlags).AsValueEnumerable().FirstOrDefault(f => f.FieldType == typeof(Asset<Texture2D>) && f.HasAttribute<ParticleTextureAssetAttribute>() && !f.IsInitOnly && !f.IsLiteral);
            field?.SetValue(null, asset);
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
    private static Dictionary<Type, int> _particleTypes;

    private static List<Particle> _particles;
    private static List<Particle> _particlesToKill;

    //Lists used when drawing particles batched
    private static List<Particle> _particlesToDraw_AlphaBlend;
    private static List<Particle> _particlesToDraw_NonPremultiplied;
    private static List<Particle> _particlesToDraw_AdditiveBlend;

    public static void Draw(SpriteBatch spriteBatch)
    {
        if (Main.dedServ)
            return;

        if (_particles.Count == 0)
            return;

        //Batch the particles to avoid constant restarting of the spritebatch
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
            {
                if (particle.UseCustomDraw)
                    particle.CustomDraw(spriteBatch);
                else
                {
                    Texture2D texture = _particleCache[particle.Type].Texture;
                    Rectangle frame = texture.Frame(1, particle.FrameVariants, 0, particle.Variant);
                    spriteBatch.DrawFromCenter(texture, particle.Center - Main.screenPosition, particle.Color, frame, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
                }
            }
        }

        if (_particlesToDraw_NonPremultiplied.Count > 0)
        {
            EnterDrawRegion_NonPremultiplied(spriteBatch);

            foreach (Particle particle in _particlesToDraw_NonPremultiplied)
            {
                if (particle.UseCustomDraw)
                    particle.CustomDraw(spriteBatch);
                else
                {
                    Texture2D texture = _particleCache[particle.Type].Texture;
                    Rectangle frame = texture.Frame(1, particle.FrameVariants, 0, particle.Variant);
                    spriteBatch.DrawFromCenter(texture, particle.Center - Main.screenPosition, particle.Color, frame, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
                }
            }
        }

        if (_particlesToDraw_AdditiveBlend.Count > 0)
        {
            EnterDrawRegion_AdditiveBlend(spriteBatch);

            foreach (Particle particle in _particlesToDraw_AdditiveBlend)
            {
                if (particle.UseCustomDraw)
                    particle.CustomDraw(spriteBatch);
                else
                {
                    Texture2D texture = _particleCache[particle.Type].Texture;
                    Rectangle frame = texture.Frame(1, particle.FrameVariants, 0, particle.Variant);
                    spriteBatch.DrawFromCenter(texture, particle.Center - Main.screenPosition, particle.Color, frame, particle.Rotation, particle.Scale, SpriteEffects.None, 0f);
                }
            }
        }

        _particlesToDraw_AlphaBlend.Clear();
        _particlesToDraw_NonPremultiplied.Clear();
        _particlesToDraw_AdditiveBlend.Clear();

        ExitParticleDrawRegion(spriteBatch);
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
            particle.Center += particle.Velocity;
            particle.Time++;
            particle.Update();
        }

        //Clear out particles whose time is up
        _particles.RemoveAll(particle => (particle.Time >= particle.Lifetime && particle.AutoKillByLifeTime) || _particlesToKill.Contains(particle));
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

        On_Main.DrawInfernoRings += (orig, self) =>
        {
            Draw(Main.spriteBatch);
            orig(self);
        };
    }

    void IResourceLoader.OnModUnload()
    {
        ParticleDataCache._nextID = 0;

        foreach (Type type in _particleTypes.Keys)
        {
            FieldInfo field = type.GetFields(TOReflectionUtils.StaticBindingFlags).AsValueEnumerable().FirstOrDefault(f => f.FieldType == typeof(Asset<Texture2D>) && f.HasAttribute<ParticleTextureAssetAttribute>() && !f.IsInitOnly && !f.IsLiteral);
            field?.SetValue(null, null);
        }

        _particleCache = null;
        _particleTypes = null;
        _particles = null;
        _particlesToKill = null;
        _particlesToDraw_AlphaBlend = null;
        _particlesToDraw_NonPremultiplied = null;
        _particlesToDraw_AdditiveBlend = null;

    }

    public static void SpawnParticle(Particle particle) => SpawnParticle_Inner(particle, false);

    private static void SpawnParticle_Inner(Particle particle, bool forceSpawn = false)
    {
        if (Main.gamePaused || Main.dedServ || _particles is null)
            return;

        if (_particles.Count >= ParticleLimit && !particle.Important && !forceSpawn)
            return;

        _particles.Add(particle);
        particle.Type = _particleTypes[particle.GetType()];
        return;
    }

    public static void RemoveParticle(Particle particle)
    {
        if (Main.dedServ)
            return;

        _particlesToKill.Add(particle);
    }

    /// <summary>
    /// Gives you the amount of particle slots that are available. Useful when you need multiple particles at once to make an effect and dont want it to be only halfway drawn due to a lack of particle slots
    /// </summary>
    /// <returns></returns>
    public static int FreeSpacesAvailable
    {
        get
        {
            //Safety check
            if (Main.dedServ || _particles is null)
                return 0;

            return ParticleLimit - _particles.Count;
        }
    }

    /// <summary>
    /// Gives you the texture of the particle type. Useful for custom drawing
    /// </summary>
    public static Texture2D GetTexture(int type)
    {
        if (Main.dedServ)
            return null;

        return _particleCache[type].Texture;
    }
}
