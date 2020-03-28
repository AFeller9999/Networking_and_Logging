using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;


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
///    This file contains a CustomFileLogProvider which acts as a ILoggerProvider for the CustomFileLogger class. It contains all required methods
namespace FileLogger
{
	public class CustomFileLogProvider : ILoggerProvider
	{

		CustomFileLogger logger;
		public ILogger CreateLogger(string categoryName, bool appendToEnd)
		{
			this.logger = new CustomFileLogger(categoryName, appendToEnd);
			return logger;
		}

		public ILogger CreateLogger(string categoryName)
		{
			return this.CreateLogger(categoryName, false);
		}

		public void Dispose()
		{
			logger?.Close();
			logger = null;
		}
	}
}
