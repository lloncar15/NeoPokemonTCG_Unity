using System.Collections.Generic;
using UnityEngine;

namespace GimGim.ActionSystem {
    /// <summary>
    /// Sorts GameActions so that actions with lower OrderOfPlay are resolved first (FIFO, queue behavior).
    /// </summary>
    public class ActionSystemSorterFiFo : IActionSystemSorter {
        public int Compare(GameAction first, GameAction second) {
            Debug.Assert(first != null && second != null, "GameAction cannot be null");
            return first.Priority != second.Priority ? first.Priority.CompareTo(second.Priority) : first.OrderOfPlay.CompareTo(second.OrderOfPlay);
        }
    }
    
    /// <summary>
    /// Sorts GameActions so that actions with higher OrderOfPlay are resolved first (LIFO, stack behavior).
    /// </summary>
    public class ActionSystemSorterLiFo : IActionSystemSorter {
        public int Compare(GameAction first, GameAction second) {
            Debug.Assert(first != null && second != null, "GameAction cannot be null");
            return first.Priority != second.Priority ? first.Priority.CompareTo(second.Priority) : second.OrderOfPlay.CompareTo(first.OrderOfPlay);
        }
    }

    public interface IActionSystemSorter : IComparer<GameAction> { }
}