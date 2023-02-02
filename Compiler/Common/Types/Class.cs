using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Compiler.Common.Types
{
    public struct Class
    {
        public string Name { get; init; }

        public Dictionary<string, Closure> Methods { get; init; }
    }
}