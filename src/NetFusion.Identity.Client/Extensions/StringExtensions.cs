namespace NetFusion.Identity.Client.Extensions;

public static class StringExtensions
{
    public static string SplitIntoParts(this string value, int numberOfParts, string separator = " ")
    {
        if (numberOfParts < 1) throw new ArgumentException("Must be greater or equal to one.", nameof(numberOfParts));
        if (string.IsNullOrEmpty(value)) throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        var parts = Enumerable.Range(0, value.Length / numberOfParts)
            .Select(i => value.Substring(i * numberOfParts, numberOfParts));

        return string.Join(separator, parts);
    }
}