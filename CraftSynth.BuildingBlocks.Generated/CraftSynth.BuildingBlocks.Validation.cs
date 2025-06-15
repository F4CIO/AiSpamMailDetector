using System.Globalization;
using System.Text.RegularExpressions;

namespace CraftSynth.BuildingBlocks;

public static class Validation
{
	public const string Pattern_EMail = @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";

	public const string Pattern_Url = @"^(?<proto>\w+)://[^/]+?(?<port>:\d+)?/";

	//        foo://example.com:8042/over/there?name=ferret#nose
	//        \_/   \______________/\_________/\__________/ \__/
	//         |           |             |           |        |
	//      scheme     authority       path        query   fragment
	//        s0          a0            p0          q0        f0
	public const string Pattern_Uri = @"^(?<s1>(?<s0>[^:/\?#]+):)?(?<a1>"
											+ @"//(?<a0>[^/\?#]*))?(?<p0>[^\?#]*)"
											+ @"(?<q1>\?(?<q0>[^#]*))?"
											+ @"(?<f1>#(?<f0>.*))?";

	// Function to test for Positive Integers.
	public static bool IsNaturalNumber(this string strNumber)
	{
		var objNotNaturalPattern = new Regex("[^0-9]");
		var objNaturalPattern = new Regex("0*[1-9][0-9]*");
		return !objNotNaturalPattern.IsMatch(strNumber) &&
		objNaturalPattern.IsMatch(strNumber);
	}
	// Function to test for Positive Integers with zero inclusive
	public static bool IsWholeNumber(this string strNumber)
	{
		var objNotWholePattern = new Regex("[^0-9]");
		return !objNotWholePattern.IsMatch(strNumber);
	}
	// Function to Test for Integers both Positive & Negative
	public static bool IsInteger(this string strNumber)
	{
		var objNotIntPattern = new Regex("[^0-9-]");
		var objIntPattern = new Regex("^-[0-9]+$|^[0-9]+$");
		return !objNotIntPattern.IsMatch(strNumber) && objIntPattern.IsMatch(strNumber);
	}
	// Function to Test for Positive Number both Integer & Real
	public static bool IsPositiveNumber(this string strNumber)
	{
		var objNotPositivePattern = new Regex("[^0-9.]");
		var objPositivePattern = new Regex("^[.][0-9]+$|[0-9]*[.]*[0-9]+$");
		var objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
		return !objNotPositivePattern.IsMatch(strNumber) &&
		objPositivePattern.IsMatch(strNumber) &&
		!objTwoDotPattern.IsMatch(strNumber);
	}
	// Function to test whether the string is valid number or not
	public static bool IsNumber(this string strNumber)
	{
		var objNotNumberPattern = new Regex("[^0-9.-]");
		var objTwoDotPattern = new Regex("[0-9]*[.][0-9]*[.][0-9]*");
		var objTwoMinusPattern = new Regex("[0-9]*[-][0-9]*[-][0-9]*");
		string strValidRealPattern = "^([-]|[.]|[-.]|[0-9])[0-9]*[.]*[0-9]+$";
		string strValidIntegerPattern = "^([-]|[0-9])[0-9]*$";
		var objNumberPattern = new Regex("(" + strValidRealPattern + ")|(" + strValidIntegerPattern + ")");
		return !objNotNumberPattern.IsMatch(strNumber) &&
		!objTwoDotPattern.IsMatch(strNumber) &&
		!objTwoMinusPattern.IsMatch(strNumber) &&
		objNumberPattern.IsMatch(strNumber);
	}

	/// <summary>
	/// </summary>
	/// <param name="s"></param>
	/// <returns></returns>
	public static bool IsNumber(this string s, bool allowSign, bool allowDots = false, bool allowCommas = false, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		bool r = true;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(!char.IsDigit(c))
				{
					if(allowSign && (c == '-' || c == '+') && i == 0)
					{
						//ok
					}
					else if(allowDots && c == '.')
					{
						//ok
					}
					else if(allowCommas && c == ',')
					{
						//ok
					}
					else
					{
						r = false;
						break;
					}
				}
				i++;
			}
		}
		return r;
	}

	public static bool IsAllAllowedCharactersUS(this string s, bool nullCaseResult = false, bool emptyCaseResult = true, bool allowLetters = false, bool allowDigits = false, bool allowPunctuations = false, string allowedExtraCharacters = null)
	{
		bool r = true;
		allowedExtraCharacters = allowedExtraCharacters ?? string.Empty;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(
					(!allowLetters && char.IsLetter(c)) ||
					(!allowDigits && char.IsDigit(c)) ||
					(!allowPunctuations && char.IsPunctuation(c))
				)
				{
					if(!allowedExtraCharacters.Contains(c))
					{
						r = false;
						break;
					}
				}
				i++;
			}
		}
		return r;
	}

	public static bool IsLetterOrCombiningMark(this char c)
	{
		UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
		return category == UnicodeCategory.UppercaseLetter ||
			   category == UnicodeCategory.LowercaseLetter ||
			   category == UnicodeCategory.TitlecaseLetter ||
			   category == UnicodeCategory.ModifierLetter ||
			   category == UnicodeCategory.OtherLetter ||
			   category == UnicodeCategory.NonSpacingMark ||
			   category == UnicodeCategory.SpacingCombiningMark ||
			   category == UnicodeCategory.EnclosingMark;
	}

	public static bool IsLetterDigitOrCombiningMark(this char c)
	{
		UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

		return category == UnicodeCategory.UppercaseLetter ||
			   category == UnicodeCategory.LowercaseLetter ||
			   category == UnicodeCategory.TitlecaseLetter ||
			   category == UnicodeCategory.ModifierLetter ||
			   category == UnicodeCategory.OtherLetter ||
			   category == UnicodeCategory.DecimalDigitNumber ||
			   category == UnicodeCategory.NonSpacingMark ||
			   category == UnicodeCategory.SpacingCombiningMark ||
			   category == UnicodeCategory.EnclosingMark;
	}

	public static bool IsAllLetters(this string s, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		if(s == null)
		{
			return nullCaseResult;
		}
		else if(s == string.Empty)
		{
			return emptyCaseResult;
		}
		else
		{
			foreach(char c in s)
			{
				if(!c.IsLetterOrCombiningMark())
				{
					return false;
				}
			}
			return true;
		}
	}

	public static bool IsAllLettersOrExceptions(this string s, string exceptionalCharacters = null, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		bool r = true;
		exceptionalCharacters = exceptionalCharacters ?? string.Empty;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(!c.IsLetterOrCombiningMark() && !exceptionalCharacters.Contains(c))
				{
					r = false;
					break;
				}
				i++;
			}
		}
		return r;
	}

	public static bool IsAllLettersOrDigits(this string s, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		if(s == null)
		{
			return nullCaseResult;
		}
		else if(s == string.Empty)
		{
			return emptyCaseResult;
		}
		else
		{
			foreach(char c in s)
			{
				if(!c.IsLetterDigitOrCombiningMark())
				{
					return false;
				}
			}
			return true;
		}
	}

	public static bool IsAllLettersDigitsOrExceptions(this string s, string exceptionalCharacters = null, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		bool r = true;
		exceptionalCharacters = exceptionalCharacters ?? string.Empty;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(!c.IsLetterDigitOrCombiningMark() && !exceptionalCharacters.Contains(c))
				{
					r = false;
					break;
				}
				i++;
			}
		}
		return r;
	}

	public static bool IsAllLettersOrPunctuations(this string s, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		bool r = true;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(!c.IsLetterOrCombiningMark() && !char.IsPunctuation(c))
				{
					r = false;
					break;
				}
				i++;
			}
		}
		return r;
	}

	public static bool IsAllLettersDigitsOrPunctuations(this string s, bool nullCaseResult = false, bool emptyCaseResult = true)
	{
		bool r = true;

		if(s == null)
		{
			r = nullCaseResult;
		}
		else if(s == string.Empty)
		{
			r = emptyCaseResult;
		}
		else
		{
			int i = 0;
			foreach(char c in s)
			{
				if(!c.IsLetterDigitOrCombiningMark() && !char.IsPunctuation(c))
				{
					r = false;
					break;
				}
				i++;
			}
		}
		return r;
	}

	///// <summary>
	///// Returns true if all characters are among these A-Z,a-Z,0-9
	///// </summary>
	///// <param name="s"></param>
	///// <param name="r"></param>
	///// <returns></returns>
	//public static bool IsAlphaNumeric(this string s)
	//{
	//	bool r;

	//	const string alphaNumericArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
	//	foreach (char ch in s)
	//	{
	//		if (!alphaNumericArray.Contains(ch))
	//		{
	//			r = false;
	//			break;
	//		}
	//	}
	//	r = true;

	//	return r;
	//}

	///// <summary>
	///// Returns true if all characters are among these A-Z,a-Z
	///// </summary>
	///// <param name="s"></param>
	///// <param name="r"></param>
	///// <returns></returns>
	//public static bool IsAlpha(this string s)
	//{
	//	bool r;

	//	const string alphaArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
	//	foreach (char ch in s)
	//	{
	//		if (!alphaArray.Contains(ch))
	//		{
	//			r = false;
	//			break;
	//		}
	//	}
	//	r = true;

	//	return r;
	//}

	// Function To test for Alphabets.
	public static bool IsAlphaUS(this string strToCheck)
	{
		var objAlphaPattern = new Regex("[^a-zA-Z]");
		return !objAlphaPattern.IsMatch(strToCheck);
	}
	// Function to Check for AlphaNumeric.
	public static bool IsAlphaNumericUS(this string strToCheck)
	{
		var objAlphaNumericPattern = new Regex("[^a-zA-Z0-9]");
		return !objAlphaNumericPattern.IsMatch(strToCheck);
	}

	/// <summary>
	/// Checks wether passed string is valid full url.
	/// </summary>
	/// <param name="strToCheck">Uri to check where protocol part must be included. Examples:
	/// '<example>http://www.mywebsite.com:8080/mywebapp/mydir/mypage.htm?mykey1=myvalue1&mykey2=myvalue2#myanchor</example>'
	/// '<example>http://www.mywebsite.com</example>'
	/// </param>
	/// <returns></returns>
	public static bool IsUri(this string strToCheck, bool uriWithoutHttpOrHttpsIsValid)
	{
		//Regex objAplhaPattern = new Regex(Pattern_Uri);
		//return objAplhaPattern.IsMatch(strToCheck);

		bool r;

		if(!uriWithoutHttpOrHttpsIsValid)
		{
			r = Uri.IsWellFormedUriString(strToCheck, UriKind.Absolute);
		}
		else
		{
			r = Uri.IsWellFormedUriString(strToCheck, UriKind.Absolute);
			if(r == false)
			{
				r = Uri.IsWellFormedUriString("http://" + strToCheck, UriKind.Absolute);
			}
		}

		return r;
	}

	/// <summary>
	/// method to determine is the absolute file path is a valid path
	/// </summary>
	/// <param name="path">the path we want to check</param>
	public static bool IsFilePath(this string path)
	{
		string pattern = @"^(([a-zA-Z]\:)|(\\))(\\{1}|((\\{1})[^\\]([^/:*?<>""|]*))+)$";
		var reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
		return reg.IsMatch(path);
	}


	public static bool IsEMail(this string strToCheck)
	{
		///Initial version: Was Laying about "03 9620 9611"!!! 
		//Regex objAlphaPattern = new Regex(Pattern_EMail);
		//return !objAlphaPattern.IsMatch(strToCheck);

		if(string.IsNullOrEmpty(strToCheck))
		{
			return false;
		}

		// Use IdnMapping class to convert Unicode domain names.
		strToCheck = Regex.Replace(strToCheck, @"(@)(.+)$", DomainMapper);
		if(invalid)
		{
			return false;
		}

		// Return true if strIn is in valid e-mail format. 
		return Regex.IsMatch(strToCheck,
				  @"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
				  @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9]{2,17}))$",
				  RegexOptions.IgnoreCase);
	}
	private static bool invalid = false;
	private static string DomainMapper(Match match)
	{
		// IdnMapping class with default property values.
		var idn = new IdnMapping();

		string domainName = match.Groups[2].Value;
		try
		{
			domainName = idn.GetAscii(domainName);
		}
		catch(ArgumentException)
		{
			invalid = true;
		}
		return match.Groups[1].Value + domainName;
	}
}
