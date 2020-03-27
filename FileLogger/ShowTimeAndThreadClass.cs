using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

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
