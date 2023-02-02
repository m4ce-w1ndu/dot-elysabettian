using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Common
{
    public class Value
    {
        private object? value;

        public Value(object? value)
        {
            this.value = value;
        }

        public T? Get<T>()
        {
            return (T?)value;
        }

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