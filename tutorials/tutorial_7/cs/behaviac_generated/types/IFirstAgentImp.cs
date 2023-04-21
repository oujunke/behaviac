using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
public interface IFirstAgentImp
{
	Task Start();
	Task<behaviac.EBTStatus> Wait();
}
