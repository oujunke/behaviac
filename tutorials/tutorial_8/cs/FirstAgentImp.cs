using behaviac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_8
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public FirstAgent FirstAgent;

        public Task<int> GetP1s1()
        {
           return Task.FromResult( FirstAgent._get_p1().s1);
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
