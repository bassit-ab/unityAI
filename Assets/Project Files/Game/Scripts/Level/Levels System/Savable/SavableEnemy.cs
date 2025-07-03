using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    public class SavableEnemy : AbstractSavable
    {
        [SerializeField] float health = 1;
        [SerializeField] float damage = 1;
        [SerializeField] float fireRate = 1;
        [SerializeField, LevelDataPicker(LevelDataType.Weapon)] string gunId = "default_gun";

        [Space]
        [SerializeField] DropableItemType dropType = DropableItemType.None;
        [ShowIf("IsDropACurrency")]
        [SerializeField] CurrencyType dropCurrencyType;
        [ShowIf("HaveDrop")]
        [SerializeField] int dropItemsCount = 1;
        [ShowIf("HaveDrop")]
        [SerializeField] float dropItemValue = 1;

        public float Health { get => health; set => health = value; }
        public float Damage { get => damage; set => damage = value; }
        public float FireRate { get => fireRate; set => fireRate = value; }
        public string GunId { get => gunId; set => gunId = value; }

        public DropableItemType DropType { get => dropType; set => dropType = value; }
        public CurrencyType DropCurrencyType { get => dropCurrencyType; set => dropCurrencyType = value; }
        public int DropItemsCount { get => dropItemsCount; set => dropItemsCount = value; }
        public float DropItemValue { get => dropItemValue; set => dropItemValue = value; }

        protected bool IsDropACurrency => dropType == DropableItemType.Money;
        protected bool HaveDrop => dropType != DropableItemType.None;
    }
}
