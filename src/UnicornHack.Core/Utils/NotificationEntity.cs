﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UnicornHack.Utils
{
    public abstract class NotificationEntity : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public static readonly PropertyChangedEventArgs CountPropertyChanged
            = new PropertyChangedEventArgs("Count");

        public static readonly PropertyChangingEventArgs CountPropertyChanging
            = new PropertyChangingEventArgs("Count");

        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangingEventHandler PropertyChanging;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void SetWithNotify<T>(
            T value,
            ref T field,
            [CallerMemberName] string propertyName = "")
        {
            if (Equals(field, value))
            {
                return;
            }

            FirePropertyChanging(propertyName);
            field = value;
            FirePropertyChanged(propertyName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FirePropertyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void FirePropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
