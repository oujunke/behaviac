using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_3
{
    internal class FirstAgentImp : IFirstAgentImp, ISecondAgentImp
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
