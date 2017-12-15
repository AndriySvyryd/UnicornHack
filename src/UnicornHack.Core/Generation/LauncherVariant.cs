using System;
using System.Collections.Generic;
using CSharpScriptSerialization;

namespace UnicornHack.Generation
{
    public class LauncherVariant : ItemVariant
    {
        private string _projectile;

        public virtual string Projectile
        {
            get => _projectile ?? (Base as LauncherVariant)?.Projectile;
            set => _projectile = value;
        }

        public override Item Instantiate(Game game)
        {
            var launcher = (Launcher)base.Instantiate(game);
            launcher.Projectile = Loader.Get(Projectile).Instantiate(game).AddReference().Referenced;
            return launcher;
        }

        protected override Item CreateInstance(Game game) => new Launcher(game);

        private static readonly CSScriptSerializer Serializer =
            new PropertyCSScriptSerializer<LauncherVariant>(GetPropertyConditions<LauncherVariant>());

        protected new static Dictionary<string, Func<TItemVariant, object, bool>> GetPropertyConditions<TItemVariant>()
            where TItemVariant : LauncherVariant
        {
            var propertyConditions = ItemVariant.GetPropertyConditions<TItemVariant>();
            propertyConditions.Add(nameof(Projectile), (o, v) => v != null);
            return propertyConditions;
        }

        public override ICSScriptSerializer GetSerializer() => Serializer;
    }
}