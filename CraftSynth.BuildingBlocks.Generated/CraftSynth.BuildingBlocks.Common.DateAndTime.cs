namespace CraftSynth.BuildingBlocks.Common;

public static class DateAndTime
{
	/// <summary>
	/// Returns for example 2018-12-31 23:59:59
	/// </summary>
	/// <returns></returns>
	public static string GetCurrentDateAndTimeInSortableFormat()
	{
		DateTime now = DateTime.Now;
		string currentDateAndTimeSortable =
			string.Format("{0}-{1}-{2} {3}:{4}:{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018-12-31
	/// </summary>
	/// <returns></returns>
	public static string GetCurrentDateInSortableFormat()
	{
		DateTime now = DateTime.Now;
		string currentDateAndTimeSortable =
			string.Format("{0}-{1}-{2}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018.12.31 23-59-59
	/// </summary>
	/// <returns></returns>
	public static string GetCurrentDateAndTimeInSortableFormatForFileSystem()
	{
		DateTime now = DateTime.Now;
		string currentDateAndTimeSortable =
			string.Format("{0}.{1}.{2}. {3}-{4}-{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018.12.31.
	/// </summary>
	/// <returns></returns>
	public static string GetCurrentDateInSortableFormatForFileSystem()
	{
		DateTime now = DateTime.Now;
		string currentDateAndTimeSortable =
			string.Format("{0}.{1}.{2}.",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018.12.31 23-59-59
	/// </summary>
	/// <returns></returns>
	public static string ToDateAndTimeInSortableFormatForFileSystem(this DateTime now)
	{
		string currentDateAndTimeSortable =
			string.Format("{0}.{1}.{2}. {3}-{4}-{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018-12-31-23-59-59
	/// </summary>
	/// <returns></returns>
	public static string ToDateAndTimeInSortableFormatForAzureBlob(this DateTime now)
	{
		string currentDateAndTimeSortable =
			string.Format("{0}-{1}-{2}-{3}-{4}-{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// returns null if error occcured. Parses this for example 2018-12-31-23-59-59
	/// </summary>
	/// <param name="s"></param>
	/// <param name="throwErrorIfInInvalidFormat"></param>
	/// <param name="errorCaseResult"></param>
	/// <returns></returns>
	public static DateTime? ParseDateAndTimeInSortableFormatForAzureBlob(this string s, bool trimNonDigitsCharsAtEnd = false)
	{
		DateTime? r;
		try
		{
			//{0}-{1}-{2}-{3}-{4}-{5}
			string[] parts = s.Split('-');
			if(trimNonDigitsCharsAtEnd)
			{
				parts[5] = parts[5].TrimNonDigitsAtEnd();
			}
			r = new DateTime(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]), int.Parse(parts[3]), int.Parse(parts[4]), int.Parse(parts[5]));
		}
		catch(Exception)
		{
			r = null;
		}
		return r;
	}

	/// <summary>
	/// Returns null if error occurred. Parses this for example 201812312359
	/// </summary>
	/// <param name="s"></param>
	/// <param name="allowCharsAfterDate"></param>
	/// <returns></returns>
	public static DateTime? ParseDateAndTimeAsYYYYMMDDHHMM(this string s, bool allowCharsAfterDate = false)
	{
		DateTime? r;
		try
		{
			//20140624000
			if(!allowCharsAfterDate && s.Length != 12)
			{
				throw new Exception("Invalid length.");
			}

			int year = int.Parse(s.Substring(0, 4));
			int month = int.Parse(s.Substring(4, 2));
			int day = int.Parse(s.Substring(6, 2));
			int hour = int.Parse(s.Substring(8, 2));
			int minute = int.Parse(s.Substring(10, 2));
			r = new DateTime(year, month, day, hour, minute, 0);
		}
		catch(Exception)
		{
			r = null;
		}
		return r;
	}

	/// <summary>
	/// Returns for example 20181231235959
	/// </summary>
	public static string ToDateAndTimeAsYYYYMMDDHHMM(this DateTime now)
	{
		string r =
			string.Format("{0}{1}{2}{3}{4}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00")
			//,now.Second.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 20181231235959
	/// </summary>
	public static string ToDateAndTimeAsYYYYMMDDHHMMSS(this DateTime now)
	{
		string r =
		string.Format("{0}{1}{2}{3}{4}{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00")
			, now.Second.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 20181231235959
	/// </summary>
	public static string ToDateAndTimeAsYYYYMMDDHHMMSSMMM(this DateTime now)
	{
		string r =
		string.Format("{0}{1}{2}{3}{4}{5}{6}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00")
			, now.Second.ToString("00")
			, now.Millisecond.ToString("000")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 2018.12.31 23-59-59
	/// </summary>
	public static string ToDDateAndTimeAs_YYYY_MM_DD__HH_MM_SS(this DateTime now)
	{
		string currentDateAndTimeSortable =
			string.Format("{0}.{1}.{2}. {3}-{4}-{5}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns for example 2018.12.31 23-59-59.999
	/// </summary>
	public static string ToDateAndTimeAs_YYYY_MM_DD__HH_MM_SS_MMM(this DateTime now)
	{
		string currentDateAndTimeSortable =
		string.Format("{0}.{1}.{2}. {3}-{4}-{5}.{6}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00"),
			now.Millisecond.ToString("000")
			);
		return currentDateAndTimeSortable;
	}

	/// <summary>
	/// Returns null on error. Parses this for example 2018.12.31 23-59-59
	/// </summary>
	public static DateTime? ParseDateAndTimeAs_YYYY_MM_DD__HH_MM_SS(this string s, bool allowCharsAfterDate = false)
	{
		DateTime? r;
		try
		{
			//2014.06.31. 23-59-59
			//01234567890123456789
			if(!allowCharsAfterDate && s.Length != 12)
			{
				throw new Exception("Invalid length.");
			}

			int year = int.Parse(s.Substring(0, 4));
			int month = int.Parse(s.Substring(5, 2));
			int day = int.Parse(s.Substring(8, 2));
			int hour = int.Parse(s.Substring(12, 2));
			int minute = int.Parse(s.Substring(15, 2));
			int second = int.Parse(s.Substring(18, 2));
			r = new DateTime(year, month, day, hour, minute, second);
		}
		catch(Exception)
		{
			r = null;
		}
		return r;
	}

	/// <summary>
	/// Returns for example 31/12/2018.
	/// </summary>
	public static string ToDDateAs_MM_DD_YYYY(this DateTime now)
	{
		string r = string.Format("{0}/{1}/{2}", // $"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year
			);
		return r;
	}
	/// <summary>
	/// Returns for example 2018.12.31
	/// </summary>
	public static string ToDDateAs_YYYY_MM_DD(this DateTime now)
	{
		string r = string.Format("{0}.{1}.{2}",
			now.Year,
			now.Month.ToString("00"),
			now.Day.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 7:59. or 3600:59
	/// </summary>
	public static string ToTimeAs_HH_MM(this TimeSpan now)
	{
		string r = string.Format("{0}:{1}",
			Math.Floor(now.TotalHours).ToString(),
			now.Minutes.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 7:59:00. or 3600:59:59
	/// </summary>
	public static string ToTimeAs_HH_MM_SS(this TimeSpan now)
	{
		string r = string.Format("{0}:{1}:{2}",
			Math.Floor(now.TotalHours).ToString(),
			now.Minutes.ToString("00"),
			now.Seconds.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 7:59:00.000. or 3600:59:59.999
	/// </summary>
	public static string ToTimeAs_HH_MM_SS_MMM(this TimeSpan now)
	{
		string r = string.Format("{0}:{1}:{2}.{3}",
			Math.Floor(now.TotalHours).ToString(),
			now.Minutes.ToString("00"),
			now.Seconds.ToString("00"),
			now.Milliseconds.ToString("000")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 31/12/2018 11:59:59 PM.
	/// </summary>
	public static string ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS_AMPM(this DateTime now)
	{
		string r = string.Format("{0}/{1}/{2} {3}:{4}:{5} {6}", //$"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")} {(now.Hour < 12 ? "AM" : "PM")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year,
			(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00"),
			(now.Hour < 12 ? "AM" : "PM")
			);

		return r;
	}

	/// <summary>
	/// Returns for example 31/12/2018 11:59:59.999 PM.
	/// </summary>
	public static string ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS_MMM_AMPM(this DateTime now)
	{
		string r = string.Format("{0}/{1}/{2} {3}:{4}:{5}.{6} {7}", //$"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")} {(now.Hour < 12 ? "AM" : "PM")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year,
			(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00"),
			now.Millisecond.ToString("00"),
			(now.Hour < 12 ? "AM" : "PM")
			);

		return r;
	}

	/// <summary>
	/// Returns for example 31/12/2018 11:59 PM.
	/// </summary>
	public static string ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_AMPM(this DateTime now)
	{
		string r = string.Format("{0}/{1}/{2} {3}:{4} {5}", //$"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")} {(now.Hour < 12 ? "AM" : "PM")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year,
			(now.Hour < 13 ? now.Hour : now.Hour - 12).ToString("00"),
			now.Minute.ToString("00"),
			(now.Hour < 12 ? "AM" : "PM")
			);

		return r;
	}

	/// <summary>
	/// Returns for example 31/12/2018 23:59:59.
	/// </summary>
	public static string ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS(this DateTime now)
	{
		string r = string.Format("{0}/{1}/{2} {3}:{4}:{5}", // $"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year,
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 31.12.2018._23-59-59.
	/// </summary>
	public static string ToDDateAndTimeAs_MM_DD_YYYY__HH_MM_SS__ForFileSystem(this DateTime now)
	{
		string r = string.Format("{0}.{1}.{2}_{3}-{4}-{5}", // $"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}";
			now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Year,
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return r;
	}

	/// <summary>
	/// Returns for example 2018.12.31._23-59-59.
	/// </summary>
	public static string ToDDateAndTimeAs_YYY_MM_DD__HH_MM_SS__ForFileSystem(this DateTime now)
	{
		string r = string.Format("{0}.{1}.{2}_{3}-{4}-{5}", // $"{now.Month.ToString("00")}/{now.Day.ToString("00")}/{now.Year} {now.Hour.ToString("00")}:{now.Minute.ToString("00")}:{now.Second.ToString("00")}";				
			now.Year,
				now.Month.ToString("00"),
			now.Day.ToString("00"),
			now.Hour.ToString("00"),
			now.Minute.ToString("00"),
			now.Second.ToString("00")
			);
		return r;
	}

	public static DateTime MoveToDayOfWeek(this DateTime input, DayOfWeek dayOfWeek, bool moveForward = true, bool resetTimeToMidninght = false, bool ifInputIsAlreadyTargetDayOfWeekDontChangeDay = true)
	{
		DateTime r = input;
		int direction = moveForward ? 1 : -1;

		if(r.DayOfWeek == dayOfWeek)
		{
			if(ifInputIsAlreadyTargetDayOfWeekDontChangeDay)
			{
				//don't change
			}
			else
			{
				//move whole week
				r = r.AddDays(direction * 7);
			}
		}
		else
		{
			while(r.DayOfWeek != dayOfWeek)
			{
				r = r.AddDays(direction * 1);
			}
		}

		if(resetTimeToMidninght)
		{
			r = r.Date;
		}

		return r;
	}
}
