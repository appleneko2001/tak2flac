using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tak2flac
{
    public class Timeline
    {
        public Timeline(TimeSpan start, TimeSpan end)
        {
            Start = start;
            Duration = end - start;
        }
        public TimeSpan Start;
        public TimeSpan Duration;
    }
}
