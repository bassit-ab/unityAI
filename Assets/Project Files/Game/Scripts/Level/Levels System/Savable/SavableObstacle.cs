using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Watermelon
{
    public class SavableObstacle : AbstractSavable
    {
        [SerializeField] [OnValueChanged("UpdateHealthText")] int health = 1;
        [SerializeField] float boosterHeight;

        [Space]
        [SerializeField] DropableItemType dropType = DropableItemType.None;
        [ShowIf("IsDropACurrency")]
        [SerializeField] CurrencyType dropCurrencyType;
        [ShowIf("HaveDrop")]
        [SerializeField] int dropItemsCount = 1;
        [ShowIf("HaveDrop")]
        [SerializeField] float dropItemValue = 1;

        public int Health
        {
            get => health; set
            {
                health = value;
                UpdateHealthText();
            }
        }

        protected bool IsDropACurrency => dropType == DropableItemType.Money;
        protected bool HaveDrop => dropType != DropableItemType.None;

        public float BoosterHeight { get => boosterHeight; set => boosterHeight = value; }
        
        public DropableItemType DropType { get => dropType; set => dropType = value; }
        public CurrencyType DropCurrencyType { get => dropCurrencyType; set => dropCurrencyType = value; }
        public int DropItemsCount { get => dropItemsCount; set => dropItemsCount = value; }
        public float DropItemValue { get => dropItemValue; set => dropItemValue = value; }

        public void UpdateHealthText()
        {
            ObstacleBehavior obstacleBehavior = GetComponent<ObstacleBehavior>();

            if (obstacleBehavior == null)
            {
                Debug.LogError("ObstacleBehavior not found");
                return;
            }

            System.Type type = obstacleBehavior.GetType();
            System.Reflection.FieldInfo field = type.GetField("healthText",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if(field == null)
            {
                Debug.LogError("Field is null");
                return;
            }

            TMP_Text healthText = field.GetValue(obstacleBehavior) as TMP_Text;
            healthText.text = Health.ToString();

        }
    }
}
