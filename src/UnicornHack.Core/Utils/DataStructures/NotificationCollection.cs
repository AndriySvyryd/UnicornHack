using System.Collections.ObjectModel;
using UnicornHack.Utils.MessagingECS;

namespace UnicornHack.Utils.DataStructures;

public class NotificationCollection<TItem> : ObservableCollection<TItem>
    where TItem : NotificationCollectionItem
{
    private readonly Component _component;
    private readonly string _propertyName;

    public NotificationCollection(Component component, string propertyName)
    {
        _component = component;
        _propertyName = propertyName;
    }

    protected override void InsertItem(int index, TItem item)
    {
        item.SetContainingComponent(_component, _propertyName);
        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, TItem item)
    {
        this[index].SetContainingComponent(null, null);
        item.SetContainingComponent(_component, _propertyName);
        base.SetItem(index, item);
    }

    protected override void RemoveItem(int index)
    {
        this[index].SetContainingComponent(null, null);
        base.RemoveItem(index);
    }

    protected override void ClearItems()
    {
        foreach (var item in this)
        {
            item.SetContainingComponent(null, null);
        }

        base.ClearItems();
    }
}