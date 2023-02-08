using Compiler.Common.Types;

namespace VirtualMachine
{
    /// <summary>
    /// Visits a set of types
    /// </summary>
    public static class TypeVisitor
    {
        /// <summary>
        /// Visits a double
        /// </summary>
        public static string Visit(double d) => $"{d:0:0.00000000}";

        /// <summary>
        /// Visits a string
        /// </summary>
        public static string Visit(string s) => s;

        /// <summary>
        /// Visits a monostate (null) value
        /// </summary>
        public static string Visit(Monostate _) => "null";

        /// <summary>
        /// Visits a function
        /// </summary>
        public static string Visit(Function f)
        {
            if (string.IsNullOrEmpty(f.Name)) return "<main>";
            return $"<func {f.Name}>";
        }

        /// <summary>
        /// Visits a native function
        /// </summary>
        public static string Visit(NativeFunction _) => "<native func>";

        /// <summary>
        /// Visits a closure
        /// </summary>
        public static string Visit(Closure _) => "<closure>";

        /// <summary>
        /// Visits an upvalue
        /// </summary>
        public static string Visit(Upvalue _) => "<upvalue>";

        /// <summary>
        /// Visits a class
        /// </summary>
        public static string Visit(Class c) => c.Name;

        /// <summary>
        /// Visits an instance
        /// </summary>
        public static string Visit(Instance i) => $"{i.Class.Name} instance";

        /// <summary>
        /// Visits a method
        /// </summary>
        public static string Visit(Method m) => $"{m.Function.Function.Name}";

        /// <summary>
        /// Visits a file
        /// </summary>
        public static string Visit(Compiler.Common.Types.File f) => f.Path;

        /// <summary>
        /// Visits an array
        /// </summary>
        public static string Visit(Compiler.Common.Types.Array a)
        {
            return $"<array[{a.Values.Count}]";
        }
    }
}
