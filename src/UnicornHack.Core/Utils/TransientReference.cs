using System;

namespace UnicornHack.Utils
{
    public struct TransientReference<T> : IDisposable where T : IReferenceable
    {
        public TransientReference(T referenced)
        {
            referenced.AddReference();
            Referenced = referenced;
        }

        public T Referenced { get; private set; }

        public void Dispose()
        {
            Referenced?.RemoveReference();
            Referenced = default;
        }
    }
}
