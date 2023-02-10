using Compiler.Common.Types;
using System.Text;
using Array = Compiler.Common.Types.Array;
using File = Compiler.Common.Types.File;

namespace Compiler.Common
{
    /// <summary>
    /// This type can hold a series of values of predefined
    /// types.
    /// </summary>
    public class Value
    {
        /// <summary>
        /// Max number of variables in functions.
        /// </summary>
        public const int UINT8_COUNT = byte.MaxValue + 1;

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
        /// Constructs an empty value
        /// </summary>
        public Value()
        {
            value = new Monostate();
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

        public static implicit operator Value(string? str)
        {
            return new Value(str);
        }

        public static implicit operator Value(Function? function)
        {
            return new Value(function);
        }

        public static implicit operator Value(Monostate? _)
        {
            return new Value(null);
        }

        public static implicit operator Value(Method? method)
        {
            return new Value(method);
        }

        public static implicit operator Value(Closure? closure)
        {
            return new Value(closure);
        }

        public static implicit operator Value(NativeFunction? native)
        {
            return new Value(native);
        }

        public static implicit operator Value(Types.Array? array)
        {
            return new Value(array);
        }

        public static implicit operator Value(Class? classValue)
        {
            return new Value(classValue);
        }

        public override string ToString()
        {
            if (value is double d)
            {
                return d.ToString();
            }

            if (value is bool b)
            {
                return b.ToString().ToLower();
            }

            if (value is Monostate)
                return "null";

            if (value is string s)
                return s;

            if (value is Function f)
            {
                if (string.IsNullOrEmpty(f.Name)) return "<main>";
                return $"<fn {f.Name}>";
            }

            if (value is NativeFunction nf)
                return "<native fn>";

            if (value is Closure c)
            {
                if (string.IsNullOrEmpty(c.Function.Name)) return "<main>";
                return $"<fn {c.Function.Name}>";
            }

            if (value is Upvalue)
                return "upvalue";

            if (value is Class cl)
                return cl.Name;

            if (value is Instance i)
                return $"{i.Class.Name} instance";

            if (value is Method m)
            {
                if (string.IsNullOrEmpty(m.Function.Function.Name)) return "<main>";
                return $"<fn {m.Function.Function.Name}>";
            }

            if (value is File fi)
                return $"path: {fi.Path}";

            if (value is Array ar)
            {
                var sb = new StringBuilder("array {");
                for (int idx = 0; idx < ar.Values.Count; ++idx)
                {
                    if (idx < ar.Values.Count - 1) sb.Append($"{ar.Values[idx]}, ");
                    else sb.Append(ar.Values[idx]);
                }
                sb.Append(" }");

                return sb.ToString();
            }

            return "";
        }
    }

    /// <summary>
    /// Visits a value producing its corresponding output.
    /// </summary>
    public static class OutputVisitor
    {
        public static void Visit(double d) => Console.Write(d);

        public static void Visit(bool b) => Console.Write(b ? "true" : "false");

        public static void Visit(Monostate _) => Console.Write("null");

        public static void Visit(string s) => Console.Write(s);

        public static void Visit(Function f)
        {
            if (string.IsNullOrEmpty(f.Name)) Console.Write("<main>");
            else Console.Write($"<fn {f.Name}>");
        }

        public static void Visit(NativeFunction _) => Console.Write("<native fn>");

        public static void Visit(Closure c) => Visit(c.Function);

        public static void Visit(Upvalue _) => Console.Write("upvalue");

        public static void Visit(Class c) => Console.Write(c.Name);

        public static void Visit(Instance i) => Console.Write($"{i.Class.Name} instance");

        public static void Visit(Method m) => Visit(m.Function.Function);

        public static void Visit(Types.File f) => Console.Write($"path: {f.Path}");

        public static void Visit(Types.Array a)
        {
            Console.Write("array {");
            for (int i = 0; i < a.Values.Count; ++i)
            {
                if (i < a.Values.Count - 1) Console.Write($"{a.Values[i]}, ");
                else Console.Write(a.Values[i]);
            }
            Console.Write(" }");
        }
    }

    /// <summary>
    /// Returns a boolean value based on the type given.
    /// </summary>
    public static class FalseVisitor
    {
        public static bool Visit(bool b) { return !b; }

        public static bool Visit(System.Type _) => true;

        public static bool Visit<T>(T _) => false;
    }
}