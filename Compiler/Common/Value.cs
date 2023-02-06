using Compiler.Common.Types;

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

        public static implicit operator Value(Function function)
        {
            return new Value(function);
        }
    }

    /// <summary>
    /// Visits a value producing its corresponding output.
    /// </summary>
    public static class OutputVisitor
    {
        public static void Visit(double d) => Console.Write(d);

        public static void Visit(bool b) => Console.Write(b ? "true" : "false");

        public static void Visit(Type _) => Console.Write("null");

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