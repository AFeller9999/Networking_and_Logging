using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.IO;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace FileLogger
{
	class CustomFileLogger : ILogger
	{

		StreamWriter writer;

		string categoryName;

		/// <summary>
		/// Creates a new logger of the specific log level, pointing to the given file, as well as whether or not the logger should 
		/// create a new file or if it should append to an already existing file.
		/// </summary>
		/// <param name="outputFile"></param>
		public CustomFileLogger(string categoryName, bool appendToEnd)
		{
			this.categoryName = categoryName;
			string fileName = "LOG_" + categoryName + ".txt";
			writer = new StreamWriter(fileName, appendToEnd);
		}

		/// <summary>
		/// This method begins a "session" or scope of logging, to be used with the "using" block.
		/// In this context, a "session" is similar to an instance of a method on the stack;
		/// all logging information is associated with this scope as long as it is undisposed.
		/// </summary>
		public IDisposable BeginScope<TState>(TState state)
		{
			throw new NotImplementedException();
		}

		public bool IsEnabled(LogLevel logLevel)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Writes a log entry to the file found or created upon construction. Each log entry is put into a new line automatically.
		/// </summary>
		/// <typeparam name="TState"></typeparam>
		/// <param name="logLevel"></param>
		/// <param name="eventId"></param>
		/// <param name="state"></param>
		/// <param name="exception"></param>
		/// <param name="formatter"></param>
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			StringBuilder sb = new StringBuilder();
			switch (logLevel)
			{
				case LogLevel.Information:
					sb.Append("- Infor - ".ShowTimeAndThread());
					break;
				case LogLevel.Error:
					sb.Append("- Error - ".ShowTimeAndThread());
					break;
				case LogLevel.Warning:
					sb.Append("- Warni - ".ShowTimeAndThread());
					break;
				case LogLevel.Critical:
					sb.Append("- Criti - ".ShowTimeAndThread());
					break;
				case LogLevel.Debug:
					sb.Append("- Debug - ".ShowTimeAndThread());
					break;
				case LogLevel.Trace:
					sb.Append("- Trace - ".ShowTimeAndThread());
					break;
			}
			sb.Append(state.ToString());
			writer.WriteLine(sb.ToString());
			
			/// The following three lines are done in order to esnure that data is written to the file if the program is closed abnormally
			/// This is primarily meant for the ChatServer, as it will never end normally, and thus never log information otherwise.
			writer.Close();
			string fileName = "LOG_" + categoryName + ".txt";
			writer = new StreamWriter(fileName, true);
		}

		/// <summary>
		/// Closes this logger. It is understood that the logger is then inaccessable after doing this.
		/// </summary>
		public void Close()
		{
			writer.Close();
		}
	}
}
