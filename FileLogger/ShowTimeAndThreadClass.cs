using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

/// <summary> 
/// Author:    Allan Feller
/// Partner:   None 
/// Date:      3/29/2020 
/// Course:    CS 3500, University of Utah, School of Computing 
/// Copyright: CS 3500 and Allan Feller - This work may not be copied for use in Academic Coursework. 
/// 
/// I, Allan Feller, certify that I wrote this code from scratch and did not copy it in part or whole from  
/// another source.  All references used in the completion of the assignment are cited in my README file. 
/// 
/// File Contents 
/// 
///    This file contains a static class ShowTimeAndThreadClass that has a string extension that appends to the beginning of the string the current time and date as well as the current thread.
namespace FileLogger
{
	internal static class ShowTimeAndThreadClass
	{

		public static string ShowTimeAndThread(this string s)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(DateTime.Now);
			sb.Append(" (");
			sb.Append(Thread.CurrentThread.ManagedThreadId);
			sb.Append(") ");
			return sb.ToString() + s;
		}
	}
}
