using System;

namespace CSharpSerialization
{
    public interface ICSScriptSerializerFactory
    {
        ICSScriptSerializer TryCreate(Type type);
    }
}