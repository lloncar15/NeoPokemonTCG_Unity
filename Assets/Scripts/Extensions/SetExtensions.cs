using System;
using System.Collections.Generic;

namespace GimGim.Extensions {
    public static class SetExtensions {
        
        /// <summary>
        /// Sort the elements of a HashSet in ascending order and returns an ordered list.
        /// </summary>
        /// <typeparam name="T">Type of elements in the List.</typeparam>
        /// <param name="set">Hashset that is sorted.</param>
        /// <returns>Sorted list</returns>
        public static List<T> ToSortedList<T>(this HashSet<T> set) {
            T[] array = new T[set.Count];
            set.CopyTo(array);
            
            List<T> list = new List<T>(array);
            list.Sort();
            return list;
        }
        
        /// <summary>
        /// Sort the elements of a HashSet according to a comparison and returns an ordered list.
        /// </summary>
        /// /// <typeparam name="T">Type of elements in the List.</typeparam>
        /// <param name="set">Hashset that is sorted.</param>
        /// <param name="comparer">The method that T implements to compare two objects.</param>
        /// <returns>Sorted list</returns>
        public static List<T> ToSortedList<T> (this HashSet<T> set, IComparer<T> comparer)
        {
            T[] array = new T[set.Count];
            set.CopyTo(array);

            List<T> list = new List<T>(array);
            list.Sort(comparer);
            return list;
        }

        /// <summary>
        /// Sort the elements of a HashSet according to a comparison and returns an ordered list.
        /// </summary>
        /// <typeparam name="T">Type of elements in the List.</typeparam>
        /// <param name="set">Hashset that is sorted.</param>
        /// <param name="comparison">The comparison method to compare two objects.</param>
        /// <returns>Sorted list</returns>
        public static List<T> ToSortedList<T> (this HashSet<T> set, Comparison<T> comparison)
        {
            T[] array = new T[set.Count];
            set.CopyTo(array);

            List<T> list = new List<T>(array);
            list.Sort(comparison);
            return list;
        }

        /// <summary>
        /// Sort the elements of a HashSet in a range according to a comparator and returns an ordered list.
        /// </summary>
        /// <typeparam name="T">Type of elements in the List.</typeparam>
        /// <param name="set">Hashset that is sorted.</param>
        /// <param name="index">The starting index of the range.</param>
        /// <param name="count">Number of elements to sort.</param>
        /// <param name="comparer">The method that T implements to compare two objects.</param>
        /// <returns>Sorted list for a range of elements.</returns>
        public static List<T> ToSortedList<T> (this HashSet<T> set, int index, int count, IComparer<T> comparer)
        {
            T[] array = new T[set.Count];
            set.CopyTo(array);

            List<T> list = new List<T>(array);
            list.Sort(index, count, comparer);
            return list;
        }
    }
}