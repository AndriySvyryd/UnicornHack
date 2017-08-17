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

        public IReadOnlyList<T> GetAll()
        {
            if (_objects == null)
            {
                LoadAll();
                _objects = NameLookup.Values.ToList();
            }

            return _objects;
        }
    }
}