namespace Compiler.Common.Types
{
    /// <summary>
    /// Holds a chunk of memory related with a function or specific
    /// block of code.
    /// </summary>
    public class Chunk
    {
        /// <summary>
        /// Instruction and small data memory
        /// </summary>
        private List<byte> code;

        /// <summary>
        /// Data memory
        /// </summary>
        private List<Value> constants;

        /// <summary>
        /// Lines of code
        /// </summary>
        private List<int> lines;

        /// <summary>
        /// Constructs a new, empty chunk
        /// </summary>
        public Chunk()
        {
            code = new();
            constants = new();
            lines = new();
        }

        /// <summary>
        /// Returns a byte of code from the instruction memory.
        /// </summary>
        /// <param name="offset">Offset of the byte.</param>
        /// <returns>Byte of code.</returns>
        public byte GetCode(int offset) => code[offset];

        /// <summary>
        /// Sets the value of a byte of code in the instruction memory.
        /// </summary>
        /// <param name="offset">Offset of the byte.</param>
        /// <param name="value">Value of the byte to write.</param>
        public void SetCode(int offset, byte value)
        {
            code[offset] = value;
        }

        /// <summary>
        /// Returns a new constant (value) from the data memory.
        /// </summary>
        /// <param name="constant">Offset of the constant.</param>
        /// <returns>Value of the constant at specified offset.</returns>
        public Value GetConstant(int constant) => constants[constant];

        /// <summary>
        /// Writes a new byte to the instruction memory.
        /// </summary>
        /// <param name="byte_">Byte to write.</param>
        /// <param name="line">Line bound with this byte.</param>
        public void Write(byte byte_, int line)
        {
            code.Add(byte_);
            lines.Add(line);
        }

        /// <summary>
        /// Writes a new instruction to the instruction memory.
        /// </summary>
        /// <param name="instr">Instruction to write</param>
        /// <param name="line">Line bound with this instruction.</param>
        public void Write(Instruction instr, int line)
        {
            Write(instr.GetByte(), line);
        }

        /// <summary>
        /// Adds a new constant to the data memory.
        /// </summary>
        /// <param name="value">Value to add.</param>
        /// <returns>The offset of the added constant.</returns>
        public int AddConstant(Value value)
        {
            constants.Add(value);
            return constants.Count - 1;
        }

        /// <summary>
        /// Returns the line bound with the given instruction.
        /// </summary>
        /// <param name="instruction">Instruction to check.</param>
        /// <returns>Line bound with the instruction.</returns>
        public int GetLine(int instruction) => lines[instruction];

        /// <summary>
        /// Current size of the instruction memory.
        /// </summary>
        public int Count { get => code.Count; }
    }
}