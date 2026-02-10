using System;
using UnityEngine;

namespace Stanpac.Utilities
{ 
    /**
     * Simple implementation of a weighted selection system. It allows you to add choices with associated weights and then randomly select a choice based on those weights.
     */
    public class WeightedSelection<T>
    {
        /** Use Array for Better Performance */
        [SerializeField]
        public WeightItem[] ItemsChoices;
        
        private int _count;
        private float _totalWeight;
        
        /** Force a minimum capacity
         * - Avoid too small Selection
         */
        private const int _minCapacity = 8;
        
        public int Count
        {
            get => _count;
            private set => _count = value;
        }
        
        /** Get The */
        public int Capacity
        {
            get => ItemsChoices.Length;
            set {
                if (value < _minCapacity || value < this.Count) 
                    throw new ArgumentOutOfRangeException(nameof(value));
                
                WeightItem[] choices1 = ItemsChoices;
                ItemsChoices = new WeightItem[value];
                WeightItem[] choices2 = ItemsChoices;
                int count = Count;
                Array.Copy(choices1,choices2, count);
            }
        }
        
        public WeightedSelection(int capacity = _minCapacity)
        {
            ItemsChoices = new WeightItem[capacity];
        }
        
        public WeightItem GetChoice(int i) => ItemsChoices[i];
        
        public void AddChoice(T item, float weight) => AddChoice(new WeightItem()
        {
            item = item,
            weight = weight
        });

        public void AddChoice(WeightItem ItemsChoice)
        { 
            if (Count == Capacity)  // Resize if we reach the capacity
                Capacity *= 2;
            
            ItemsChoices[Count++] = ItemsChoice;
            _totalWeight += ItemsChoice.weight;
        }

        /** Remove a choice by index and shift the remaining choices to fill the gap. */
        public void RemoveChoice(int choiceIndex)
        {
            int index1 = choiceIndex >= 0 && Count > choiceIndex ? choiceIndex : throw new ArgumentOutOfRangeException(nameof(choiceIndex));

            // Remove the choice by shifting the elements after it to the left
            for (int index2 = Count - 1; index1 < index2; index1++)
            {
                ItemsChoices[index1] = ItemsChoices[index1 + 1];
            }
            
            ItemsChoices[--Count] = new WeightItem();
            RecalculateTotalWeight();
        }

        public void Clear()
        {
            for (int index = 0; index < Count; index++)
            {
                ItemsChoices[index] = new WeightItem();
            }
             
            Count = 0;
            _totalWeight = 0.0f;
        }

        private void RecalculateTotalWeight()
        {
            _totalWeight = 0.0f;
            for (int index = 0; index < Count; ++index)
            {
                _totalWeight += ItemsChoices[index].weight;
            }
        }

        public T Select(float normalizedIndex) => ItemsChoices[SelectChoiceIndex(normalizedIndex)].item;

        public int SelectChoiceIndex(float normalizedIndex) => SelectChoiceIndex(normalizedIndex, (int[])null);

        public int SelectChoiceIndex(float normalizedIndex, int[] ignoreIndexes)
        {
            if (Count == 0)
                throw new InvalidOperationException("Cannot call Evaluate without available choices.");
            
            float totalWeight = _totalWeight;
            if (ignoreIndexes != null)
            {
                foreach (int ignoreIndex in ignoreIndexes)
                {
                    totalWeight -= ItemsChoices[ignoreIndex].weight;
                }
            }
            
            float num1 = normalizedIndex * totalWeight;
            float num2 = 0.0f;
            for (int IndexToChoose = 0; IndexToChoose < Count; IndexToChoose++)
            {
                if (ignoreIndexes == null || Array.IndexOf(ignoreIndexes, IndexToChoose) == -1)
                {
                    num2 += ItemsChoices[IndexToChoose].weight;
                    if (num1 < num2)
                        return IndexToChoose;
                }
            }
            return Count - 1;
        }

        [Serializable]
        public struct WeightItem
        {
            public T item;
          
            [Min(0)]
            public float weight;
        }
    }
} 

