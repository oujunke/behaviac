using behaviac;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public class SecondAgent : behaviac.Agent
{
    private int p1 = 0;

    public SecondAgent(Workspace workspace) : base(workspace)
    {
    }

    public void _set_p1(int value)
    {
        p1 = value;
    }
    public int _get_p1()
    {
        return p1;
    }

    public Task m1(string value)
    {
        Console.WriteLine();
        Console.WriteLine("{0}", value);
        Console.WriteLine();
        return Task.CompletedTask;
    }
}
