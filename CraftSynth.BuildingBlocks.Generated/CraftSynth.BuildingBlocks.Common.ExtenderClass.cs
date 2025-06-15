using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace CraftSynth.BuildingBlocks.Common;

public static class ExtenderClass
{
    #region Generics

    public static T2? NullOr<T, T2>(this T obj, T2 nonNullCase) where T2 : struct
    {
        if(obj == null)
        {
            return null;
        }
        else
        {
            return nonNullCase;
        }
    }

    public static T2? NullOr<T, T2>(this T obj, T2? nonNullCase) where T2 : struct
    {
        if(obj == null)
        {
            return null;
        }
        else
        {
            return nonNullCase;
        }
    }

    public static bool IsNull<T>(this T? nullableObject) where T : struct
    {
        return !nullableObject.HasValue;
    }

    /// <summary>
    /// Gets property name from expression without magic strings. 
    /// Examle: ExtenderClass.GetPropertyName(()=>Model.AdvertisementImage)  
    /// "Extended" expressions such model.Propery.PropertyA are not supported.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="expression"></param>
    /// <returns></returns>
    public static string GetPropertyName<T>(System.Linq.Expressions.Expression<Func<T>> expression)
    {
        var body = expression.Body as System.Linq.Expressions.MemberExpression;
        if(body == null)
        {
            throw new ArgumentException("'expression' should be a member expression");
        }

        return body.Member.Name;
    }

    public static bool SequenceEqual<T>(this IEnumerable<T> s1, IEnumerable<T> s2)
    {
        bool equal;
        if(s1 == null && s2 == null)
        {
            equal = true;
        }
        else if(s1 == null || s2 == null)
        {
            equal = false;
        }
        else if(s1.LongCount() != s2.LongCount())
        {
            equal = false;
        }
        else
        {
            equal = true;
            System.Collections.IEnumerator en = s1.GetEnumerator();
            System.Collections.IEnumerator en2 = s2.GetEnumerator();
            while(en.MoveNext())
            {
                en2.MoveNext();
                if(en.Current != null && en.Current != null)
                {
                    if(
                        (en.Current != null && en2.Current == null)
                        || (en.Current == null && en2.Current != null)
                        || (!en.Current.Equals(en2.Current))
                        )
                    {
                        equal = false;
                        break;
                    }
                }
            }
        }
        return equal;
    }

    public static void CopyAllPropertiesTo<T>(this T source, T target) where T : class
    {
        if(source == null)
        {
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");
        }
        if(target == null)
        {
            throw new ArgumentNullException(nameof(target), "Target cannot be null.");
        }

        // Get the properties of the type
        PropertyInfo[] properties = typeof(T).GetProperties();

        foreach(PropertyInfo property in properties)
        {
            // Check if the property can be read from the source and written to the target
            if(property.CanRead && property.CanWrite)
            {
                // Copy the value from the source to the target
                object? value = property.GetValue(source, null);
                property.SetValue(target, value, null);
            }
        }
    }

    public static void CopyAllPropertiesTo<TTarget, TSource>(this TSource source, TTarget target)
        where TSource : class
        where TTarget : class
    {
        if(source == null)
        {
            throw new ArgumentNullException(nameof(source), "Source cannot be null.");
        }
        if(target == null)
        {
            throw new ArgumentNullException(nameof(target), "Target cannot be null.");
        }

        // Get the properties of the source type and copy them to the target type
        PropertyInfo[] sourceProps = source.GetType().GetProperties();
        foreach(PropertyInfo propInfo in sourceProps)
        {
            // Check if the source property can be read
            if(!propInfo.CanRead)
            {
                continue;
            }

            PropertyInfo targetPropInfo = target.GetType().GetProperty(propInfo.Name);

            // Check if the target property exists and can be written to
            if(targetPropInfo != null && targetPropInfo.CanWrite)
            {
                // Check if the properties are of the same type
                if(targetPropInfo.PropertyType.IsAssignableFrom(propInfo.PropertyType))
                {
                    // Copy the value from the source to the target
                    targetPropInfo.SetValue(target, propInfo.GetValue(source, null), null);
                }
            }
        }
    }

    #endregion

    #region Enum
    /// <summary>
    /// Gets Description from enum value ie [Description("MasterCard")] returns MasterCard
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string Description(this Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        var attributes =
            (DescriptionAttribute[])fi.GetCustomAttributes(
                typeof(DescriptionAttribute), false);
        return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
    }

    public static T? GetEnumFromString<T>(string enumConstantName, T? errorCaseResult = null, bool ignoreCase = true) where T : struct, IConvertible
    {
        T? r = null;

        if(!typeof(T).IsEnum)
        {
            r = errorCaseResult;
        }
        else
        if(string.IsNullOrEmpty(enumConstantName))
        {
            r = errorCaseResult;
        }
        else
        {
            var enumNames = Enum.GetNames(typeof(T)).ToList();
            string matchedConstantName = enumNames.FirstOrDefault(n => string.Compare(n, enumConstantName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) == 0);
            if(matchedConstantName == null)
            {
                r = errorCaseResult;
            }
            else
            {
                r = (T)Enum.Parse(typeof(T), matchedConstantName);
            }
        }

        return r;
    }

    /// <summary>
    /// Usage: 
    /// CraftSynth.BuildingBlocks.Common.ExtenderClass.GetEnumAsList<MyEnum>()
    /// </summary>
    public static List<T> GetEnumAsList<T>()
    {
        var r = new List<T>();

        foreach(string enumItemAsString in Enum.GetNames(typeof(T)))
        {
            var enumItem = (T)Enum.Parse(typeof(T), enumItemAsString);
            r.Add(enumItem);
        }

        return r;
    }

    /// <summary>
    /// Usage: 
    /// CraftSynth.BuildingBlocks.Common.ExtenderClass.GetEnumAsListOfInt<MyEnum>()
    /// </summary>
    public static List<int> GetEnumAsListOfInt<T>()
    {
        var r = new List<int>();

        foreach(string enumItemAsString in Enum.GetNames(typeof(T)))
        {
            int enumItem = (int)Enum.Parse(typeof(T), enumItemAsString);
            r.Add(enumItem);
        }

        return r;
    }
    #endregion

    #region Bool
    public static string ToYesOrNoString(this bool v)
    {
        string r = v ? "yes" : "no";
        return r;
    }
    #endregion

    #region String
    public static string ToNonNullString<T>(this T? nullableObject) where T : struct
    {
        return ToNonNullString<T>(nullableObject, string.Empty);
    }

    public static string ToNonNullString<T>(this T? nullableObject, string nullCaseString) where T : struct
    {
        if(nullableObject.HasValue)
        {
            return nullableObject.Value.ToString();
        }
        else
        {
            return nullCaseString;
        }
    }

    public static string ToNonNullString(this DateTime? nullableObject)
    {
        return ToNonNullString(nullableObject, string.Empty, null);
    }

    public static string ToNonNullString(this DateTime? nullableObject, string nullCaseString)
    {
        return ToNonNullString(nullableObject, nullCaseString, null);
    }

    public static string ToNonNullString(this DateTime? nullableObject, string nullCaseString, string format)
    {
        if(nullableObject.HasValue)
        {
            if(format == null)
            {
                return nullableObject.Value.ToString();
            }
            else
            {
                return nullableObject.Value.ToString(format);
            }
        }
        else
        {
            return nullCaseString;
        }
    }

    public static string ToNonNullString(this string nullableObject)
    {
        return ToNonNullString(nullableObject, string.Empty);
    }

    public static string ToNonNullString(this string nullableObject, string nullCaseString)
    {
        if(nullableObject != null)
        {
            return nullableObject;
        }
        else
        {
            return nullCaseString;
        }
    }

    public static string ToNonNullNonEmptyString(this string nullableObject, string nullOrEmptyCaseString)
    {
        if(nullableObject != null && nullableObject.Trim().Length > 0)
        {
            return nullableObject;
        }
        else
        {
            return nullOrEmptyCaseString;
        }
    }

    public static bool IsNullOrWhiteSpace(this string nullableObject)
    {
        return string.IsNullOrEmpty(nullableObject) || string.IsNullOrEmpty(nullableObject.Trim());
    }

    public static bool IsNOTNullOrWhiteSpace(this string nullableObject)
    {
        return nullableObject.ToNonNullString().Trim().Length > 0;
    }

    public static bool IsEqualTo(this string s1, string s2, bool caseSensitive, bool bothNullCaseResult = true, bool onlyOneIsNullCaseResult = false)
    {
        if(s1 == null && s2 == null)
        {
            return bothNullCaseResult;
        }
        else if(s1 == null && s2 != null)
        {
            return onlyOneIsNullCaseResult;
        }
        else if(s1 != null && s2 == null)
        {
            return onlyOneIsNullCaseResult;
        }
        else
        {
            if(!caseSensitive)
            {
                return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
            }
            else
            {
                return string.Compare(s1, s2, StringComparison.Ordinal) == 0;
            }
        }
    }

    public static bool IsEqualToWhileDisregardingCasing(this string s1, string s2, bool bothNullCaseResult = true, bool onlyOneIsNullCaseResult = false)
    {
        if(s1 == null && s2 == null)
        {
            return bothNullCaseResult;
        }
        else if(s1 == null && s2 != null)
        {
            return onlyOneIsNullCaseResult;
        }
        else if(s1 != null && s2 == null)
        {
            return onlyOneIsNullCaseResult;
        }
        else
        {
            return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }

    public static bool IsNOTEqualToWhileDisregardingCasing(this string s1, string s2, bool bothNullCaseResult = false, bool onlyOneIsNullCaseResult = true)
    {
        if(s1 == null && s2 == null)
        {
            return bothNullCaseResult;
        }
        else if(s1 == null && s2 != null)
        {
            return onlyOneIsNullCaseResult;
        }
        else if(s1 != null && s2 == null)
        {
            return onlyOneIsNullCaseResult;
        }
        else
        {
            return string.Compare(s1, s2, StringComparison.OrdinalIgnoreCase) != 0;
        }
    }

    public static Nullable<T> ToNullable<T>(this string exXmlValue) where T : struct, IConvertible
    {
        if(string.IsNullOrEmpty(exXmlValue))
        {
            return null;
        }
        else
        {
            return (T)Convert.ChangeType(exXmlValue, typeof(T));
        }
    }

    public static string EnclosedWithPercentSign(this string nullableObject)
    {
        string result = nullableObject;

        if(nullableObject.IsNOTNullOrWhiteSpace())
        {
            if(!nullableObject.StartsWith("%"))
            {
                result = "%" + result;
            }

            if(!nullableObject.EndsWith("%"))
            {
                result = result + "%";
            }
        }

        return result;
    }

    public static string AppendIfValueToCheckIsNotNull(this string s, string? prefix, object? valueToCheck, string? postfix = "")
    {
        string r = s;
        if(valueToCheck != null)
        {
            r = r + prefix + valueToCheck + postfix;
        }
        return r;
    }

    public static string AppendIfValueToCheckIsNotNullOrWhiteSpace(this string s, string? prefix, object? valueToCheck, string? postfix = "")
    {
        string r = s;
        if(valueToCheck != null && (valueToCheck is not string || (valueToCheck is string && (valueToCheck as string).IsNOTNullOrWhiteSpace())))
        {
            r = r + prefix + valueToCheck + postfix;
        }
        return r;
    }

    public static string AppendIfNotNullOrWhiteSpace(this string nullableObject, string stringToAppend)
    {
        string result = nullableObject;
        if(nullableObject.IsNOTNullOrWhiteSpace())
        {
            result = result + stringToAppend;
        }
        return result;
    }

    public static string PrependIfNotNullOrWhiteSpace(this string nullableObject, string stringToPrepend)
    {
        string result = nullableObject;
        if(nullableObject.IsNOTNullOrWhiteSpace())
        {
            result = stringToPrepend + result;
        }
        return result;
    }

    /// <summary>
    /// Returns bytes of specified string. No encoding is used.
    /// PROS: No data loss as with encoding when char is illegal.
    /// CONS: This and ToString method must be both used on same machine - Other case not tested.
    /// Source: http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static byte[] ToBytes(this string s)
    {
        byte[] bytes = new byte[s.Length * sizeof(char)];
        System.Buffer.BlockCopy(s.ToCharArray(), 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Returns string build up with bytes - no encoding is used.
    /// PROS: No data loss as with encoding when char is illegal.
    /// CONS: This and ToString method must be both used on same machine - Other case not tested.
    /// Source: http://stackoverflow.com/questions/472906/net-string-to-byte-array-c-sharp
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static string ToString(this byte[] bytes)
    {
        char[] chars = new char[bytes.Length / sizeof(char)];
        System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
        return new string(chars);
    }

    public static string ReplaceNonUnicodeChars(string inputText, string replacement, bool alsoReplaceControlChars = false, char[]? charsToPreserve = null)
    {
        var stringBuilder = new StringBuilder(inputText.Length);
        bool replaceIt = false;
        foreach(char c in inputText)
        {
            replaceIt = false;
            replaceIt = !replaceIt && alsoReplaceControlChars && char.IsControl(c);
            replaceIt = !replaceIt && CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.OtherNotAssigned;

            if(replaceIt && charsToPreserve != null && charsToPreserve.Contains(c))
            {
                replaceIt = false;
            }

            if(!replaceIt)
            {
                stringBuilder.Append(c);
            }
            else
            {
                if(replacement != null)
                {
                    stringBuilder.Append(replacement);
                }
            }
        }
        return stringBuilder.ToString();
    }

    /// <summary>
    /// Source: https://stackoverflow.com/questions/40564692/c-sharp-regex-to-remove-non-printable-characters-and-control-characters-in-a
    /// </summary>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string ReplaceNonPrintableChars(this string s, string r)
    {
        var sb1 = new StringBuilder(s.Length);
        foreach(char ch in s)
        {
            if(ch != ' ' && !char.IsControl(ch))
            {
                sb1.Append(ch);
            }
            else
            {
                sb1.Append(r);
            }
        }
        return sb1.ToString();
    }

    public static bool IsNullOrNonPrintable(this string s)
    {
        if(s == null)
        {
            return true;
        }
        else
        {
            return s.All(c => c == ' ' || char.IsControl(c));
        }
    }

    public static bool IsPrintable(this string s)
    {
        if(s == null)
        {
            return false;
        }
        else
        {
            return !s.All(c => c == ' ' || char.IsControl(c));
        }
    }

    public static string TrimNonPrintableChars(this string s)
    {
        //int firstPrintableIndex = int.MaxValue;
        //int lastPrintableIndex = -1;
        //for(int i=0;i<s.Length;i++)
        //{
        //	if (s[i] == ' ' || char.IsControl(s[i]))
        //	{
        //		firstPrintableIndex = Math.Min(firstPrintableIndex, i);
        //		lastPrintableIndex = Math.Max(lastPrintableIndex, i);
        //	}
        //}
        int firstPrintableIndex = s.IndexOf(s.FirstOrDefault(c => c != ' ' && !char.IsControl(c)));
        int lastPrintableIndex = s.LastIndexOf(s.LastOrDefault(c => c != ' ' && !char.IsControl(c)));

        if(firstPrintableIndex == -1 || lastPrintableIndex == -1)
        {
            return s;
        }
        else
        {
            return s.Substring(firstPrintableIndex, lastPrintableIndex - firstPrintableIndex + 1);
        }
    }

    public static string TrimStartOfNonPrintableChars(this string s)
    {
        int? firstPrintableIndex = 0;
        int? lastPrintableIndex = s.LastOrDefault(c => c == ' ' || char.IsControl(c));
        if(firstPrintableIndex == null)
        {
            return s;
        }
        else
        {
            return s.Substring(firstPrintableIndex.Value, lastPrintableIndex.Value - firstPrintableIndex.Value + 1);
        }
    }

    public static string TrimEndOfNonPrintableChars(this string s)
    {
        int? firstPrintableIndex = s.FirstOrDefault(c => c == ' ' || char.IsControl(c));
        int? lastPrintableIndex = s.Length - 1;
        if(firstPrintableIndex == null)
        {
            return s;
        }
        else
        {
            return s.Substring(firstPrintableIndex.Value, lastPrintableIndex.Value - firstPrintableIndex.Value + 1);
        }
    }

    /// <summary>
    /// All except these chars are replaced:
    /// (space)!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
    /// </summary>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string ReplaceNonUSAndNonPrintableChars(this string s, string r)
    {
        var sb1 = new StringBuilder(s.Length);
        foreach(char ch in s)
        {
            if(0x20 <= ch && ch <= 0x7E)
            {
                sb1.Append(ch);
            }
            else
            {
                sb1.Append(r);
            }
        }
        return sb1.ToString();
    }

    /// <summary>
    /// All except these chars are replaced:
    /// A-Z,a-Z,0-9
    /// </summary>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string ReplaceNonAlphaNumericCharacters(this string s, string r)
    {
        var sb1 = new StringBuilder(s.Length);
        foreach(char ch in s)
        {
            if(char.IsLetterOrDigit(ch))
            {
                sb1.Append(ch);
            }
            else
            {
                sb1.Append(r);
            }
        }
        return sb1.ToString();
    }

    /// <summary>
    /// All except these chars are replaced:
    /// A-Z,a-Z
    /// </summary>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string ReplaceNonAlphaCharacters(this string s, string r)
    {
        var sb1 = new StringBuilder(s.Length);
        foreach(char ch in s)
        {
            if(char.IsLetter(ch))
            {
                sb1.Append(ch);
            }
            else
            {
                sb1.Append(r);
            }
        }
        return sb1.ToString();
    }

    /// <summary>
    /// All chars found in Path.GetInvalidFileNameChars() or Path.GetInvalidPathChars() are replaced.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    public static string ReplaceInvalidFileSystemCharacters(this string s, string r)
    {
        IEnumerable<char> invalid = Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars());

        foreach(char c in invalid)
        {
            s = s.Replace(c.ToString(), r);
        }

        return s;
    }

    public static (string replacedText, int replacementsCount) ReplaceLines(string text, string oldLines, string newLines, bool ignoreCase = false, bool ignoreLeadingWhitespaces = false, bool ignoreEndingWhitespaces = false)
    {
        List<string> textLines = text.ToLines(true, true, null);
        List<string> oldLinesArray = oldLines.ToLines(true, true, null);
        var sb = new StringBuilder();
        int replacementsCount = 0; // Initialize the replacement counter

        for(int i = 0; i < textLines.Count; i++)
        {
            bool isMatch = true;
            if(i + oldLinesArray.Count <= textLines.Count) // Ensure there are enough lines remaining for a potential match
            {
                for(int j = 0; j < oldLinesArray.Count && isMatch; j++)
                {
                    string textLine = textLines[i + j];
                    string oldLine = oldLinesArray[j];

                    if(ignoreCase)
                    {
                        textLine = textLine.ToLower();
                        oldLine = oldLine.ToLower();
                    }

                    if(ignoreLeadingWhitespaces)
                    {
                        textLine = textLine.TrimStart();
                        oldLine = oldLine.TrimStart();
                    }
                    if(ignoreEndingWhitespaces)
                    {
                        textLine = textLine.TrimEnd();
                        oldLine = oldLine.TrimEnd();
                    }

                    if(textLine != oldLine)
                    {
                        isMatch = false;
                    }
                }

                if(isMatch)
                {
                    sb.Append(newLines + "\r\n"); // Perform the replacement
                    i += oldLinesArray.Count - 1; // Skip the matched lines
                    replacementsCount++; // Increment the counter
                    continue;
                }
            }

            sb.Append(textLines[i]);
            if(i < textLines.Count - 1) // Avoid adding a new line at the end of text
            {
                sb.Append('\n');
            }
        }

        return (sb.ToString(), replacementsCount); // Return the modified text and the number of replacements
    }

    public static string RemoveRepeatedWords(this string s)
    {
        if(s == null)
        {
            //just return s;
        }
        else if(s.Length == 0)
        {
            //just return s;
        }
        else
        {
            var words = s.Split(' ').ToList();
            var uniqueWordList = new List<string>();
            foreach(string word in words)
            {
                if(!uniqueWordList.Contains(word))
                {
                    uniqueWordList.Add(word);
                }
            }
            s = uniqueWordList.ToSingleString(null, null, ' ');
            return s;
        }
        return s;
    }

    /// <summary>
    /// All except these chars are replaced with: &#(char code):
    /// (space)!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string AddHtmlReferenceToNonUSAndNonPrintableChars(this string s)
    {
        var sb1 = new StringBuilder(s.Length);
        foreach(char ch in s)
        {
            if(0x20 <= ch && ch <= 0x7E)
            {
                sb1.Append(ch);
            }
            else
            {
                sb1.Append("&#" + (int)ch);
            }
        }
        return sb1.ToString();
    }

    /// <summary>
    /// Returns true if contains any character outside from this list:
    /// (space)!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool HasNonUSOrNonPrintableChars(this string s)
    {
        foreach(char ch in s)
        {
            if(0x20 <= ch && ch <= 0x7E)
            {
                //
            }
            else
            {
                return true;
            }
        }
        return false;
    }

    public static string PrependWithTimestamp(this string s, bool useUtc = false)
    {
        DateTime now = useUtc ? DateTime.UtcNow : DateTime.Now;
        s = string.Format("{0}.{1}.{2} {3}:{4}:{5} ({6}) {7}",
            now.Year,
            now.Month.ToString().PadLeft(2, '0'),
            now.Day.ToString().PadLeft(2, '0'),
            now.Hour.ToString().PadLeft(2, '0'),
            now.Minute.ToString().PadLeft(2, '0'),
            now.Second.ToString().PadLeft(2, '0'),
            useUtc ? "UTC" : "local",
            s);
        return s;
    }


    public static string FirstXChars(this string s, int count, string endingIfTrimmed)
    {
        string r = string.Empty;

        if(s.ToNonNullString().Length >= count)
        {

            r = s.ToNonNullString().Substring(0, count - endingIfTrimmed.ToNonNullString().Length).ToNonNullString() + endingIfTrimmed.ToNonNullString();
        }
        else
        {
            r = s.ToNonNullString();
        }

        return r;
    }

    /// <summary>
    /// Returns ending of string.
    /// If count is bigger than character count in string whole string is returned. 
    /// </summary>
    /// <param name="s"></param>
    /// <param name="count"></param>
    /// <param name="startingPartIfTrimmed">Can be used as '...' to indicate that result was trimmed at the beginning.</param>
    /// <returns></returns>
    public static string LastXChars(this string s, int count, string startingPartIfTrimmed)
    {
        string r = s.ToNonNullString();
        startingPartIfTrimmed = startingPartIfTrimmed.ToNonNullString();

        if(r.Length >= count)
        {
            if(startingPartIfTrimmed.Length > count)
            {
                throw new ArgumentException("Length of startingPartIfTrimmed must be smaller than count.");
            }
            r = r.Substring(r.Length - count, count);
            r = r.Remove(0, startingPartIfTrimmed.Length);
            r = startingPartIfTrimmed + r;
        }

        return r;
    }

    /// <summary>
    /// Example: from "Today is lovely wether!" makes "Toda...her!"
    /// </summary>
    /// <param name="s"></param>
    /// <param name="maxLength"></param>
    /// <param name="middleReplacement"></param>
    /// <returns></returns>
    public static string Bubble(this string s, int maxLength, string middleReplacement)
    {
        if(maxLength < middleReplacement.Length + 2)
        {
            throw new ArgumentException("maxLength too small or middleReplacement too long");
        }
        string r;
        if(s.Length < maxLength)
        {
            r = s;
        }
        else
        {
            int reminder;
            int countOfCharsVisibleAtBeginning = Math.DivRem(maxLength - middleReplacement.Length, 2, out reminder);
            int countOfCharsVisibleAtTheEnd = countOfCharsVisibleAtBeginning + reminder;

            r = s.FirstXChars(countOfCharsVisibleAtBeginning, string.Empty);
            r = r + middleReplacement;
            r = r + s.LastXChars(countOfCharsVisibleAtTheEnd, string.Empty);
        }
        return r;
    }

    public static string GetSubstringBefore(this string s, string endMarker, string nullCaseResult = null, string zeroLengthCaseResult = "", string endMarkerNotFoundCaseResult = null, bool throwExceptionIfEndMarkerIsNullOrZeroLength = true, string endMarkerIsNullOrZeroLengthCaseResult = null)
    {
        string r = null;

        if(s == null)
        {
            r = nullCaseResult;
        }
        else if(s.Length == 0)
        {
            r = zeroLengthCaseResult;
        }
        else if(endMarker == null || endMarker.Length == 0)
        {
            if(throwExceptionIfEndMarkerIsNullOrZeroLength)
            {
                throw new Exception("endMarker is null");
            }
            else
            {
                r = endMarkerIsNullOrZeroLengthCaseResult;
            }
        }
        else
        {
            int endMarkerIndex = s.IndexOf(endMarker);

            if(endMarkerIndex < 0)
            {
                r = endMarkerNotFoundCaseResult;
            }
            else if(endMarkerIndex == 0)
            {
                r = string.Empty;
            }
            else if(endMarkerIndex > 0)
            {
                r = s.Substring(0, endMarkerIndex);
            }
        }

        return r;
    }

    public static string GetSubstringAfter(this string s, string startMarker)
    {
        string r = null;

        if(s != null && !string.IsNullOrEmpty(startMarker))
        {
            int startMarkerIndex = s.IndexOf(startMarker);
            int endMarkerIndex = s.Length;

            if(startMarkerIndex >= 0 && endMarkerIndex >= 0)
            {
                startMarkerIndex = startMarkerIndex + startMarker.Length;

                r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);
            }
        }

        return r;
    }

    public static string GetSubstringAfterLastOccurence(this string s, string marker)
    {
        string r = null;

        if(s != null && !string.IsNullOrEmpty(marker))
        {
            int startMarkerIndex = s.LastIndexOf(marker);
            int endMarkerIndex = s.Length;

            if(startMarkerIndex >= 0 && endMarkerIndex >= 0)
            {
                startMarkerIndex = startMarkerIndex + marker.Length;

                r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);
            }
        }

        return r;
    }
    public static string GetSubstring(this string s, string startMarker, string endMarker, bool includeMarkersInResult = false, int? searchFromIndex = null, bool caseSensitive = true, string notFoundCaseResult = null)
    {
        int? notUsedA;
        int? notUsedB;
        string r = GetSubstring(s, startMarker, endMarker, includeMarkersInResult, out notUsedA, out notUsedB, searchFromIndex, caseSensitive);
        return r;
    }
    public static string GetSubstring(this string s, string startMarker, string endMarker, bool includeMarkersInResult, out int? startIndex, out int? endIndex, int? searchFromIndex, bool caseSensitive = true, string notFoundCaseResult = null)
    {
        string r = notFoundCaseResult;
        startIndex = null;
        endIndex = null;

        if(s != null && !string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
        {
            int startMarkerIndex = s.IndexOf(startMarker, searchFromIndex ?? 0, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            int endMarkerIndex = -1;
            if(startMarkerIndex > -1 && startMarkerIndex + startMarker.Length < s.Length)
            {
                endMarkerIndex = s.IndexOf(endMarker, startMarkerIndex + startMarker.Length, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
            }

            if(startMarkerIndex >= 0 && endMarkerIndex >= 0)
            {
                if(!includeMarkersInResult)
                {
                    startMarkerIndex = startMarkerIndex + startMarker.Length;

                    r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex);

                    startIndex = startMarkerIndex;
                    endIndex = endMarkerIndex + endMarker.Length;
                }
                else
                {
                    r = s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex + endMarker.Length);

                    startIndex = startMarkerIndex;
                    endIndex = endMarkerIndex + endMarker.Length;
                }
            }
        }

        return r;
    }
    public static List<string> GetSubstrings(this string sOrig, string startMarker, string endMarker, bool caseSensitive = true)
    {
        var r = new List<string>();

        if(sOrig == null)
        {
            throw new Exception("GetSubstrings: sOrig can not be null.");
        }

        string s = sOrig;
        while(true)
        {
            if(s != null && !string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
            {
                int startMarkerIndex = s.IndexOf(startMarker, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
                if(startMarkerIndex >= 0)
                {
                    int endMarkerIndex = s.IndexOf(endMarker, startMarkerIndex + startMarker.Length, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);

                    if(startMarkerIndex >= 0 && endMarkerIndex >= 0)
                    {
                        startMarkerIndex = startMarkerIndex + startMarker.Length;

                        r.Add(s.Substring(startMarkerIndex, endMarkerIndex - startMarkerIndex));
                    }
                    else
                    {
                        break;
                    }
                    s = s.Remove(0, endMarkerIndex + endMarker.Length);
                }
                else
                {
                    break;
                }
            }
        }

        return r;
    }

    public static string ReplaceSubstrings(this string s, string startMarker, string endMarker, string replacement, bool preserveMarkers = false, bool caseSensitive = true)
    {
        List<string> parts = s.GetSubstrings(startMarker, endMarker, caseSensitive);
        if(preserveMarkers)
        {
            replacement = startMarker + replacement + endMarker;
        }
        foreach(string p in parts)
        {
            s = s.Replace(startMarker + p + endMarker, replacement);
        }
        return s;
    }

    public static string RemoveSubstring(this string s, string startMarker, string endMarker, bool removeMarkersAlso)
    {
        string r = null;

        if(s != null && !string.IsNullOrEmpty(startMarker) && !string.IsNullOrEmpty(endMarker))
        {
            int startMarkerIndex = s.IndexOf(startMarker);
            int endMarkerIndex = s.IndexOf(endMarker, startMarkerIndex);

            if(startMarkerIndex >= 0 && endMarkerIndex >= 0)
            {
                if(!removeMarkersAlso)
                {
                    startMarkerIndex = startMarkerIndex + startMarker.Length;
                }
                else
                {
                    endMarkerIndex = endMarkerIndex + endMarker.Length;
                }

                r = s.Remove(startMarkerIndex, endMarkerIndex - startMarkerIndex);
            }
        }

        return r;
    }

    public static string ReplaceSubstringsWithIndexes(this string s, string startMarker, string endMarker, string replacementBeforeIndex, string replacementAfterIndex, out Dictionary<string, string> substringsAndReplacements, bool replaceMarkersToo = false, bool caseSensitive = true)
    {
        substringsAndReplacements = new Dictionary<string, string>();

        List<string> substrings = s.GetSubstrings(startMarker, endMarker, caseSensitive);
        substrings = substrings.RemoveDuplicates();
        var replacements = new List<string>();

        int index = 0;
        foreach(string p in substrings)
        {
            string replacement = replacementBeforeIndex + index + replacementAfterIndex;
            if(replaceMarkersToo)
            {
                replacement = startMarker + replacement + endMarker;
            }
            s = s.Replace(startMarker + p + endMarker, replacement);

            replacements.Add(replacement);
            index++;
        }

        for(int i = 0; i < substrings.Count; i++)
        {
            string substring = substrings[i];
            if(replaceMarkersToo)
            {
                substring = startMarker + substrings[i] + endMarker;
            }
            substringsAndReplacements.Add(substring, replacements[i]);
        }

        return s;
    }

    public static string ReplaceBackSubstringWithIndexes(this string s, Dictionary<string, string> substringsAndReplacements, string startMarker = null, string endMarker = null)
    {
        startMarker = startMarker ?? "";
        endMarker = endMarker ?? "";
        foreach(KeyValuePair<string, string> substringAndReplacement in substringsAndReplacements)
        {
            s = s.Replace(substringAndReplacement.Value, startMarker + substringAndReplacement.Key + endMarker);
        }
        return s;
    }

    public static string ReplaceWithIndexedString(this string s, string whatToReplace, string replacementBeforeIndex, string replacementAfterIndex, out List<KeyValuePair<string, string>> whatWasReplacedAndWithWhat, bool caseSensitive = true)
    {
        whatWasReplacedAndWithWhat = new List<KeyValuePair<string, string>>();

        List<int> locations = s.IndexesOfOccurrences(whatToReplace, caseSensitive);

        int j = 0;
        foreach(int location in locations.OrderByDescending(l => l))
        {
            string replaceWith = replacementBeforeIndex + j + replacementAfterIndex;

            string whatWasReplaced;
            s = s.Replace(location, location + whatToReplace.Length, replaceWith, out whatWasReplaced);

            whatWasReplacedAndWithWhat.Add(new KeyValuePair<string, string>(whatWasReplaced, replaceWith));
            j++;
        }

        return s;
    }

    public static string ReplaceBackIndexedStrings(this string s, List<KeyValuePair<string, string>> whatWasReplacedAndWithWhat)
    {
        foreach(KeyValuePair<string, string> replacement in whatWasReplacedAndWithWhat)
        {
            s = s.Replace(replacement.Value, replacement.Key);
        }

        return s;
    }

    public static string Replace(this string s, string whatToReplace, string replaceWith, bool caseSensitive = true)
    {
        List<int> locations = s.IndexesOfOccurrences(whatToReplace, caseSensitive);
        foreach(int location in locations)
        {
            s = s.Replace(location, location + whatToReplace.Length, replaceWith);
        }

        return s;
    }

    /// <summary>
    /// Returns string with replaced part. Returns null on error.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="partToReplaceStartIndex"></param>
    /// <param name="partToReplaceEndIndex"></param>
    /// <param name="replacementString"></param>
    /// <returns></returns>
    public static string Replace(this string s, int partToReplaceStartIndex, int partToReplaceEndIndex, string replacementString)
    {
        if(partToReplaceStartIndex >= s.Length || partToReplaceEndIndex >= s.Length ||
            partToReplaceStartIndex > partToReplaceEndIndex)
        {
            s = null;
        }
        else
        {
            string firstPart = s.Substring(0, partToReplaceStartIndex);
            string lastPart = s.Substring(partToReplaceEndIndex);
            s = firstPart + replacementString + lastPart;
        }
        return s;
    }

    /// <summary>
    /// Returns string with replaced part. Returns null on error.
    /// </summary>
    /// <param name="s"></param>
    /// <param name="partToReplaceStartIndex"></param>
    /// <param name="partToReplaceEndIndex"></param>
    /// <param name="replacementString"></param>
    /// <returns></returns>
    public static string Replace(this string s, int partToReplaceStartIndex, int partToReplaceEndIndex, string replacementString, out string replacedPart)
    {
        if(partToReplaceStartIndex >= s.Length || partToReplaceEndIndex >= s.Length ||
            partToReplaceStartIndex > partToReplaceEndIndex)
        {
            s = null;
            replacedPart = null;
        }
        else
        {
            string firstPart = s.Substring(0, partToReplaceStartIndex);
            replacedPart = s.Substring(partToReplaceStartIndex, partToReplaceEndIndex - partToReplaceStartIndex);
            string lastPart = s.Substring(partToReplaceEndIndex);
            s = firstPart + replacementString + lastPart;
        }
        return s;
    }

    public static string ReplaceIfNotPrefixedWith(this string s, List<string> prefixes, string whatToReplace, string replaceWith, bool caseSensitive = true, bool replaceIfWhatToReplaceIsAtBegining = false)
    {
        foreach(string prefix in prefixes)
        {
            //obfuscate prefix+whatToReplace
            var allReplacementsOfPrefixesWithWhatToReplaceNextToIt = new List<KeyValuePair<string, string>>();
            foreach(string p in prefixes)
            {
                List<KeyValuePair<string, string>> replacements;
                s = s.ReplaceWithIndexedString(p + whatToReplace, "[prefixAndWhatToReplace", "]", out replacements, caseSensitive);
                replacements.ToList().ForEach(r => allReplacementsOfPrefixesWithWhatToReplaceNextToIt.Add(new KeyValuePair<string, string>(r.Key, r.Value)));
            }

            //replace all whatToReplace that were left
            s = s.Replace(whatToReplace, replaceWith, caseSensitive);

            //obfuscate back prefix+whatToReplace
            s = s.ReplaceBackIndexedStrings(allReplacementsOfPrefixesWithWhatToReplaceNextToIt);
        }

        if(replaceIfWhatToReplaceIsAtBegining)
        {
            if(s.StartsWith(whatToReplace, caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
            {
                s = replaceWith + s.Substring(whatToReplace.Length);
            }
        }

        return s;
    }

    public static string ReplaceLastOccurrenceIfFound(string s, string whatToReplace, string replaceWith)
    {
        int foundAt = s.LastIndexOf(whatToReplace);
        if(foundAt >= 0)
        {
            s = s.Remove(foundAt, whatToReplace.Length).Insert(foundAt, replaceWith);
        }
        return s;
    }

    public static string Linkify(this string s, bool appendHttpToWwwIfNeccessary = true, bool detectAndPreserveExistingLinks = true)
    {
        Dictionary<string, string> existingLinks = null;
        if(detectAndPreserveExistingLinks)
        {
            s = s.ReplaceSubstringsWithIndexes("<a", "</a>", "[Link_", "]", out existingLinks, true, false);
        }

        if(appendHttpToWwwIfNeccessary)
        {
            var wwwPrefixesThatAbortAddingWww = new List<string>(); wwwPrefixesThatAbortAddingWww.AddRange("//,a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,x,y,z".ParseCSV());
            s = s.ReplaceIfNotPrefixedWith(wwwPrefixesThatAbortAddingWww, "www.", "http://www.", false, true);
        }
        s = Regex.Replace(s, @"((http|HTTP|https|HTTPS|ftp|FTP|ftps|FTPS)\://[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,3}(:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*)", @"<a target='_blank' href='$1'>$1</a>");

        if(detectAndPreserveExistingLinks)
        {
            s = s.ReplaceBackSubstringWithIndexes(existingLinks);
        }
        return s;
    }

    public static string RemoveSpaces(this string s)
    {
        if(s == null)
        {
            return s;
        }
        else
        {
            return s.Replace(" ", "");
        }
    }

    public static string RemoveRepeatedSpaces(this string s)
    {
        if(s == null)
        {
            return s;
        }
        else
        {
            string newS = s;
            do
            {
                s = newS;
                newS = s.Replace("  ", " ");
            } while(newS.Length != s.Length);

            return s;
        }
    }

    public static List<string> DistinctStrings(this List<string> items)
    {
        var r = new List<string>();
        foreach(string item in items)
        {
            if(!r.Contains(item))
            {
                r.Add(item);
            }
        }
        return r;
    }

    public static string PrepandHiddenHtmlZeros(this string number, int length)
    {
        string r = number;
        string zeros = string.Empty;

        while(r.Length + zeros.Length < length)
        {
            zeros += " ";
        }
        //zeros = string.Format("<span style=\"visibility:hidden\">{0}</span>", zeros);
        return zeros + number;
    }

    /// <summary>
    /// String length check is performed. I.e. “test”. SafeLeftSubstring(50) returns “test”
    /// </summary>
    /// <param name="data"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string SafeLeftSubstring(this string data, int length)
    {
        return string.IsNullOrEmpty(data) ? data : data.Substring(0, data.Length > length ? length : data.Length);
    }

    public static string ToProperStringDisplayValue(this decimal value, int decimalPlaces)
    {
        double d = (double)value;

        d = d * Math.Pow(10, decimalPlaces);
        d = Math.Truncate(d);
        d = d / Math.Pow(10, decimalPlaces);

        return string.Format("{0:N" + Math.Abs(decimalPlaces) + "}", d);
    }



    public static List<string> RemoveDuplicates(this List<string> list, bool ignoreCasing = false)
    {
        if(list != null)
        {
            var map = new Dictionary<string, object>();
            int i = 0;
            while(i < list.Count)
            {
                string current = list[i];
                if(ignoreCasing)
                {
                    current = current.ToLower();
                }

                if(map.ContainsKey(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<int> RemoveDuplicates(this List<int> list)
    {
        if(list != null)
        {
            var map = new Dictionary<int, object>();
            int i = 0;
            while(i < list.Count)
            {
                int current = list[i];

                if(map.ContainsKey(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<int?> RemoveDuplicates(this List<int?> list)
    {
        if(list != null)
        {
            var map = new Dictionary<int?, object>();
            int i = 0;
            while(i < list.Count)
            {
                int? current = list[i];

                if(map.ContainsKey(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<long> RemoveDuplicates(this List<long> list)
    {
        if(list != null)
        {
            var map = new Dictionary<long, object>();
            int i = 0;
            while(i < list.Count)
            {
                long current = list[i];

                if(map.ContainsKey(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<long?> RemoveDuplicates(this List<long?> list)
    {
        if(list != null)
        {
            var map = new Dictionary<long?, object>();
            int i = 0;
            while(i < list.Count)
            {
                long? current = list[i];

                if(map.ContainsKey(current))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<DateTime> RemoveDuplicates(this List<DateTime> list)
    {
        if(list != null)
        {
            var map = new Dictionary<DateTime, object>();
            int i = 0;
            while(i < list.Count)
            {
                DateTime current = list[i];

                if(map.Keys.Any(k => k.Ticks == current.Ticks))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static List<DateTime?> RemoveDuplicates(this List<DateTime?> list)
    {
        if(list != null)
        {
            var map = new Dictionary<DateTime?, object>();
            int i = 0;
            while(i < list.Count)
            {
                DateTime? current = list[i];

                if(map.Keys.Any(k => k.Value.Ticks == current.Value.Ticks))
                {
                    list.RemoveAt(i);
                }
                else
                {
                    i++;
                    map.Add(current, null);
                }
            }
        }

        return list;
    }

    public static int IndexOfNthOccurrence(this string s, string part, int N)
    {
        int offset = -1;
        for(int i = 1; i <= N; i++)
        {
            if(offset < 0)
            {
                offset = s.IndexOf(part);
            }
            else
            {
                offset = s.IndexOf(part, offset + 1);
            }
        }

        return offset;
    }

    public static List<int> IndexesOfOccurrences(this string s, string part, bool caseSensitive)
    {
        if(s == null)
        {
            throw new ArgumentNullException("s parameter is null");
        }

        if(part == null || part.Length == 0)
        {
            throw new Exception("part parameter is null or zero-length");
        }

        var r = new List<int>();

        if(!caseSensitive)
        {
            s = s.ToLower();
            part = part.ToLower();
        }

        int offset = -1;
        do
        {
            if(offset < 0)
            {
                offset = s.IndexOf(part);
            }
            else
            {
                if(offset + 1 < s.Length)
                {
                    offset = s.IndexOf(part, offset + 1);
                }
                else
                {
                    offset = -1;
                }
            }

            if(offset >= 0)
            {
                r.Add(offset);
            }
        } while(offset >= 0);

        return r;
    }

    /// <summary>
    /// Finds consecvutive words and returns first next index after them.
    ///  Example: for text.IndexAfterWords(true, "charset",ExtenderClass.OPTIONAL_SPACES,"=",ExtenderClass.OPTIONAL_SPACES,"\"") it will find all these:
    ///  'charset = "' 
    ///  'charset    = "'
    ///  'charset="'
    /// </summary>
    /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
    /// <param name="words">The words. You can use ExtenderClass.OPTIONAL_SPACES and ExtenderClass.OPTIONAL_SPACES as special constants.</param>
    /// <returns></returns>
    public static int IndexAfterWords(this string text, bool caseSensitive, params string[] words)
    {
        int index = -1;
        string origText = text;

        if(text.IsNOTNullOrWhiteSpace() && text.Length >= words.Sum(w => w.Length))
        {
            //if (caseSensitive == false)
            //{
            //	text = text.ToLower();
            //	for (int i = 0; i < words.Count(); i++)
            //	{
            //		words[i] = words[i].ToLower();
            //	}
            //}
            StringComparison sc = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            index = 0;
            while(index != -1)
            {
                index = origText.IndexOf(words[0], index, sc);
                if(index != -1)
                {
                    index += words[0].Length;
                    text = origText.Substring(index);
                    if(words.Count() > 1)
                    {
                        int index2 = index;
                        for(int i = 1; i < words.Count(); i++)
                        {
                            if(text != null && index2 != -1)
                            {
                                switch(words[i])
                                {
                                    case OPTIONAL_SPACES:
                                        text = text.TrimSpacesAtStartAndAdvanceIndex(ref index2, false);
                                        break;
                                    case REQUIRED_SPACES:
                                        text = text.TrimSpacesAtStartAndAdvanceIndex(ref index2, true);
                                        break;
                                    default:
                                        text = text.TrimXAtStartAndAdvanceIndex(words[i], ref index2, true, caseSensitive);
                                        break;
                                }
                            }
                        }

                        if(index2 != -1)
                        {//all words found
                            index = index2;
                            break;
                        }
                    }
                }
            }
        }

        return index;
    }

    public const string OPTIONAL_SPACES = "<({OS})>";
    public const string REQUIRED_SPACES = "<({RS})>";

    public static string TrimXAtStartAndAdvanceIndex(this string text, string X, ref int index, bool resetResultAndIndexIfNotFound, bool caseSensitive = true)
    {
        if(text != null)
        {
            StringComparison sc = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            if(text.StartsWith(X, sc))
            {
                text = text.Substring(X.Length);
                index += X.Length;
            }
            else
            {
                if(resetResultAndIndexIfNotFound)
                {
                    text = null;
                    index = -1;
                }
            }
        }

        return text;
    }

    public static string TrimSpacesAtStartAndAdvanceIndex(this string text, ref int index, bool resetResultAndIndexIfNotFound)
    {
        if(text != null)
        {
            string textTrimmed = text.TrimStart(' ');
            if(textTrimmed.Length == text.Length)
            {
                if(resetResultAndIndexIfNotFound)
                {
                    text = null;
                    index = -1;
                }
            }
            else
            {
                index += text.Length - textTrimmed.Length;
                text = textTrimmed;
            }
        }

        return text;
    }

    public static int OccurrencesCount(this string s, string keyword)
    {
        int offset = -1;
        int i = 0;
        while(i == 0 || offset >= 0)
        {
            if(offset < 0)
            {
                offset = s.IndexOf(keyword);
            }
            else
            {
                offset = s.IndexOf(keyword, offset + keyword.Length);
            }

            if(offset < 0)
            {
                break;
            }
            else
            {
                i++;
            }
        }

        return i;
    }

    public static string RemoveLastXChars(this string s, int x, string resultIfStringIsShorter = "")
    {
        if(s.Length < x)
        {
            return resultIfStringIsShorter;
        }
        else
        {
            return s.Remove(s.Length - x);
        }
    }

    public static string RemoveFirstXChars(this string s, int x, string resultIfStringIsShorter = "")
    {
        if(s.Length < x)
        {
            return resultIfStringIsShorter;
        }
        else
        {
            return s.Substring(x);
        }
    }

    public static string TrimStart(this string s, string stringToTrim, bool ignoreCasing = false, string nullCaseResult = null)
    {
        if(stringToTrim == null)
        {
            //leave unchanged
        }
        else
        {
            if(s == null)
            {
                s = nullCaseResult;
            }
            else
            {
                if(ignoreCasing)
                {
                    if(s.ToLower().StartsWith(stringToTrim))
                    {
                        s = s.Substring(stringToTrim.Length);
                    }
                }
                else
                {
                    if(s.StartsWith(stringToTrim))
                    {
                        s = s.Substring(stringToTrim.Length);
                    }
                }
            }
        }

        return s;
    }

    public static string TrimEnd(this string s, string stringToTrim, bool ignoreCasing = false, string nullCaseResult = null)
    {
        if(stringToTrim == null)
        {
            //leave unchanged
        }
        else
        {
            if(s == null)
            {
                s = nullCaseResult;
            }
            else
            {
                if(ignoreCasing)
                {
                    if(s.ToLower().EndsWith(stringToTrim))
                    {
                        s = s.Substring(0, s.Length - stringToTrim.Length);
                    }
                }
                else
                {
                    if(s.EndsWith(stringToTrim))
                    {
                        s = s.Substring(0, s.Length - stringToTrim.Length);
                    }
                }
            }
        }

        return s;
    }

    public static string TrimNonDigitsAtEnd(this string s)
    {
        while(s.Length > 0)
        {
            if(s[s.Length - 1] != '0' && s[s.Length - 1] != '1' && s[s.Length - 1] != '2' && s[s.Length - 1] != '3' && s[s.Length - 1] != '4' && s[s.Length - 1] != '5' &&
                s[s.Length - 1] != '6' && s[s.Length - 1] != '7' && s[s.Length - 1] != '8' && s[s.Length - 1] != '9')//is number?
            {
                s = s.Remove(s.Length - 1);//s = s.RemoveLastXChars(1);
            }
            else
            {
                break;
            }
        }

        return s;
    }

    public static List<string> ToLines(this string s, bool keepZeroLengthLines, bool keepWhitespaceLines, List<string> nullCaseResult)
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
                    if(r[i].IsNullOrWhiteSpace())
                    {
                        r.RemoveAt(i);
                    }
                }
            }
        }

        return r;
    }

    public static List<string> Split(this string s, string splitter, bool keepZeroLengthLines, bool keepWhitespaceLines, List<string> nullCaseResult)
    {
        List<string> r;

        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            StringSplitOptions sso = keepZeroLengthLines ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
            r = s.Split(new string[] { splitter }, sso).ToList();
            if(r.Count > 0 && !keepWhitespaceLines)
            {
                for(int i = r.Count - 1; i >= 0; i--)
                {
                    if(r[i].IsNullOrWhiteSpace())
                    {
                        r.RemoveAt(i);
                    }
                }
            }
        }

        return r;
    }

    public static List<string> Split(this string s, List<string> splitters, bool keepZeroLengthLines, bool keepWhitespaceLines, List<string> nullCaseResult)
    {
        List<string> r;

        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            StringSplitOptions sso = keepZeroLengthLines ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries;
            r = s.Split(splitters.ToArray(), sso).ToList();
            if(r.Count > 0 && !keepWhitespaceLines)
            {
                for(int i = r.Count - 1; i >= 0; i--)
                {
                    if(r[i].IsNullOrWhiteSpace())
                    {
                        r.RemoveAt(i);
                    }
                }
            }
        }

        return r;
    }

    public static bool AllLettersAreUppercase(this string s)
    {
        //for (int i = 0; i < s.Length; i++)
        //{
        //	if (Char.IsLetter(s[i]) && !Char.IsUpper(s[i]))
        //		return false;
        //}
        return s.All(c => !char.IsLetter(c) || char.IsUpper(c));
    }

    public static bool AllLettersAreLowercase(this string s)
    {
        //for (int i = 0; i < s.Length; i++)
        //{
        //	if (Char.IsLetter(s[i]) && !Char.IsUpper(s[i]))
        //		return false;
        //}
        return s.All(c => !char.IsLetter(c) || char.IsLower(c));
    }

    /// <summary>
    /// Capitalizes first letter and lowers remaining letters. Be careful -if first letter is not in A-Z like space nothing is capitalized.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToSentenceCase(this string s)
    {
        var sb1 = new StringBuilder(s.Length);
        int i = 0;
        foreach(char ch in s)
        {
            if(i == 0)
            {
                sb1.Append(char.ToUpper(ch));
            }
            else
            {
                sb1.Append(char.ToLower(ch));
            }
            i++;
        }
        return sb1.ToString();
    }

    /// <summary>
    /// In every word that starts with letter A-Z capitalizes that letter. Words a, an, the, at, by, for, in, of, on, to, up, and, as, but, or, nor are not capitalized if they are between spaces/words. If null is passed null is returned.
    /// Source: https://www.google.com/search?site=&source=hp&q=titles+capitalization+and+or&oq=titles+capitalization+and+or&gs_l=hp.3...3259.13356.0.13943.21.18.0.3.3.0.159.1910.4j13.17.0....0...1c.1.64.hp..1.19.1837.0Il_xsB1Ds0
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToTitleCase(this string s, bool preserveAcronyms = false, bool whenAllWordsAreCapitalizedNoneIsAcronym = true)
    {
        if(s == null)
        {
            return null;
        }
        else
        {
            var sb1 = new StringBuilder(s.Length);
            bool lastCharWasSpaceOrBeggining = true;
            int i = 0;
            foreach(char ch in s)
            {
                if(lastCharWasSpaceOrBeggining)
                {
                    sb1.Append(char.ToUpper(ch));
                }
                else
                {
                    sb1.Append(char.ToLower(ch));
                }
                lastCharWasSpaceOrBeggining = ch == ' ';
                i++;
            }
            string r = sb1.ToString();
            r = r.Replace(" A ", " a ");
            r = r.Replace(" An ", " an ");
            r = r.Replace(" The ", " the ");
            r = r.Replace(" At ", " at ");
            r = r.Replace(" By ", " by ");
            r = r.Replace(" For ", " for ");
            r = r.Replace(" In ", " in ");
            r = r.Replace(" Of ", " of ");
            r = r.Replace(" On ", " on ");
            r = r.Replace(" To ", " to ");
            r = r.Replace(" Up ", " up ");
            r = r.Replace(" And ", " and ");
            r = r.Replace(" As ", " as ");
            r = r.Replace(" But ", " but ");
            r = r.Replace(" Or ", " or ");
            r = r.Replace(" Nor ", " nor ");

            if(preserveAcronyms)
            {
                if(whenAllWordsAreCapitalizedNoneIsAcronym && s.AllLettersAreUppercase())
                {
                    //whole string is capitalized - no acronyms inside
                }
                else
                {
                    string[] originalWords = s.Split();
                    string[] sentenceCasedWords = r.Split();
                    var finalWords = new List<string>();
                    int j = 0;
                    foreach(string originalWord in originalWords)
                    {
                        if(originalWord.Length > 1 && originalWord.AllLettersAreUppercase())
                        {
                            //use acronym from original
                            finalWords.Add(originalWord);

                        }
                        else
                        {
                            //use already sentance-cased word
                            finalWords.Add(sentenceCasedWords[j]);
                        }
                        j++;
                    }
                    r = finalWords.ToSingleString(null, "", ' ');
                }
            }

            return r;
        }
    }

    public static string LowerFirstChar(this string s)
    {
        string r = s;
        if(!string.IsNullOrEmpty(r))
        {
            r = s[0].ToString().ToLower() + s.Remove(0, 1);
        }
        return r;
    }

    public static string LowerFirstCharOfEveryWord(this string s)
    {
        string r = string.Empty;
        string[] words;
        if(!string.IsNullOrEmpty(s))
        {
            words = s.Split();
            foreach(string word in words)
            {
                r = r + word[0].ToString().ToLower() + word.Remove(0, 1);
                r = r + " ";
            }
            r = r.Trim();
        }
        return r;
    }

    public static string UpperFirstChar(this string s)
    {
        string r = s;
        if(!string.IsNullOrEmpty(r))
        {
            r = s[0].ToString().ToUpper() + s.Remove(0, 1);
        }
        return r;
    }

    public static string UpperFirstCharOfEveryWord(this string s)
    {
        string r = string.Empty;
        string[] words;
        if(!string.IsNullOrEmpty(s))
        {
            words = s.Split();
            foreach(string word in words)
            {
                r = r + word[0].ToString().ToUpper() + word.Remove(0, 1);
                r = r + " ";
            }
            r = r.Trim();
        }
        return r;
    }

    /// <summary>
    /// For example from "Hello world!" makes "H e l l o  w o r l d !"
    /// </summary>
    public static string AddSpacesBetweenChars(this string s, bool addSpaceAfterSpaceToo = true)
    {
        string r;

        var sb1 = new StringBuilder();
        bool lastCharIsAppendedSpace = false;
        int i = 0;
        foreach(char ch in s)
        {
            sb1.Append(ch);
            lastCharIsAppendedSpace = false;
            if(ch != ' ')
            {
                sb1.Append(' ');
                lastCharIsAppendedSpace = true;
            }
            else
            {
                if(addSpaceAfterSpaceToo == true)
                {
                    sb1.Append(' ');
                    lastCharIsAppendedSpace = true;
                }
                else
                {

                }
            }

            i++;
        }
        r = sb1.ToString();

        if(lastCharIsAppendedSpace)
        {
            r = r.RemoveLastXChars(1);
        }

        return r;
    }

    public static bool ContainsDigit(this string s, bool nullCaseResult = false)
    {
        bool r = false;
        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            foreach(char c in s)
            {
                if(char.IsDigit(c))
                {
                    r = true;
                    break;
                }
            }
        }
        return r;
    }

    public static bool ContainsLetter(this string s, bool nullCaseResult = false)
    {
        bool r = false;
        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            foreach(char c in s)
            {
                if(char.IsLetter(c))
                {
                    r = true;
                    break;
                }
            }
        }
        return r;
    }

    public static bool ContainsCapitalLetter(this string s, bool nullCaseResult = false)
    {
        bool r = false;
        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            foreach(char c in s)
            {
                if(char.IsLetter(c) && char.IsUpper(c))
                {
                    r = true;
                    break;
                }
            }
        }
        return r;
    }

    public static bool ContainsNonLetterNonDigit(this string s, bool nullCaseResult = false)
    {
        bool r = false;
        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            foreach(char c in s)
            {
                if(!char.IsLetterOrDigit(c))
                {
                    r = true;
                    break;
                }
            }
        }
        return r;
    }

    public static string InsureProperDirectorySeparatorChar(this string s, string nullCaseResult = null)
    {
        string r;

        if(s == null)
        {
            r = nullCaseResult;
        }
        else
        {
            r = s.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        }

        return r;
    }
    #endregion

    #region CSV
    /// <summary>
    /// Splits string instance on every comma sign. 
    /// Does trimming all the way. 
    /// Returns list if resuting parts which does not include empty or whitespace items.
    /// On any error returns null.
    /// </summary>
    /// <param name="nullableString"></param>
    /// <returns></returns>
    public static List<string> ParseCSV(this string nullableString)
    {
        return ParseCSV<string>(nullableString, new char[] { ',' }, new char[] { ' ' }, false, null, null, null);
    }

    public static List<string> ParseCSV(this string nullableString, char[] separator)
    {
        return ParseCSV<string>(nullableString, separator, new char[] { ' ' }, false, null, null, null);
    }

    public static List<T> ParseCSV<T>(this string nullableString, char[] separator, char[] trimChars, bool includeEmptyOrWhiteSpaceItems, List<T> nullCaseResult, List<T> errorCaseResult, List<T> emptyStringCaseResult)
    {
        List<T> r = null;

        if(nullableString == null)
        {
            r = nullCaseResult;
        }
        else
        {
            try
            {
                string v = nullableString;
                if(trimChars != null)
                {
                    v = v.Trim(trimChars);
                }

                string[] parts = nullableString.Split(separator, StringSplitOptions.None);
                if(parts.Length == 1 && parts[0] == string.Empty)
                {
                    return emptyStringCaseResult;
                }
                else
                {
                    foreach(string part in parts)
                    {
                        string p = part;
                        if(trimChars != null)
                        {
                            p = p.Trim(trimChars);
                        }

                        if(!p.IsNullOrWhiteSpace() || includeEmptyOrWhiteSpaceItems)
                        {
                            if(r == null)
                            {
                                r = new List<T>();
                            }

                            var pAsT = (T)Convert.ChangeType((object)p, typeof(T));
                            r.Add(pAsT);
                        }
                    }
                }
            }
            catch(Exception)
            {
                r = errorCaseResult;
            }
        }

        return r;
    }

    /// <summary>
    /// Converts list of values to comma-separated-values (CSV) string. Empty and whitespace items are not included in resulting string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static string ToCSV<T>(this List<T> list)
    {
        return ToCSV<T>(list, false);
    }

    /// <summary>
    /// Converts list of values to comma-separated-values (CSV) string.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="includeEmptyOrWhiteSpaceItems"></param>
    /// <returns></returns>
    public static string ToCSV<T>(this List<T> list, bool includeEmptyOrWhiteSpaceItems)
    {
        return ToCSV<T>(list, ",", new char[] { ' ' }, includeEmptyOrWhiteSpaceItems, null, null);
    }

    /// <summary>
    /// Creates the CSV from a generic list.
    /// </summary>
    public static string ToCSV<T>(this List<T> list, string separator, char[] trimChars, bool includeEmptyOrWhiteSpaceItems, string nullCaseResult, string errorCaseResult)
    {
        string r = null;

        if(list == null)
        {
            r = nullCaseResult;
        }
        else
        {
            try
            {
                var sb = new StringBuilder(string.Empty);
                foreach(T? v in list)
                {
                    string item = string.Format("{0}", v);

                    if(trimChars != null)
                    {
                        item = item.Trim(trimChars);
                    }

                    if(!item.IsNullOrWhiteSpace() ||
                    item.IsNullOrWhiteSpace() && includeEmptyOrWhiteSpaceItems)
                    {
                        if(sb.Length > 0)
                        {
                            sb.Append(separator);
                        }
                        sb.Append(item);
                    }
                }
                r = sb.ToString();
            }
            catch(Exception)
            {
                r = errorCaseResult;
            }
        }

        return r;
    }

    public static string ToSingleString<T>(this IEnumerable<T> ss, string nullCaseResult = null, string zeroItemsCaseResult = "", char? separator = null)
    {
        string r;

        if(ss == null)
        {
            r = nullCaseResult;
        }
        else if(ss.Count() == 0)
        {
            r = zeroItemsCaseResult;
        }
        else
        {
            r = string.Empty;
            foreach(T s in ss)
            {
                r = r + s + separator ?? string.Empty;
            }
            if(separator != null)
            {
                r = r.TrimEnd(separator.Value);
            }
        }

        return r;
    }

    public static string ToSingleString<T>(this IEnumerable<T> ss, string nullCaseResult = null, string zeroItemsCaseResult = "", string separator = null)
    {
        string r;

        if(ss == null)
        {
            r = nullCaseResult;
        }
        else if(ss.Count() == 0)
        {
            r = zeroItemsCaseResult;
        }
        else
        {
            r = string.Empty;
            int i = 0;
            foreach(T s in ss)
            {
                r = r + s;
                if(i + 1 < ss.Count())
                {
                    r = r + separator ?? string.Empty;
                }
                i++;
            }
        }

        return r;
    }
    #endregion

    #region Numbers
    public static int Round(this decimal v)
    {
        return (int)Math.Round(v);
    }
    public static int Round(this float v)
    {
        return (int)Math.Round(v);
    }
    public static int Round(this double v)
    {
        return (int)Math.Round(v);
    }
    public static long RoundToLong(this decimal v)
    {
        return (long)Math.Round(v);
    }
    public static long RoundToLong(this float v)
    {
        return (long)Math.Round(v);
    }
    public static long RoundToLong(this double v)
    {
        return (long)Math.Round(v);
    }

    public static decimal Round(this decimal v, int decimals)
    {
        return (decimal)Math.Round(v, decimals);
    }
    public static float Round(this float v, int decimals)
    {
        return (float)Math.Round(v, decimals);
    }
    public static double Round(this double v, int decimals)
    {
        return (double)Math.Round(v, decimals);
    }

    #region LimitToRange
    public static bool IsBetweenButNotEqualTo(this int value, int min, int max)
    {
        return value > min && value < max;
    }

    public static bool IsBetweenOrEqualTo(this int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    public static bool IsNotBetweenNorEqualTo(this int value, int min, int max)
    {
        return value < min || value > max;
    }

    public static bool IsNotBetweenButMaybeEqualTo(this int value, int min, int max)
    {
        return value <= min && value >= max;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref byte value, byte min, byte max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref int value, int min, int max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref long value, long min, long max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref decimal value, decimal min, decimal max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref float value, float min, float max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }

    /// <summary>
    /// if first parameter is smaller than min it will be set to min. 
    /// If first parameter is bigger then max it will be set to max. 
    /// Returns true if value was changed.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static bool LimitToRange(ref double value, double min, double max)
    {
        if(value < min)
        {
            value = min;
            return true;
        }
        else if(value > max)
        {
            value = max;
            return true;
        }
        return false;
    }
    #endregion

    public static string GetDigitsAfterDecimalPoint(this decimal n, int numberOfDigits)
    {
        int result = decimal.ToInt32((decimal.Round(n, 2) - decimal.Floor(n)) * 100);
        return result > 9 ? result.ToString() : "0" + result;
    }

    public static string GetDigitsBeforeDecimalPoint(this decimal n)
    {
        //need to floor n first?
        return decimal.ToInt32(n).ToString();
    }

    public static int MiddleValue(int a, int b)
    {
        if(a == b)
        {
            return a;
        }
        if(a < b)
        {
            return a + ((b - a) / 2);
        }
        else
        {
            return b + ((a - b) / 2);
        }
    }

    /// <summary>
    /// For example from 23500444 returns 23,500,444
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    public static string PutCommaOnThousants(this int n)
    {
        string r = "";

        string r2 = n.ToString();
        int digitsAdded = 0;
        for(int i = r2.Length - 1; i >= 0; i--)
        {
            int rem; Math.DivRem(digitsAdded, 3, out rem);
            if(digitsAdded > 0 && rem == 0)
            {
                r = "," + r;
            }
            r = r2[i] + r;
            digitsAdded++;
        }

        return r;
    }
    #endregion

    #region Parameter
    /// <summary>
    /// Tells if for example in string: someCommand /parA:1 /ParB
    /// parameter ParA is present.
    /// </summary>
    /// <param name="line"></param>
    /// <param name="parameterName"></param>
    /// <param name="hideErrors"></param>
    /// <param name="resultInErrorCase"></param>
    /// <param name="parameterPrefix"></param>
    /// <param name="separatorBetweenKeyAndValue"></param>
    /// <returns></returns>
    public static bool GetParameterPresence(this string line, string parameterName, bool hideErrors, bool resultInErrorCase, char parameterPrefix = '/', char? separatorBetweenKeyAndValue = null)
    {
        bool found;

        try
        {
            line = line.Trim();

            if(parameterName.StartsWith(parameterPrefix.ToString()))
            {
                parameterName = parameterName.TrimStart(parameterPrefix);
            }

            string[] parts = line.SplitButDontBreakQuotedParts('"').ToArray();
            string parameterPart = null;
            if(parts.Length > 0)
            {
                string separator = string.Empty;
                if(separatorBetweenKeyAndValue != null)
                {
                    separator = separatorBetweenKeyAndValue.ToString();
                }
                parameterPart = parts.SingleOrDefault(p => p.ToLower().StartsWith(parameterPrefix.ToString().ToLower() + parameterName.ToLower() + separator.ToLower()));
            }

            found = !string.IsNullOrEmpty(parameterPart);
        }
        catch(Exception exception)
        {
            if(hideErrors)
            {
                found = resultInErrorCase;
            }
            else
            {
                var outerException = new Exception(string.Format("Can not read parameter '{0}' from command '{1}'.", parameterName, line.Split()[0]), exception);
                throw outerException;
            }
        }

        return found;
    }

    /// <summary>
    /// Example from: somecommand /paramNameA:1 /paramNameB
    /// extracts 1. Its not case sensitive.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="line"></param>
    /// <param name="throwErrorIfNotFound"></param>
    /// <param name="resultInNotFoundCase"></param>
    /// <param name="throwErrorIfEmptyString"></param>
    /// <param name="resultInEmptyStringCase"></param>
    /// <param name="hideErrors"></param>
    /// <param name="resultInErrorCase"></param>
    /// <param name="separatorBetweenKeyAndValue"></param>
    /// <returns></returns>
    public static T GetParameterValue<T>(this string line, string parameterName, bool throwErrorIfNotFound, T resultInNotFoundCase, bool throwErrorIfEmptyString, T resultInEmptyStringCase, bool hideErrors, T resultInErrorCase, char parameterPrefix = '/', char separatorBetweenKeyAndValue = ':', bool allowQuotedValue = false, char quoteChar = '"')
    {
        var r = default(T);

        try
        {
            line = line.Trim();
            bool found = false;

            if(parameterName.StartsWith(parameterPrefix.ToString()))
            {
                parameterName = parameterName.TrimStart(parameterPrefix);
            }

            List<string> parts = null;
            if(!allowQuotedValue)
            {
                parts = line.Split(' ').ToList();
            }
            else
            {
                parts = line.SplitButDontBreakQuotedParts(quoteChar);
            }
            string parameterPart = null;
            if(parts.Count > 0)
            {
                parameterPart = parts.SingleOrDefault(p => p.ToLower().StartsWith(parameterPrefix.ToString().ToLower() + parameterName.ToLower() + separatorBetweenKeyAndValue.ToString().ToLower()));
            }

            found = !string.IsNullOrEmpty(parameterPart);

            if(found == false)
            {
                if(!throwErrorIfNotFound)
                {
                    r = resultInNotFoundCase;
                }
                else
                {
                    string errorMessage;
                    if(resultInNotFoundCase == null)
                    {
                        errorMessage = string.Format("Missing parameter '{0}' from command '{1}'", parts[0]);
                    }
                    else
                    {
                        errorMessage = resultInEmptyStringCase.ToString();
                    }
                    throw new Exception(errorMessage);
                }
            }
            else
            {
                string value = parameterPart.Split(separatorBetweenKeyAndValue)[1];

                if(value == string.Empty)
                {
                    if(!throwErrorIfEmptyString)
                    {
                        r = resultInEmptyStringCase;
                    }
                    else
                    {
                        string errorMessage;
                        if(resultInEmptyStringCase == null)
                        {
                            errorMessage = string.Format("Empty value for parameter '{0}' in command '{1}' is not allowed.", parts[0]);
                        }
                        else
                        {
                            errorMessage = resultInErrorCase.ToString();
                        }
                        throw new Exception(errorMessage);
                    }
                }
                else
                {
                    if(allowQuotedValue && value.StartsWith(quoteChar.ToString()) && value.EndsWith(quoteChar.ToString()))
                    {
                        value = value.Substring(1);
                        value = value.RemoveLastXChars(1);
                    }
                    r = (T)Convert.ChangeType(value, typeof(T));
                }
            }
        }
        catch(Exception exception)
        {
            if(hideErrors)
            {
                r = resultInErrorCase;
            }
            else
            {
                var outerException = new Exception(string.Format("Can not read parameter '{0}' from command '{1}'.", parameterName, line.Split()[0]), exception);
                throw outerException;
            }
        }
        return r;
    }

    /// <summary>
    /// For example from: someCommand arg1 "arg2 with space" "arg3"
    /// returns list:
    ///   someCommand
    ///   arg1
    ///   arg2 with space
    ///   arg3
    /// </summary>
    /// <param name="line"></param>
    /// <param name="includeFirstWord"></param>
    /// <param name="leaveBrackets"></param>
    /// <param name="bracketChar"></param>
    /// <returns></returns>
    public static List<string> GetParameters(this string line, bool includeFirstWord = true, bool leaveBrackets = false, char bracketChar = '"')
    {
        List<string> r = null;
        string tempLine = string.Empty;

        //inside brackets replace spaces with special string

        bool bracketOpen = false;
        foreach(char c in line)
        {
            if(c == bracketChar)
            {
                bracketOpen = !bracketOpen;
                if(leaveBrackets)
                {
                    tempLine += c;
                }
            }
            else
            {
                if(c == ' ' && bracketOpen)
                {
                    tempLine += "[(<space>)]";
                }
                else
                {
                    tempLine += c;
                }
            }
        }

        r = tempLine.Split(' ').ToList();

        if(!includeFirstWord && r.Any())
        {
            r.RemoveAt(0);
        }

        for(int i = 0; i < r.Count; i++)
        {
            r[i] = r[i].Replace("[(<space>)]", " ");
        }

        return r;
    }

    /// <summary>
    /// For example from: arg1 "arg2 with space" "arg3"
    /// returns list:   
    ///   arg1
    ///   arg2 with space
    ///   arg3
    /// </summary>

    /// <returns></returns>
    public static List<string> SplitButDontBreakQuotedParts(this string line, char quoteChar = '"')
    {
        List<string> r = null;
        string tempLine = string.Empty;

        //inside quote replace spaces with special string

        bool quoteOpen = false;
        foreach(char c in line)
        {
            if(c == quoteChar)
            {
                quoteOpen = !quoteOpen;
                tempLine += c;
            }
            else
            {
                if(c == ' ' && quoteOpen)
                {
                    tempLine += "[(<space>)]";
                }
                else
                {
                    tempLine += c;
                }
            }
        }

        r = tempLine.Split(' ').ToList();

        for(int i = 0; i < r.Count; i++)
        {
            r[i] = r[i].Replace("[(<space>)]", " ");
        }

        return r;
    }

    public static string RemoveParameter(this string line, string parameterName, bool hideErrors, string resultInErrorCase, char parameterPrefix = '/', char bracketChar = '"', char? separatorBetweenKeyAndValue = null)
    {
        string newLine;

        try
        {
            if(parameterName.StartsWith(parameterPrefix.ToString()))
            {
                parameterName = parameterName.TrimStart(parameterPrefix);
            }

            List<string> parameters = GetParameters(line, true, true, bracketChar);

            var parametersMatched = parameters.Where(
                p => p.ToLower().StartsWith(parameterPrefix.ToString().ToLower() + parameterName.ToLower() + (separatorBetweenKeyAndValue == null ? "" : separatorBetweenKeyAndValue.Value.ToString().ToLower()))
                ).ToList();

            if(parametersMatched.Count() != 1)
            {
                throw new Exception("None or more than one matching parameter found in line.");
            }
            else
            {
                parameters.Remove(parametersMatched[0]);
            }

            newLine = string.Empty;
            foreach(string p in parameters)
            {
                newLine = newLine + " " + p;
            }
            newLine = newLine.Trim();
        }
        catch(Exception exception)
        {
            if(hideErrors)
            {
                newLine = resultInErrorCase;
            }
            else
            {
                var outerException = new Exception(string.Format("Can not read or remove parameter '{0}' from command '{1}'.", parameterName, line.Split()[0]), exception);
                throw outerException;
            }
        }

        return newLine;
    }
    #endregion

    #region KeyValuePair
    public static KeyValuePair<T1, T2> UpdateKey<T1, T2>(this KeyValuePair<T1, T2> pair, T1 newKey)
    {
        pair = new KeyValuePair<T1, T2>(newKey, pair.Value);
        return pair;
    }

    public static KeyValuePair<T1, T2> UpdateValue<T1, T2>(this KeyValuePair<T1, T2> pair, T2 newValue)
    {
        pair = new KeyValuePair<T1, T2>(pair.Key, newValue);
        return pair;
    }

    public static KeyValuePair<string, string> UpdateKey(this KeyValuePair<string, string> pair, string newKey)
    {
        pair = new KeyValuePair<string, string>(newKey, pair.Value);
        return pair;
    }

    public static KeyValuePair<string, string> UpdateValue(this KeyValuePair<string, string> pair, string newValue)
    {
        pair = new KeyValuePair<string, string>(pair.Key, newValue);
        return pair;
    }

    public static List<KeyValuePair<string, string>> RemoveDuplicates(this List<KeyValuePair<string, string>> list, bool compareByKey, bool compareByValue, bool caseSensitive = true)
    {
        var r = new List<KeyValuePair<string, string>>();
        StringComparison sc = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach(KeyValuePair<string, string> kvp in list)
        {
            bool alreadyAdded = false;

            if(compareByKey)
            {
                alreadyAdded = r.Exists(rkvp => string.Compare(rkvp.Key, kvp.Key, sc) == 0);
            }
            else if(compareByValue)
            {
                alreadyAdded = r.Exists(rkvp => string.Compare(rkvp.Value, kvp.Value, sc) == 0);
            }
            else if(compareByKey && compareByValue)
            {
                alreadyAdded = r.Exists(rkvp => string.Compare(rkvp.Key, kvp.Key, sc) == 0) && r.Exists(rkvp => string.Compare(rkvp.Value, kvp.Value, sc) == 0);
            }
            else
            {
                throw new Exception("Either compareByKey or compareByValue must be true or both can be true.");
            }

            if(!alreadyAdded)
            {
                r.Add(new KeyValuePair<string, string>(kvp.Key, kvp.Value));
            }
        }

        return r;
    }
    #endregion

    #region Dictionary
    public static void AddIfNotExist<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 value, bool constructDictIfNull = false)
    {
        if(dict == null && constructDictIfNull)
        {
            dict = new Dictionary<T1, T2>();
        }

        if(!dict.ContainsKey(key))
        {
            dict.Add(key, value);
        }
    }

    public static void AddOrUpdate<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 value, bool constructDictIfNull = false)
    {
        if(dict == null && constructDictIfNull)
        {
            dict = new Dictionary<T1, T2>();
        }

        if(!dict.ContainsKey(key))
        {
            dict.Add(key, value);
        }
        else
        {
            dict[key] = value;
        }
    }

    public static T2 GetByKey<T1, T2>(this Dictionary<T1, T2> dict, T1 key, T2 notFoundCaseResult, bool throwErrorIfDictIsNull = true, T2 dictIsNullResultCase = default(T2))
    {
        if(dict == null)
        {
            if(throwErrorIfDictIsNull)
            {
                throw new Exception($"Dict<{typeof(T1).Name.ToNonNullString("null")},{typeof(T1).Name.ToNonNullString("null")}> is null.");
            }
            else
            {
                return dictIsNullResultCase;
            }
        }
        else
        {
            if(!dict.ContainsKey(key))
            {
                return notFoundCaseResult;
            }
            else
            {
                return dict[key];
            }
        }
    }
    #endregion

    #region Byte array
    public static string ToCSVOfDecimals(this byte[] bytes)
    {
        string r = string.Empty;
        var sb = new StringBuilder();
        foreach(byte b in bytes)
        {
            sb.Append(System.Convert.ToDecimal(b));
            sb.Append(",");
        }
        r = sb.ToString().TrimEnd(',');
        return r;
    }

    public static byte[] FromCSVOfDecimals(this string csvOfDecimals)
    {
        var bytes = new List<byte>();

        try
        {
            foreach(string d in csvOfDecimals.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                bytes.Add(byte.Parse(d));
            }
        }
        catch
        {
            bytes = new List<byte>();
        }

        return bytes.ToArray();
    }

    /// <summary>
    /// Source: http://www.pvladov.com/2012/07/arbitrary-to-decimal-numeral-system.html
    /// </summary>
    /// <param name="number">The arbitrary numeral system number to convert.</param>
    /// <param name="baseA">The radix of the numeral system the given number
    /// is in (in the range [2, 36]).</param>
    /// <returns></returns>
    public static long FromDifferentBase(string number, string allPossibleDigits)//"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    {
        if(allPossibleDigits.Length < 2)
        {
            throw new ArgumentException("allPossibleDigits must contain at lease two digits.");
        }

        if(string.IsNullOrEmpty(number))
        {
            return 0;
        }

        // Make sure the arbitrary numeral system number is in upper case
        number = number.ToUpperInvariant();

        long result = 0;
        long multiplier = 1;
        for(int i = number.Length - 1; i >= 0; i--)
        {
            char c = number[i];
            if(i == 0 && c == '-')
            {
                // This is the negative sign symbol
                result = -result;
                break;
            }

            int digit = allPossibleDigits.IndexOf(c);
            if(digit == -1)
            {
                throw new ArgumentException("Invalid character in the arbitrary numeral system number", "number");
            }

            result += digit * multiplier;
            multiplier *= allPossibleDigits.Length;
        }

        return result;
    }

    public static string ToDifferentBase(this int value, string allPossibleDigits)
    {
        int i = 32;
        char[] buffer = new char[i];
        int targetBase = allPossibleDigits.Length;

        do
        {
            buffer[--i] = allPossibleDigits[value % targetBase];
            value = value / targetBase;
        }
        while(value > 0);

        char[] result = new char[32 - i];
        Array.Copy(buffer, i, result, 0, 32 - i);

        return new string(result);
    }

    public static string ToHex(this byte[] bytes)
    {
        var r = new StringBuilder(bytes.Length * 2);
        foreach(byte b in bytes)
        {
            r.AppendFormat("{0:x2}", b);
        }
        return r.ToString();
    }

    public static byte[] FromHex(this string hex)
    {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for(int i = 0; i < NumberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public static string ToAlphaOnlyHex(this byte[] bytes)
    {
        string allChars = "0123456789ABCDEFGHIJKLMNOP";
        string hex = ToHex(bytes);
        var alphaOnlyHex = new StringBuilder();
        foreach(char c in hex.ToUpper())
        {
            alphaOnlyHex.Append(allChars[allChars.IndexOf(c) + 10]);
        }
        return alphaOnlyHex.ToString();
    }

    public static byte[] FromAlphaOnlyHex(this string alphaOnlyHex)
    {
        string allChars = "0123456789ABCDEFGHIJKLMNOP";
        var hex = new StringBuilder();
        foreach(char c in alphaOnlyHex)
        {
            hex.Append(allChars[allChars.IndexOf(c) - 10]);
        }
        return FromHex(hex.ToString());
    }

    public static T ToStructure<T>(this byte[] bytes) where T : struct
    {
        var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
        var stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
        handle.Free();
        return stuff;
    }

    public static byte[] ReplaceStringInBytes(this byte[] source, string searchPhrase, string replacement, Encoding encoding)
    {
        byte[] searchPhraseAsUtf8Bytes = encoding.GetBytes(searchPhrase);
        byte[] replacementAsUtf8Bytes = encoding.GetBytes(replacement);
        byte[] r = ReplaceBytes(source, searchPhraseAsUtf8Bytes, replacementAsUtf8Bytes);
        return r;
    }

    public static byte[] ReplaceBytes(this byte[] source, byte[] searchPhrase, byte[] replacement)
    {
        byte[] r = null;

        List<int> matchedPositions = IndexesOfAllOccurrencesOfBytes(source, searchPhrase);
        if(matchedPositions.Count == 0)
        {
            r = new byte[source.Length];
            Buffer.BlockCopy(source, 0, r, 0, source.Length);
        }
        else
        {
            if(searchPhrase.Length <= replacement.Length)
            {
                r = new byte[source.Length + ((replacement.Length - searchPhrase.Length) * matchedPositions.Count)];
            }
            else
            {
                r = new byte[source.Length - ((searchPhrase.Length - replacement.Length) * matchedPositions.Count)];
            }

            int rPosition = 0;
            int sourcePosition = 0;
            foreach(int matchedPosition in matchedPositions)
            {
                if(sourcePosition > source.Length)
                {
                    break;
                }
                Buffer.BlockCopy(source, sourcePosition, r, rPosition, matchedPosition - sourcePosition);
                rPosition = rPosition + (matchedPosition - sourcePosition);
                sourcePosition = matchedPosition + searchPhrase.Length;


                Buffer.BlockCopy(replacement, 0, r, rPosition, replacement.Length);
                rPosition = rPosition + replacement.Length;
            }
            if(sourcePosition <= source.Length)
            {
                Buffer.BlockCopy(source, sourcePosition, r, rPosition, source.Length - sourcePosition);
            }
        }

        return r;
    }

    public static List<int> IndexesOfAllOccurrencesOfBytes(this byte[] source, byte[] searchPhrase)
    {
        var r = new List<int>();

        int i = -1;
        while(true)
        {
            i = IndexOfBytes(source, searchPhrase, i + 1, source.Length);
            if(i != -1)
            {
                r.Add(i);
            }
            else
            {
                break;
            }
        }

        return r;
    }

    /// <summary>
    /// Source: post at Thursday, March 31, 2011 5:08 AM at https://social.msdn.microsoft.com/Forums/vstudio/en-US/15514c1a-b6a1-44f5-a06c-9b029c4164d7/searching-a-byte-array-for-a-pattern-of-bytes?forum=csharpgeneral
    /// </summary>
    /// <param name="array"></param>
    /// <param name="pattern"></param>
    /// <param name="startIndex"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static int IndexOfBytes(byte[] array, byte[] pattern, int startIndex, int count)
    {
        if(array == null || array.Length == 0 || pattern == null || pattern.Length == 0 || count == 0)
        {
            return -1;
        }
        int i = startIndex;
        int endIndex = count > 0 ? Math.Min(startIndex + count, array.Length) : array.Length;
        int fidx = 0;
        int lastFidx = 0;

        while(i < endIndex)
        {
            lastFidx = fidx;
            fidx = (array[i] == pattern[fidx]) ? ++fidx : 0;
            if(fidx == pattern.Length)
            {
                return i - fidx + 1;
            }
            if(lastFidx > 0 && fidx == 0)
            {
                i = i - lastFidx;
                lastFidx = 0;
            }
            i++;
        }
        return -1;
    }
    #endregion

    #region Reflection
    /// <summary>
    /// Source: https://stackoverflow.com/questions/1196991/get-property-value-from-string-using-reflection-in-c-sharp
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <param name="propName"></param>
    /// <returns></returns>
    public static T GetPropertyValue<T>(this object obj, string propName)
    {
        var r = (T)obj.GetType().GetProperty(propName).GetValue(obj, null);
        return r;
    }

    /// <summary>
    /// Extension for 'Object' that copies the properties to a destination object.
    /// Source: https://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
    /// </summary>
    /// <param name="source">The source.</param>
    /// <param name="destination">The destination.</param>
    public static void CopyPropertiesTo(this object source, object destination)
    {
        // If any this null throw an exception
        if(source == null || destination == null)
        {
            throw new Exception("Source or/and Destination Objects are null");
        }
        // Getting the Types of the objects
        Type typeDest = destination.GetType();
        Type typeSrc = source.GetType();
        // Collect all the valid properties to map
        var results = from srcProp in typeSrc.GetProperties()
                      let targetProperty = typeDest.GetProperty(srcProp.Name)
                      where srcProp.CanRead
                      && targetProperty != null
                      && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                      && (targetProperty.GetSetMethod().Attributes & MethodAttributes.Static) == 0
                      && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                      select new { sourceProperty = srcProp, targetProperty = targetProperty };
        //map the properties
        foreach(var props in results)
        {
            props.targetProperty.SetValue(destination, props.sourceProperty.GetValue(source, null), null);
        }
    }

    public static List<string> GetAllPropertiesNames(this object o)
    {
        var r = o.GetType().GetProperties().Select(p => p.Name).ToList();
        return r;
    }
    #endregion

    #region Lists
    public static bool Contains(this List<string> list, string value, bool ignoreCase = false, bool compareTrimmedValues = false)
    {
        bool r = false;

        if(value != null)
        {
            if(ignoreCase)
            {
                value = value.ToLower();
            }
            if(compareTrimmedValues)
            {
                value = value.Trim();
            }
        }

        foreach(string s in list)
        {
            if(value == null && s == null)
            {
                r = true;
                break;
            }
            else if(s == null)
            {
                //skip to next
            }
            else
            {
                string s1 = s;
                if(ignoreCase)
                {
                    s1 = s1.ToLower();
                }
                if(compareTrimmedValues)
                {
                    s1 = s1.Trim();
                }

                if(s1 == value)
                {
                    r = true;
                    break;
                }
            }
        }

        return r;
    }
    #endregion
}
