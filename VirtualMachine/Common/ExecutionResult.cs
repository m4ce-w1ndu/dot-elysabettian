namespace VirtualMachine.Common
{
    /// <summary>
    /// Represents the possible states of execution
    /// </summary>
    public enum ExecutionResult
    {
        /// <summary>
        /// Signals an OK execution
        /// </summary>
        Ok,
        /// <summary>
        /// Signals an error in compilation phase
        /// </summary>
        CompileError,
        /// <summary>
        /// Signals an error at runtime
        /// </summary>
        RuntimeError
    }
}
