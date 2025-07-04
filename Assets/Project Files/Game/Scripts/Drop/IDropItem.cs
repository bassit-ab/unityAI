﻿using UnityEngine;

namespace Watermelon
{
    public interface IDropItem
    {
        public DropableItemType DropItemType { get; }

        public void Initialise();
        public void Unload();

        public GameObject GetDropObject(DropData dropData);
    }
}