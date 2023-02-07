using Compiler.Common;

namespace VirtualMachine.Common
{
    public static class Constants
    {
        /// <summary>
        /// Max number of stack frames
        /// </summary>
        public const int FRAMES_MAX = 64;

        /// <summary>
        /// Max size of stack
        /// </summary>
        public const int STACK_MAX = (FRAMES_MAX * Value.UINT8_COUNT);
    }
}
