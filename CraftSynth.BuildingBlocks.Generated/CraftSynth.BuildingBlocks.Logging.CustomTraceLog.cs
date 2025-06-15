//V2a
using System.Runtime.InteropServices;
using System.Text;

namespace CraftSynth.BuildingBlocks.Logging;

/// <summary>
/// Sole purpose of this class is to help to build string that represents processing steps.
/// String can be included in ex message.
/// 
/// Result example:
/// Started login process...
///   Checking pass...
///     Success..
///   Checing roles..
///     Sucess..
/// Login done.
/// </summary>
public class CustomTraceLog
{
	private StringBuilder _sb;
	private DateTime? _timestamp;
	private bool _prependTimestamps;
	private bool _useUtcForTimestamps;
	private int _ident;
	private string _oneIdent;
	private SensitiveStringsList _sensitiveStringsList = new SensitiveStringsList();
	private DateTime _lastChangeAsUtc = DateTime.UtcNow;
	public object _lock = new object();
	public const string DEFAULT_LINE_NUMBER_FORMAT = "{0}. ";

	private string CalculateIdent(int ident)
	{
		string result = string.Empty;
		for(int i = 0; i < ident; i++)
		{
			result += _oneIdent;
		}
		return result;
	}

	public delegate void AddLinePostProcessingEventDelegate(CustomTraceLog sender, string line, bool inNewLine, int level, string lineVersionSuitableForLineEnding, string lineVersionSuitableForNewLine);
	private event AddLinePostProcessingEventDelegate _AddLinePostProcessingEvent;

	public delegate void AddLinePreProcessingEventDelegate(CustomTraceLog sender, ref string line, ref bool inNewLine, ref int level);
	private event AddLinePreProcessingEventDelegate _AddLinePreProcessingEvent;

	public delegate DateTime DateTimeNowReplacementFunctionDelegate(bool shouldReturnInUtc);
	private event DateTimeNowReplacementFunctionDelegate _dateTimeNowReplacementFunction;

	public object Tag { get; set; }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="initialValue"></param>
	/// <param name="prependTimestamps"> </param>
	/// <param name="useUtcForTimestamps"> </param>
	/// <param name="customTraceLog_AddLinePostProcessingEvent">
	/// You and set function to be called automatically after each AddLine call. Function format is:
	/// void CustomTraceLog_AddLinePostProcessingEvent(string line){}
	/// </param>
	/// <param name="customTraceLog_AddLinePreProcessingEvent">
	/// You can set function to be called automatically before each AddLine call. Use it to detect and hide passwords for example. Function format is:
	/// void CustomTraceLog_AddLinePostProcessingEvent(string line){}
	/// </param>
	public CustomTraceLog(string initialValue = null, bool prependTimestamps = true, bool useUtcForTimestamps = false, AddLinePostProcessingEventDelegate customTraceLog_AddLinePostProcessingEvent = null, AddLinePreProcessingEventDelegate customTraceLog_AddLinePreProcessingEvent = null, object tag = null, string oneIdent = "  ", DateTimeNowReplacementFunctionDelegate dateTimeNowReplacementFunction = null)
	{
		_sb = new StringBuilder(initialValue ?? string.Empty);
		_prependTimestamps = prependTimestamps;
		_useUtcForTimestamps = useUtcForTimestamps;
		_ident = 0;
		if(customTraceLog_AddLinePreProcessingEvent != null)
		{
			_AddLinePreProcessingEvent += customTraceLog_AddLinePreProcessingEvent;
		}
		if(customTraceLog_AddLinePostProcessingEvent != null)
		{
			_AddLinePostProcessingEvent += customTraceLog_AddLinePostProcessingEvent;
		}
		this.Tag = tag;
		_oneIdent = oneIdent;
		_dateTimeNowReplacementFunction = dateTimeNowReplacementFunction;
		//HandlerForLogFile.Append(this.ToString().TrimEnd('\n').TrimEnd('\r').TrimEnd('n')+"\r\n");
		_AddLine(initialValue);
	}

	public static CustomTraceLog Unbox(object customTraceLog)
	{
		CustomTraceLog log = null;
		if(customTraceLog != null)
		{
			if(!(customTraceLog is CustomTraceLog))
			{
				throw new Exception("customTraceLog param must be of type CustomTraceLog.");
			}
			else
			{
				log = (CustomTraceLog)customTraceLog;
			}
		}
		return log;
	}

	internal void _BindAddLinePostProcessingEventDelegate(AddLinePostProcessingEventDelegate customTraceLog_AddLinePostProcessingEvent)
	{
		lock(_lock)
		{
			if(customTraceLog_AddLinePostProcessingEvent != null)
			{
				_AddLinePostProcessingEvent += customTraceLog_AddLinePostProcessingEvent;
			}
		}
	}

	internal void _UnbindAddLinePostProcessingEventDelegate(AddLinePostProcessingEventDelegate customTraceLog_AddLinePostProcessingEvent)
	{
		lock(_lock)
		{
			if(customTraceLog_AddLinePostProcessingEvent != null)
			{
				_AddLinePostProcessingEvent -= customTraceLog_AddLinePostProcessingEvent;
			}
		}
	}

	internal void _IncreaseIdent()
	{
		lock(_lock)
		{
			_ident++;
		}
	}

	internal void _DecreaseIdent()
	{
		lock(_lock)
		{
			if(_ident > 0)
			{
				_ident--;
			}
		}
	}

	public void _AssignSensitiveString(string sensitiveString, int replacementType = (int)SensitiveStringReplacementType.Replace50PercentInMiddle, string replacementString = "...hidden...")
	{
		lock(_lock)
		{
			var ss = new SensitiveString(sensitiveString, replacementType, replacementString);
			_sensitiveStringsList.AddOrReplace(ss);
		}
	}

	internal void _AddLine(string message, bool inNewLine = true, int level = 0)
	{
		string line;
		string lineVersionSuitableForLineEnding;
		string lineVersionSuitableForNewLine;

		if(_AddLinePreProcessingEvent != null)
		{
			_AddLinePreProcessingEvent.Invoke(this, ref message, ref inNewLine, ref level);
		}

		lock(_lock)
		{
			message = _sensitiveStringsList.ReplaceAllSensitiveStrings(message);


			string timestamp = string.Empty;
			if(_prependTimestamps)
			{
				if(_dateTimeNowReplacementFunction == null)
				{
					_timestamp = _useUtcForTimestamps ? DateTime.UtcNow : DateTime.Now;
				}
				else
				{
					_timestamp = _dateTimeNowReplacementFunction.Invoke(_useUtcForTimestamps);
				}

				timestamp = string.Format("{0}.{1}.{2} {3}:{4}:{5} ({6}) ",
							  _timestamp.Value.Year,
							  _timestamp.Value.Month.ToString().PadLeft(2, '0'),
							  _timestamp.Value.Day.ToString().PadLeft(2, '0'),
							  _timestamp.Value.Hour.ToString().PadLeft(2, '0'),
							  _timestamp.Value.Minute.ToString().PadLeft(2, '0'),
							  _timestamp.Value.Second.ToString().PadLeft(2, '0'),
							  _useUtcForTimestamps ? "UTC" : "local");
			}
			lineVersionSuitableForLineEnding = message;
			lineVersionSuitableForNewLine = timestamp + this.CalculateIdent(_ident + (inNewLine ? 0 : 1)) + message;
			line = inNewLine ? "\r\n" + timestamp + this.CalculateIdent(_ident) + message : message;

			_sb.Append(line);
		}

		if(_AddLinePostProcessingEvent != null)
		{
			_AddLinePostProcessingEvent.Invoke(this, line, inNewLine, level, lineVersionSuitableForLineEnding, lineVersionSuitableForNewLine);
		}

		_lastChangeAsUtc = DateTime.UtcNow;
	}

	internal void _AddLines(List<string> lines, bool prefixLineNumbers = false, string prefixFormat = DEFAULT_LINE_NUMBER_FORMAT, bool inNewLine = true, int level = 0)
	{
		string prefix = "";
		int digitsInMaxLineNumber = lines.Count.ToString().Length;
		int lineNUmber = 1;
		foreach(string line in lines)
		{
			if(prefixLineNumbers)
			{
				prefix = string.Format(prefixFormat, lineNUmber.ToString().PadLeft(digitsInMaxLineNumber));
			}
			_AddLine(prefix + line, inNewLine, level);
			lineNUmber++;
		}
	}

	internal void _AddLines(string lines, bool prefixLineNumbers = false, string prefixFormat = DEFAULT_LINE_NUMBER_FORMAT, bool inNewLine = true, int level = 0)
	{
		List<string> linesAsList = this.ToLines(lines, true, true, new List<string>());
		_AddLines(linesAsList, prefixLineNumbers, prefixFormat, inNewLine, level);
	}

	private List<string> ToLines(string s, bool keepZeroLengthLines, bool keepWhitespaceLines, List<string> nullCaseResult)
	{
		List<string> r;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else
		{
			StringSplitOptions sso = keepZeroLengthLines ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
			r = s.Split(new string[] { "\n\r", "\r\n", "\n", "\r" }, sso).ToList();
			if(r.Count > 0 && !keepWhitespaceLines)
			{
				for(int i = r.Count - 1; i >= 0; i--)
				{
					if(string.IsNullOrWhiteSpace(r[i]))
					{
						r.RemoveAt(i);
					}
				}
			}
		}

		return r;
	}

	public string GetVersionForNewLine;

	internal void _AddLineAndIncreaseIdent(string message, bool inNewLine = true, int level = 0)
	{
		lock(_lock)
		{
			_AddLine(message, inNewLine, level);
			_IncreaseIdent();
		}
	}

	internal void _AddLineAndDecreaseIdent(string message, bool inNewLine = true, int level = 0)
	{
		lock(_lock)
		{
			_AddLine(message, inNewLine, level);
			_DecreaseIdent();
		}
	}

	//public void ExtendLastLine(string message)
	//{
	//	_sb.Append(message);
	//}

	//public void AddLine(int ident,string message)
	//{
	//	_sb.AppendLine(this.CalculateIdent(ident) + message);
	//}

	private static Random _randomGenerator = new Random(DateTime.UtcNow.Millisecond);

	internal int _GenerateLogPointId()
	{
		lock(_lock)
		{
			int r = CustomTraceLog._randomGenerator.Next(1, int.MaxValue);
			return r;
		}
	}

	internal int _AddLogPointId(int? logPointId = null)
	{
		lock(_lock)
		{
			logPointId = logPointId ?? _GenerateLogPointId();
			_AddLine($"LogPointId=[{logPointId}]");
			return logPointId.Value;
		}
	}

	public override string ToString()
	{
		lock(_lock)
		{
			return _sb.ToString();
		}
	}

	public int Length
	{
		get
		{
			lock(_lock)
			{
				return _sb.Length;
			}
		}
	}

	public DateTime LastChangeAsUtc
	{
		get
		{
			lock(_lock)
			{
				return _lastChangeAsUtc;
			}
		}
	}

	public string[] ToLines()
	{
		lock(_lock)
		{
			return _sb.ToString().Split(new string[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
		}
	}
}

public static class CustomTraceLogExtensions
{
	public static void BindAddLinePostProcessingEventDelegate(this CustomTraceLog log, CustomTraceLog.AddLinePostProcessingEventDelegate customTraceLog_AddLinePreProcessingEvent)
	{
		if(log != null)
		{
			log._BindAddLinePostProcessingEventDelegate(customTraceLog_AddLinePreProcessingEvent);
		}
	}

	public static void BindAddLinePostProcessingEventDelegate(this CustomTraceLog log, int aa) //CustomTraceLog.AddLinePostProcessingEventDelegate customTraceLog_AddLinePreProcessingEvent)
	{
		//if (log != null)
		//{
		//    log._BindAddLinePostProcessingEventDelegate(customTraceLog_AddLinePreProcessingEvent);
		//}
	}

	public static void UnbindAddLinePostProcessingEventDelegate(this CustomTraceLog log, CustomTraceLog.AddLinePostProcessingEventDelegate customTraceLog_AddLinePreProcessingEvent)
	{
		if(log != null)
		{
			log._UnbindAddLinePostProcessingEventDelegate(customTraceLog_AddLinePreProcessingEvent);
		}
	}

	public static void IncreaseIdent(this CustomTraceLog log)
	{
		if(log != null)
		{
			log._IncreaseIdent();
		}
	}

	public static void DecreaseIdent(this CustomTraceLog log)
	{
		if(log != null)
		{
			log._DecreaseIdent();
		}
	}

	public static void AddLine(this CustomTraceLog log, string message, bool inNewLine = true, int level = 0)
	{
		if(log != null)
		{
			log._AddLine(message, inNewLine, level);
		}
	}

	public static void AddLines(this CustomTraceLog log, List<string> lines, bool prefixLineNumbers = false, string prefixFormat = CustomTraceLog.DEFAULT_LINE_NUMBER_FORMAT, bool inNewLine = true, int level = 0)
	{
		if(log != null)
		{
			log._AddLines(lines, prefixLineNumbers, prefixFormat, inNewLine, level);
		}
	}

	public static void AddLines(this CustomTraceLog log, string lines, bool prefixLineNumbers = false, string prefixFormat = CustomTraceLog.DEFAULT_LINE_NUMBER_FORMAT, bool inNewLine = true, int level = 0)
	{
		if(log != null)
		{
			log._AddLines(lines, prefixLineNumbers, prefixFormat, inNewLine, level);
		}
	}

	public static void AddLineAndIncreaseIdent(this CustomTraceLog log, string message, bool inNewLine = true, int level = 0)
	{
		if(log != null)
		{
			log._AddLineAndIncreaseIdent(message, inNewLine, level);
		}
	}

	public static void AddLineAndDecreaseIdent(this CustomTraceLog log, string message, bool inNewLine = true, int level = 0)
	{
		if(log != null)
		{
			log._AddLineAndDecreaseIdent(message, inNewLine, level);
		}
	}

	public static LogScope LogScope(this CustomTraceLog log, string startMessage, string finishMessage = "done.", int level = 0)
	{
		return new LogScope(log, startMessage, finishMessage, level);
	}

	/// <summary>
	/// Marks particular string (like password) as sensitive so that whenever it appears in log it appears masked. 
	/// </summary>
	/// <param name="log"></param>
	/// <param name="sensitiveString">Sensitive information like password.</param>
	/// <param name="replacementType">ReplaceWhole = 0,Replace50PercentInMiddle = 1,ReplaceWithLengthInformation = 2</param>
	/// <param name="replacementString">Replacement mask like XXXX.</param>
	public static void AssignSensitiveString(this CustomTraceLog log, string sensitiveString, int replacementType = (int)SensitiveStringReplacementType.Replace50PercentInMiddle, string replacementString = "...hidden...")
	{
		if(log != null)
		{
			log._AssignSensitiveString(sensitiveString, replacementType, replacementString);
		}
	}

	public static void AddException(this CustomTraceLog log, Exception e, string additionalMessage = null)
	{
		if(log != null)
		{
			log._AddLine("============================= Exception ===========================");
			if(additionalMessage != null)
			{
				log._AddLine(additionalMessage);
				log._AddLine("-------------------------------------------------------------------");
			}
			List<Exception> ee = BuildingBlocks.Common.Misc.GetInnerExceptions(e);
			foreach(Exception e1 in ee)
			{
				log._AddLine(e1.Message);
				if(e1.StackTrace != null)
				{
					log._AddLine(e1.StackTrace);
				}
				log._AddLine("....................................................................");
			}
			log._AddLine("===================================================================");
		}
	}

	public static int GenerateLogPointId(this CustomTraceLog log)
	{
		if(log != null)
		{
			return log._GenerateLogPointId();
		}
		return 0;
	}

	public static int AddLogPointId(this CustomTraceLog log, int? logPointId = null)
	{
		if(log != null)
		{
			return log._AddLogPointId(logPointId);
		}
		return 0;
	}
}


/// <summary>
/// Usage:
/// 
/// 
/// using(log.LogScope("Preparing items to process...", "done.", 1, true))       //AddLineAndIncreaseIdent called here
/// {
///     //do something...
/// }                                                                            //AddLineAndDecreaseIdent called here
/// 
/// 
/// or:
/// 
/// 
/// using(new LogScope(log, "Preparing items to process...", "done.", 1, true))  //AddLineAndIncreaseIdent called here
/// {
///     //do something...
/// }                                                                            //AddLineAndDecreaseIdent called here
/// </summary>
public class LogScope:IDisposable
{
	private CustomTraceLog _log;
	private readonly string _startMessage;
	private readonly string _finishMessage;
	private readonly int _level;
	private readonly int _totalLengthAfterStartMessage;

	public LogScope(CustomTraceLog log, string startMessage, string finishMessage = "done.", int level = 0)
	{
		if(log != null)
		{
			_log = log;
			_startMessage = startMessage;
			_finishMessage = finishMessage;
			_level = level;

			log.AddLineAndIncreaseIdent(_startMessage, true, _level); //AddLineAndIncreaseIdent called here

			_totalLengthAfterStartMessage = log.Length;
		}
		else
		{
			_disposed = true;
		}
	}

	/// <summary>
	/// Determines whether execution is in exception.
	/// Sources:
	/// http://stackoverflow.com/questions/1815492/how-to-determine-whether-a-net-exception-is-being-handled
	/// http://www.codewrecks.com/blog/index.php/2008/07/25/detecting-if-finally-block-is-executing-for-an-manhandled-exception/
	/// https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.marshal.getexceptionpointers%28VS.80%29.aspx
	/// </summary>
	/// <returns></returns>
	private bool IsInException()
	{
		return Marshal.GetExceptionPointers() != IntPtr.Zero || Marshal.GetExceptionCode() != 0;
	}

	// Public implementation of Dispose pattern callable by consumers. 
	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	// Flag: Has Dispose already been called? 
	bool _disposed = false;

	// Protected implementation of Dispose pattern. 
	protected virtual void Dispose(bool disposing)
	{
		if(!_disposed)
		{
			bool thereWereSomeNewLogsAfterStartMessage = _log.Length != _totalLengthAfterStartMessage && _log.ToString().Substring(_totalLengthAfterStartMessage).Contains('\n');
			if(!this.IsInException())
			{
				if(_finishMessage != null)
				{
					_log._AddLine(_finishMessage, thereWereSomeNewLogsAfterStartMessage, _level);
				}
			}
			else
			{
				_log._AddLine(_finishMessage.TrimEnd('.') + " with error.", thereWereSomeNewLogsAfterStartMessage, _level);
			}
			_log.DecreaseIdent();

			if(disposing)
			{
				// Free any managed objects here. 
				//
			}

			// Free any unmanaged objects here. 
			//


			_disposed = true;
		}
	}

	~LogScope()
	{
		this.Dispose(false);
	}
}

public class SensitiveStringsList
{
	public List<SensitiveString> Items = new List<SensitiveString>();

	public void AddOrReplace(SensitiveString ss)
	{
		SensitiveString existingItem = this.Items.FirstOrDefault(item => item.OriginalValue == ss.OriginalValue);
		if(existingItem == null)
		{
			this.Items.Add(ss);
		}
		else
		{
			this.Items.Remove(existingItem);
			this.Items.Add(ss);
		}
	}
	public string ReplaceAllSensitiveStrings(string content)
	{
		foreach(SensitiveString sensitiveString in Items)
		{
			content = sensitiveString.ReplaceAllOccurnces(content);
		}
		return content;
	}
}

/// <summary>
/// Provides container for and automatic masking for particular sensitive string like password.
/// </summary>
public class SensitiveString
{
	#region Private Members

	private string _originalString;
	private int _replacementType;
	private string _replacementValue;

	#endregion

	#region Properties

	#endregion

	#region Public Methods

	public string OriginalValue
	{
		get
		{
			return _originalString;
		}
	}

	public string ReplacedValue
	{
		get
		{
			return this.ToString();
		}
	}

	public override string ToString()
	{
		string r = _originalString;

		switch(_replacementType)
		{
			case (int)SensitiveStringReplacementType.ReplaceWhole:
				r = _replacementValue;
				break;
			case (int)SensitiveStringReplacementType.Replace50PercentInMiddle:
				if(_originalString.Length == 0 || _originalString.Length == 1)
				{
					r = _replacementValue;
				}
				else if(_originalString.Length == 2)
				{
					r = _originalString[0] + _replacementValue;
				}
				else if(_originalString.Length == 3)
				{
					r = _originalString[0] + _replacementValue + _originalString[2];
				}
				else if(_originalString.Length == 4)
				{
					r = _originalString[0] + _replacementValue + _originalString[3];
				}
				else
				{
					//l:100=s:25 => s=l*25/100=l/4
					int visiblePartLength = (int)Math.Round((double)_originalString.Length / 4);
					r = _originalString.Substring(0, visiblePartLength) + _replacementValue + _originalString.Substring(_originalString.Length - visiblePartLength);
				}
				break;
			case (int)SensitiveStringReplacementType.ReplaceWithLengthInformation:
				r = "(character count: " + _replacementValue.Length + ")";
				break;
		}

		return r;
	}

	public string ReplaceAllOccurnces(string content)
	{
		content = content.Replace(_originalString, this.ReplacedValue);
		return content;
	}

	#endregion

	#region Constructors And Initialization

	public SensitiveString(string sensitiveString, int replacementType = (int)SensitiveStringReplacementType.Replace50PercentInMiddle, string replacementValue = "...hidden...")
	{
		_originalString = sensitiveString;
		_replacementType = replacementType;
		_replacementValue = replacementValue;
	}

	#endregion

	#region Deinitialization And Destructors

	#endregion

	#region Event Handlers

	#endregion

	#region Private Methods

	#endregion

	#region Helpers

	#endregion
}

public enum SensitiveStringReplacementType
{
	ReplaceWhole = 0,
	Replace50PercentInMiddle = 1,
	ReplaceWithLengthInformation = 2
}
