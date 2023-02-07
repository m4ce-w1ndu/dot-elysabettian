using Compiler;
using Compiler.Common;
using VirtualMachine.Common;

namespace VirtualMachine
{
    public class ExecutionEngine
    {
        private List<Value> stack;

        private List<CallFrame> frames;

        private Dictionary<string, Value> globals;

        private Upvalue? openUpvalues;

        public ExecutionEngine()
        {
            stack = new();
            frames = new();
            globals = new();
            openUpvalues = null;
            stack.Capacity = Constants.STACK_MAX;
        }

        private void ResetStack()
        {
            stack.Clear();
            frames.Clear();
            stack.Capacity = Constants.STACK_MAX;
            openUpvalues = null;
        }


    }
}
