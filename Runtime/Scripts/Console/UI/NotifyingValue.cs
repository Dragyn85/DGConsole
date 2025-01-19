using System;

namespace DragynGames.Console
{
    public class NotifyingValue<T>
    {
        T value;
        private Action<T> ValueChanged;
        
        public NotifyingValue(T value, Action<T> onValueChanged)
        {
            this.value = value;
            ValueChanged = onValueChanged;
        }
        
        public void Set(T value)
        {
            this.value = value;
            ValueChanged?.Invoke(value);
        }

        public T Get()
        {
            return value;
        }
    }
}