namespace NetFusion.Identity.Domain.Authentication.Entities;

/// <summary>
/// Entity containing a generated application specific JWT security token.
/// </summary>
public class TokenStatus
{
    /// <summary>
    /// The generated token.  Will be null if the generation failed.
    /// </summary>
    public string? Token { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="token">The generated JTW token.</param>
    public TokenStatus(string token)
    {
        Token = token;
    }
}