using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Common.Types
{
    /// <summary>
    /// Holds information to represent an upvalue.
    /// </summary>
    public class Upvalue
    {
        /// <summary>
        /// Location of the value.
        /// </summary>
        public Value Location { get; set; }

        /// <summary>
        /// Closed upvalue.
        /// </summary>
        public Value Closed { get; set; }

        /// <summary>
        /// Next upvalue in list.
        /// </summary>
        /// <value>May be null.</value>
        public Upvalue? Next { get; set; }

        /// <summary>
        /// Constructs a new Upvalue
        /// </summary>
        /// <param name="slot">Location of the upvalue.</param>
        public Upvalue(Value slot)
        {
            Location = slot;
            Closed = new Value(null);
            Next = null;
        }
    }
}