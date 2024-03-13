using System;

namespace CartheurCircuit.Handling
{
    public class InvalidSystemClock : Exception
    {      
        public InvalidSystemClock(string message) : base(message) { }
    }
}
