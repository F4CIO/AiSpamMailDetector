using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MimeKit;

using CraftSynth.BuildingBlocks.Common;

namespace CraftSynth.AiSpamMailDetector.Engine;

public class EMailPreparator
{
    public static string GetEMailAsPlainText(string emlFilePath, out string from, out string to, out string subject, bool inCaseOfErrorReturnErrorMessage = true)
    {
        string r = "";

        try
        {
            // Load the .eml file
            var message = MimeMessage.Load(emlFilePath);

            from = "_";
            try
            {
                from = message.From.Mailboxes.FirstOrDefault()?.Address ?? "_";
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting From:{Path.GetFileName(emlFilePath)} Error: {ex.Message}");
                from = "error_";
            }
            from = from.ReplaceInvalidFileSystemCharacters("_");

            to = "_";
            try
            {
                to = message.To.Mailboxes.FirstOrDefault()?.Address ?? "_";
            }
            catch(Exception ex)
            {
                Debug.WriteLine($"Error getting To:{Path.GetFileName(emlFilePath)} Error: {ex.Message}");
                to = "error_";
            }
            to = to.ReplaceInvalidFileSystemCharacters("_");

            subject = message.Subject ?? "error_";

            //add headers to the result
            foreach(var header in message.Headers)
            {
                if(header.Value.Contains('\t'))
                {
                    header.Value = header.Value.Replace('\t', ' '); // Replace tabs with spaces
                }
                r += $"{header.Field}: {header.Value}\n";
            }

            if(r.Trim().Length > 0)
            {
                r += "\n"; // Add a newline after headers
            }

            // Check if the message has a text part
            foreach(var part in message.BodyParts)
            {
                if(part is TextPart textPart)
                {
                    // If it's HTML, convert it to plain text
                    if(textPart.IsHtml)
                    {
                        r += ConvertHtmlToPlainText(textPart.Text, true, textPart.Text);
                    }
                    r += textPart.Text;
                }
            }

            r = r.ReplaceSubstrings("<!DOCTYPE html", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<html", ">", "</html>");
            r = r.ReplaceSubstrings("<html", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</html", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<p", ">", "</p>");
            r = r.ReplaceSubstrings("<p", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</p", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<span", ">", "</span>");
            r = r.ReplaceSubstrings("<span", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</span", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<div", ">", "</div>");
            r = r.ReplaceSubstrings("<div", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</div", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<ul", ">", "</ul>");
            r = r.ReplaceSubstrings("<ul", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</ul", ">", "\n", false, true);

            r = ConvertHtmlElementsToPlainText(r, "<li", ">", "</li>");
            r = r.ReplaceSubstrings("<li", ">", "\n", false, true);
            r = r.ReplaceSubstrings("</li", ">", "\n", false, true);

            r = r.Replace('\t', ' ');
            while(r.Contains("  "))
            {
                r = r.Replace("  ", " ");
            }
            r = r.Replace('\r', '\n');
            while(r.Contains("\n \n"))
            {
                r = r.Replace("\n \n", "\n\n");
            }
            while(r.Contains("\n\n\n"))
            {
                r = r.Replace("\n\n\n", "\n\n");
            }
        }
        catch(Exception ex)
        {
            Debug.WriteLine($"Error processing EML file {emlFilePath}: {ex.Message}");
            if(inCaseOfErrorReturnErrorMessage)
            {
                from = "error_";
                to = "error_";
                subject = "error_";
                return "CraftSynth.AiSpamMailDetector: Error processing EML file: " + ex.Message+"\r\n"+ex.StackTrace;
            }
            else
            {
                throw;
            }
        }

        return r;
    }

    private static string ConvertHtmlElementsToPlainText(string r, string elementStart, string elementStartClosure, string elementEnd)
    {
        foreach(string substring in r.GetSubstrings(elementStart, elementEnd))
        {
            string s = substring;
            //delete part from beginning to first occurrence of ">"
            int index = substring.IndexOf(elementStartClosure);
            if(index >= 0)
            {
                s = s.Substring(index + 1); // Remove everything before and including the first '>'
            }
            r = r.Replace(substring, elementStartClosure + ConvertHtmlToPlainText(s, true, s));
        }

        return r;
    }

    private static string ConvertHtmlToPlainText(string html, bool surpressErrors, string resultInCaseOfAnError)
    {
        try
        {

            // Use UnDotNet.HtmlToText to convert HTML to plain text
            var converter = new UnDotNet.HtmlToText.HtmlToTextConverter();

            var r = converter.Convert(html);
            return r.Trim();
        }
        catch
        {
            if(surpressErrors)
            {
                return resultInCaseOfAnError;
            }
            else
            {
                throw;
            }
        }
    }
}
