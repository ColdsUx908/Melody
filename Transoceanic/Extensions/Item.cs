namespace Transoceanic.Extensions;

public static partial class TOExtensions
{
    extension(Item item)
    {
        public TOGlobalItem Ocean() => item.GetGlobalItem<TOGlobalItem>();

        public T GetModItem<T>() where T : ModItem => item.ModItem as T;
    }
}
