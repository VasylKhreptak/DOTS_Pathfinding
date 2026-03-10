using System;
using System.Collections.Generic;
using Infrastructure.Services.LateTickable.Core;
using UnityEngine;
using ILateTickable = VContainer.Unity.ILateTickable;

namespace Infrastructure.Services.LateTickable
{
    public class LateTickableService : ILateTickableService, ILateTickable
    {
        private readonly List<Core.ILateTickable> _lateTickables = new List<Core.ILateTickable>();
        private readonly List<Core.ILateTickable> _itemsToAdd = new List<Core.ILateTickable>();
        private readonly List<Core.ILateTickable> _itemsToRemove = new List<Core.ILateTickable>();

        public void Add(Core.ILateTickable lateTickable) => _itemsToAdd.Add(lateTickable);

        public void Remove(Core.ILateTickable lateTickable) => _itemsToRemove.Add(lateTickable);

        public void LateTick()
        {
            if (_itemsToAdd.Count > 0)
            {
                foreach (Core.ILateTickable itemToAdd in _itemsToAdd)
                {
                    _lateTickables.Add(itemToAdd);
                }

                _itemsToAdd.Clear();
            }

            if (_itemsToRemove.Count > 0)
            {
                foreach (Core.ILateTickable itemToRemove in _itemsToRemove)
                {
                    _lateTickables.Remove(itemToRemove);
                }

                _itemsToRemove.Clear();
            }

            foreach (Core.ILateTickable lateTickable in _lateTickables)
            {
                try
                {
                    lateTickable.LateTick();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}