using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace API.Model
{
    public class Deadline
    {
        public int Day { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }

        public override string ToString()
        {
            return $"{Day:D2}/{Month:D2}/{Year} {Hour:D2}:{Minute:D2}";
        }
    }
}