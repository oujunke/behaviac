﻿// -------------------------------------------------------------------------------
// THIS FILE IS ORIGINALLY GENERATED BY THE DESIGNER.
// YOU ARE ONLY ALLOWED TO MODIFY CODE BETWEEN '///<<< BEGIN' AND '///<<< END'.
// PLEASE MODIFY AND REGENERETE IT IN THE DESIGNER FOR CLASS/MEMBERS/METHODS, ETC.
// -------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

///<<< BEGIN WRITING YOUR CODE FILE_INIT

///<<< END WRITING YOUR CODE

public class FirstAgent : behaviac.Agent
///<<< BEGIN WRITING YOUR CODE FirstAgent
///<<< END WRITING YOUR CODE
{
	private IFirstAgentImp _methodImp;
	public FirstAgent(IFirstAgentImp methodImp,behaviac.Workspace workspace):base(workspace)
	{
	    _methodImp=methodImp;
	}
	private int p1 = 0;
	public void _set_p1(int value)
	{
		p1 = value;
	}
	public int _get_p1()
	{
		return p1;
	}

	public async Task SayHello()
	{
		 await _methodImp.SayHello();
	}

///<<< BEGIN WRITING YOUR CODE CLASS_PART

///<<< END WRITING YOUR CODE

}

///<<< BEGIN WRITING YOUR CODE FILE_UNINIT

///<<< END WRITING YOUR CODE

