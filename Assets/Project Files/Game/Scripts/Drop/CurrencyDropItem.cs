﻿using UnityEngine;

namespace Watermelon
{
    public class CurrencyDropItem : IDropItem
    {
        public DropableItemType DropItemType => DropableItemType.Money;

        private Currency[] availableCurrencies;

        public GameObject GetDropObject(DropData dropData)
        {
            CurrencyType currencyType = dropData.currencyType;
            for(int i = 0; i < availableCurrencies.Length; i++)
            {
                if(availableCurrencies[i].CurrencyType == currencyType)
                {
                    return availableCurrencies[i].Data.DropPool.GetPooledObject();
                }
            }

            return null;
        }

        public void SetCurrencies(Currency[] currencies)
        {
            availableCurrencies = currencies;
        }

        public void Initialise()
        {

        }

        public void Unload()
        {

        }
    }
}
