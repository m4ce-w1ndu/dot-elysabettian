using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Common
{
    /// <summary>
    /// This type can hold a series of values of predefined
    /// types.
    /// </summary>
    public class Value
    {
        private object? value;

        /// <summary>
        /// Constructs a new value type.
        /// </summary>
        /// <param name="value">Value to assign.</param>
        public Value(object? value)
        {
            this.value = value;
        }

        /// <summary>
        /// Returns a value of the given type.
        /// </summary>
        /// <typeparam name="T">Type of the value to get.</typeparam>
        /// <returns></returns>
        public T? Get<T>()
        {
            return (T?)value;
        }

        /// <summary>
        /// Implicitly converts a double to a new Value holder.
        /// </summary>
        public static implicit operator Value(double number)
        {
            return new Value(number);
        }

        public static implicit operator Value(bool boolean)
        {
            return new Value(boolean);
        }

        public static implicit operator Value(string str)
        {
            return new Value(str);
        }
    }
}