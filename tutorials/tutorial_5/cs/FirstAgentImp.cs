using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_5
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public Task Say(string value)
        {
            Console.WriteLine();
            Console.WriteLine("{0}", value);
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
