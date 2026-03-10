using System;
using System.Collections.Generic;
using Infrastructure.Services.Tickable.Core;
using UnityEngine;
using ITickable = VContainer.Unity.ITickable;

namespace Infrastructure.Services.Tickable
{
    public class TickableService : ITickableService, ITickable
    {
        private readonly List<Core.ITickable> _tickables = new List<Core.ITickable>();
        private readonly List<Core.ITickable> _itemsToAdd = new List<Core.ITickable>();
        private readonly List<Core.ITickable> _itemsToRemove = new List<Core.ITickable>();

        public void Add(Core.ITickable tickable) => _itemsToAdd.Add(tickable);

        public void Remove(Core.ITickable tickable) => _itemsToRemove.Add(tickable);

        public void Tick()
        {
            if (_itemsToAdd.Count > 0)
            {
                foreach (Core.ITickable itemToAdd in _itemsToAdd)
                {
                    _tickables.Add(itemToAdd);
                }

                _itemsToAdd.Clear();
            }

            if (_itemsToRemove.Count > 0)
            {
                foreach (Core.ITickable itemToRemove in _itemsToRemove)
                {
                    _tickables.Remove(itemToRemove);
                }

                _itemsToRemove.Clear();
            }

            foreach (Core.ITickable tickable in _tickables)
            {
                try
                {
                    tickable.Tick();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}