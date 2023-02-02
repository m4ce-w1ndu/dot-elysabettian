namespace Compiler.Common.Types
{
    /// <summary>
    /// Holds data representing a function.
    /// </summary>
    public struct Function
    {
        /// <summary>
        /// Number of function parameters.
        /// </summary>
        public int Arity { get; set; }

        /// <summary>
        /// Number of upvalue in function.
        /// </summary>
        public int UpvalueCount { get; set; }

        /// <summary>
        /// Name of the function.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Memory chunk associated with the function.
        /// </summary>
        public Chunk Chunk { get; set; }

        /// <summary>
        /// Constructs a new function.
        /// </summary>
        /// <param name="arity">Number of function parameters.</param>
        /// <param name="name">Name of the function.</param>
        public Function(int arity, string name)
        {
            Arity = arity;
            UpvalueCount = 0;
            Name = name;
            Chunk = new Chunk();
        }

        public static bool operator ==(Function left, Function right) => false;

        public static bool operator !=(Function left, Function right) => true;

        /// <summary>
        /// Returns a byte of code from the instruction memory
        /// of the chunk related with this function.
        /// </summary>
        /// <param name="offset">Offset of the byte to retrieve.</param>
        /// <returns>Byte at provided offset.</returns>
        public byte GetCode(int offset)
        {
            return Chunk.GetCode(offset);
        }

        /// <summary>
        /// Returns a value from the data memory of the
        /// chunk related with this function.
        /// </summary>
        /// <param name="offset">Offset of the value to retrieve.</param>
        /// <returns>Value at provided offset.</returns>
        public Value GetConst(int offset)
        {
            return Chunk.GetConstant(offset);
        }

        public override bool Equals(object? obj)
        {
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
