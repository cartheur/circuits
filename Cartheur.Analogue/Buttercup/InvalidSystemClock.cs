using System;

namespace Buttercup
{
    public class InvalidSystemClock : Exception
    {      
        public InvalidSystemClock(string message) : base(message) { }
    }
}