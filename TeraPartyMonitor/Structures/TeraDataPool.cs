using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace TeraPartyMonitor.Structures
{
    internal class TeraDataPool<T> : Collection<T>
    {
        public event Action<T> ItemAdded;
        public event Action<T, T> ItemChanged;
        public event Action<T> ItemRemoved;

        public TeraDataPool() : base() { }

        public TeraDataPool(int capacity) : base(new List<T>(capacity)) { }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            ItemAdded?.Invoke(item);
        }

        protected override void RemoveItem(int index)
        {
            T item = base[index];
            base.RemoveItem(index);
            ItemRemoved?.Invoke(item);
        }

        protected override void SetItem(int index, T item)
        {
            T oldItem = base[index];
            base.SetItem(index, item);
            ItemChanged?.Invoke(oldItem, item);
        }
    }
}
