using System;
using System.Collections.Generic;
using Infrastructure.Services.FixedTickable.Core;
using UnityEngine;
using IFixedTickable = VContainer.Unity.IFixedTickable;

namespace Infrastructure.Services.FixedTickable
{
    public class FixedTickableService : IFixedTickableService, IFixedTickable
    {
        private readonly List<Core.IFixedTickable> _fixedTickables = new List<Core.IFixedTickable>();
        private readonly List<Core.IFixedTickable> _itemsToAdd = new List<Core.IFixedTickable>();
        private readonly List<Core.IFixedTickable> _itemsToRemove = new List<Core.IFixedTickable>();

        public void Add(Core.IFixedTickable fixedTickable) => _itemsToAdd.Add(fixedTickable);

        public void Remove(Core.IFixedTickable fixedTickable) => _itemsToRemove.Add(fixedTickable);

        public void FixedTick()
        {
            if (_itemsToAdd.Count > 0)
            {
                foreach (Core.IFixedTickable itemToAdd in _itemsToAdd)
                {
                    _fixedTickables.Add(itemToAdd);
                }

                _itemsToAdd.Clear();
            }

            if (_itemsToRemove.Count > 0)
            {
                foreach (Core.IFixedTickable itemToRemove in _itemsToRemove)
                {
                    _fixedTickables.Remove(itemToRemove);
                }

                _itemsToRemove.Clear();
            }

            foreach (Core.IFixedTickable fixedTickable in _fixedTickables)
            {
                try
                {
                    fixedTickable.FixedTick();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}