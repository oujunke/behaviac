using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_9
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public Task SayHello()
        {
            Console.WriteLine();
            Console.WriteLine("Hello Behaviac!");
            Console.WriteLine();
            return Task.CompletedTask;
        }
    }
}
