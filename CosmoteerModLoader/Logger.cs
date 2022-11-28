namespace CosmoteerModLoader;

public enum LogLevel
{
	Debug = 0,
	Info = 1,
	Warn = 2,
	Error = 3,
	Exception = 4,
}

public static class Logger<T>
{
	public static void Info(string message)
	{
		Logger.WriteLog(LogLevel.Info, typeof(T), message);
	}
	
	public static void Exception(Exception e)
	{
		Logger.WriteLog(LogLevel.Exception, typeof(T), $"{e.Message}\n{e.StackTrace}");
	}

	public static void Warn(string message)
	{
		Logger.WriteLog(LogLevel.Warn, typeof(T), message);
	}
	
	public static void Error(string message)
	{
		Logger.WriteLog(LogLevel.Error, typeof(T), message);
	}
	
	public static void Debug(string message)
	{
		Logger.WriteLog(LogLevel.Debug, typeof(T), message);
	}
}

public static class Logger
{
	public static LogLevel LogLevel = LogLevel.Info;

	internal static void WriteLog(LogLevel level, Type callerType, string message)
	{
		if (level < LogLevel)
			return;

		var msg = $"{level}:{callerType}: {message}";
		
		var foreGround = Console.ForegroundColor; 
		Console.ForegroundColor = ConsoleColor.Red;
		Console.WriteLine(msg);
		Console.ForegroundColor = foreGround;
	}
}