using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace NetFusion.Identity.App.Implementations;

public class UrlEncoderService
{
    public string Encode(string value)
        => WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(value));

    public string Decode(string value)
        => Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(value));
}