using Compiler.Common.Types;

namespace VirtualMachine.Common
{
    /// <summary>
    /// Holds information of a function call
    /// stack frame
    /// </summary>
    public class CallFrame
    {
        /// <summary>
        /// Function's closure
        /// </summary>
        public Closure Closure { get; set; }

        /// <summary>
        /// Instruction pointer value
        /// </summary>
        public int InstructionPtr { get; set; }

        /// <summary>
        /// Stack pointer value
        /// </summary>
        public int StackPtr { get; set; }

        /// <summary>
        /// Constructs a new CallFrame
        /// </summary>
        /// <param name="closure">Function's closure</param>
        /// <param name="instructionPtr">Instruction pointer value</param>
        /// <param name="stackPtr">Stack pointer value</param>
        public CallFrame(Closure closure, int instructionPtr, int stackPtr)
        {
            Closure = closure;
            InstructionPtr = instructionPtr;
            StackPtr = stackPtr;
        }
    }
}
