using System;
using System.Collections.Generic;
using System.Linq;

namespace UnicornHack.Utils
{
    public class CSScriptLoader<T> : CSScriptLoaderBase<T>
        where T : ILoadable
    {
        private IReadOnlyList<T> _objects;

        public CSScriptLoader(string relativePath, Type dataType = null)
            : base(relativePath, dataType)
        {
        }

        protected override void EnsureLoaded()
        {
            base.EnsureLoaded();

            if (_objects == null)
            {
                _objects = NameLookup.Values.ToList();
            }
        }

        public IReadOnlyList<T> GetAsList()
        {
            EnsureLoaded();

            return _objects;
        }
    }
}