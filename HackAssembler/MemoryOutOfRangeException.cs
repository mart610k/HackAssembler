using System;
using System.Collections.Generic;
using System.Text;

namespace HackAssembler
{
    /// <summary>
    /// This exception gets called when the address is not within the address range of compiled assembly
    /// </summary>
    public class MemoryOutOfRangeException : Exception
    {
        public MemoryOutOfRangeException(string message) : base(message)
        {
        }
    }
}
