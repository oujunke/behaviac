using behaviac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tutorial_7
{
    internal class FirstAgentImp : IFirstAgentImp
    {
        public int Count;
        public FirstAgent FirstAgent;
        public Task Start()
        {
            Count = 0;
            return Task.CompletedTask;
        }

        public Task<EBTStatus> Wait()
        {
            Count++;

            Console.WriteLine("p1 = {0}", FirstAgent._get_p1());

            if (Count == 10000)
            {
                return Task.FromResult(behaviac.EBTStatus.BT_SUCCESS);
            }

            return Task.FromResult(behaviac.EBTStatus.BT_RUNNING);
        }
    }
}
