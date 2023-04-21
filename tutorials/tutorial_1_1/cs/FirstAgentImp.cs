using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_1_1
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public Task m1(string value)
        {
            Console.WriteLine();
            Console.WriteLine("{0}", value);
            Console.WriteLine();
            return Task.CompletedTask;
        }

        public Task SayHello()
        {
            Console.WriteLine();
            Console.WriteLine("Hello Behaviac!");
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
