namespace Compiler.Common
{
    /// <summary>
    /// Represents an instruction in the VM's CPU
    /// </summary>
    public enum Instruction : byte
    {
        Const,
        Nop,
        True,
        False,
        Pop,
        GetLoc,
        GetGlob,
        DefGlob,
        SetLoc,
        SetGlob,
        GetUpv,
        SetUpv,
        GetProp,
        SetProp,
        GetSup,
        Eq,
        Gt,
        Lt,
        Add,
        Sub,
        Mul,
        Div,
        Not,
        Neg,
        Pnt,
        Jmp,
        JmpZ,
        Branch,
        Call,
        Invoke,
        SupInvoke,
        Closure,
        CloseUpv,
        Ret,
        Class,
        Inherit,
        Method,
        BwAnd,
        BwOr,
        BwXor,
        BwNot,
        ArrBuild,
        ArrIdx,
        ArrStore
    }

    /// <summary>
    /// Extension methods for the instruction enumerator
    /// </summary>
    public static class InstructionExtensions
    {
        /// <summary>
        /// Returns the corresponding byte value of a given
        /// instruction.
        /// </summary>
        /// <param name="instr">Instruction value.</param>
        /// <returns>Converted byte value of a given instruction.</returns>
        public static byte GetByte(this Instruction instr)
        {
            return (byte)instr;
        }

        /// <summary>
        /// Returns the corresponding instruction based on the given value.
        /// </summary>
        /// <param name="value">Byte instruction value.</param>
        /// <returns>Instruction enumeration value.</returns>
        public static Instruction GetInstruction(this byte value)
        {
            return (Instruction)value;
        }
    }
}