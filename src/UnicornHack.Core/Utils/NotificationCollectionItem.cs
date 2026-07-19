using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Utils;

public abstract class NotificationCollectionItem : NotificationEntity
{
    public Component? ContainingComponent { get; set; }

    public string? ContainingPropertyName { get; set; }

    public void SetContainingComponent(Component? component, string? propertyName)
    {
        Debug.Assert(component == null || ContainingComponent == null || ContainingComponent == component);
        ContainingComponent = component;
        ContainingPropertyName = propertyName;
    }

    protected override void OnPropertyValueChanged<T>(string propertyName, T oldValue, T newValue)
    {
        if (ContainingComponent is { } component
            && ContainingPropertyName is { } containingPropertyName)
        {
            component.OnNestedPropertyValueChanged(
                containingPropertyName, propertyName, this, oldValue, newValue);
        }
    }
}