namespace SpotifyServer;

/// <summary>
/// Configuration for Spotify API credentials
/// </summary>
public class SpotifyConfig
{
    /// <summary>
    /// Spotify Client ID from your application dashboard
    /// </summary>
    public required string ClientId { get; set; }

    /// <summary>
    /// Spotify Client Secret from your application dashboard
    /// </summary>
    public required string ClientSecret { get; set; }

    /// <summary>
    /// Redirect URI configured in your Spotify application
    /// Default: http://localhost:5000/callback
    /// </summary>
    public string RedirectUri { get; set; } = "http://localhost:5000/callback";

    /// <summary>
    /// Refresh token for authenticated requests (obtained after initial auth)
    /// </summary>
    public string? RefreshToken { get; set; }
}
