namespace Compiler.Utilities
{
    public static class ListExtensions
    {
        /// <summary>
        /// Resizes the list.
        /// </summary>
        /// <typeparam name="T">Type of the object. Usually inferred.</typeparam>
        /// <param name="list">List to resize.</param>
        /// <param name="size">New size.</param>
        /// <param name="element">Value of the elements to fill.</param>
        public static void Resize<T>(this List<T> list, int size, T element)
        {
            int curr = list.Count;
            if (size < curr)
            {
                list.RemoveRange(curr, size);
                return;
            }

            if (size > curr) list.Capacity = size;
            list.AddRange(Enumerable.Repeat(element, size - curr));
        }

        /// <summary>
        /// Resizes the list.
        /// </summary>
        /// <typeparam name="T">Type of the object. Usually inferred.</typeparam>
        /// <param name="list">List to resize.</param>
        /// <param name="size">New size.</param>
        public static void Resize<T>(this List<T> list, int size) where T : new()
        {
            Resize(list, size, new T());
        }
    }
}
