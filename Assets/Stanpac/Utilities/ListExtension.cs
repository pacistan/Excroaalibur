using System.Collections.Generic;
using UnityEngine;

namespace Stanpac.Utilities
{
    public static class ListExtension
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
            }
        }
    }
    
}


