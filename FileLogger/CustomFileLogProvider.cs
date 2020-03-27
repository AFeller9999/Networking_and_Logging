using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

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
			logger.Close();
		}
	}
}
