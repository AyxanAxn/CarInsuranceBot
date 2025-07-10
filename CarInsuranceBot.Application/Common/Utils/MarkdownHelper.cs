namespace CarInsuranceBot.Application.Common.Utils;

public static class MarkdownHelper
{
    /// <summary>
    /// Escapes special characters for Telegram Markdown parsing
    /// </summary>
    public static string EscapeMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Escape backslash first!
        text = text.Replace("\\", "\\\\");
        // Then escape all other MarkdownV2 special characters
        var charsToEscape = new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" };
        foreach (var c in charsToEscape)
        {
            text = text.Replace(c, "\\" + c);
        }
        return text;
    }

    /// <summary>
    /// Safely wraps text in backticks for code formatting, escaping any backticks in the content
    /// </summary>
    public static string SafeCodeBlock(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "``";
            
        // Replace backticks with escaped backticks
        var escapedText = text.Replace("`", "\\`");
        return $"`{escapedText}`";
    }

    /// <summary>
    /// Safely wraps text in bold formatting, escaping any asterisks in the content
    /// </summary>
    public static string SafeBold(string text)
    {
        if (string.IsNullOrEmpty(text))
            return "****";
            
        // Replace asterisks with escaped asterisks
        var escapedText = text.Replace("*", "\\*");
        return $"*{escapedText}*";
    }
} 