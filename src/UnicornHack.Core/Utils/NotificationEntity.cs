using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnicornHack.Utils
{
    public class NotificationEntity : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public static readonly PropertyChangedEventArgs CountPropertyChanged
            = new PropertyChangedEventArgs("Count");

        public static readonly PropertyChangingEventArgs CountPropertyChanging
            = new PropertyChangingEventArgs("Count");

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            if (!Equals(field, value))
            {
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}