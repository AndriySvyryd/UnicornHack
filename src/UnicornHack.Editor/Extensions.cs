using System.Reflection;

namespace UnicornHack.Editor;

public static class Extensions
{
    public static bool TryCreateInstance<T>(this Type type, [NotNullWhen(returnValue: true)] out T? instance)
        where T : class
    {
        try
        {
            var defaultCtor = type.GetConstructor(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                Array.Empty<Type>(),
                modifiers: null);

            instance = defaultCtor != null
                ? (T)Activator.CreateInstance(
                    type,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    binder: null,
                    args: null,
                    culture: null)!
                : null;

            return instance != null;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to create instance of {type.FullName} in {type.AssemblyQualifiedName}.", ex);
        }
    }
}
