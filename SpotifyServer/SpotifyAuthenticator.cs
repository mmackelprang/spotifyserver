using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifyServer;

/// <summary>
/// Helper class for Spotify authentication flows
/// </summary>
public class SpotifyAuthenticator
{
    private readonly SpotifyConfig _config;
    private EmbedIOAuthServer? _server;

    public SpotifyAuthenticator(SpotifyConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    /// <summary>
    /// Start an interactive authentication flow that opens a browser for user login
    /// </summary>
    /// <returns>Authenticated Spotify client</returns>
    public async Task<SpotifyClient> AuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SpotifyClient>();
        
        // Parse the redirect URI and extract the base URI for the server
        // EmbedIOAuthServer automatically appends /callback, so we need to provide just the base URI
        var redirectUri = new Uri(_config.RedirectUri);
        var baseUri = new UriBuilder(redirectUri.Scheme, redirectUri.Host, redirectUri.Port).Uri;
        
        _server = new EmbedIOAuthServer(baseUri, baseUri.Port);
        await _server.Start();

        _server.AuthorizationCodeReceived += async (sender, response) =>
        {
            await _server.Stop();
            
            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
                new AuthorizationCodeTokenRequest(
                    _config.ClientId,
                    _config.ClientSecret,
                    response.Code,
                    new Uri(_config.RedirectUri)
                )
            );

            // Store the refresh token for future use
            _config.RefreshToken = tokenResponse.RefreshToken;
            
            var client = new SpotifyClient(tokenResponse.AccessToken);
            tcs.SetResult(client);
        };

        _server.ErrorReceived += async (sender, error, state) =>
        {
            await _server.Stop();
            tcs.SetException(new Exception($"Authentication error: {error}"));
        };

        var loginRequest = new LoginRequest(
            new Uri(_config.RedirectUri),
            _config.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = new List<string>
            {
                Scopes.UserReadEmail,
                Scopes.UserReadPrivate,
                Scopes.PlaylistReadPrivate,
                Scopes.PlaylistReadCollaborative,
                Scopes.UserReadPlaybackState,
                Scopes.UserModifyPlaybackState,
                Scopes.UserReadCurrentlyPlaying,
                Scopes.UserLibraryRead
            }
        };

        var uri_to_open = loginRequest.ToUri();
        try
        {
            BrowserUtil.Open(uri_to_open);
        }
        catch (Exception)
        {
            Console.WriteLine("Unable to open browser automatically.");
            Console.WriteLine($"Please open this URL in your browser: {uri_to_open}");
        }

        return await tcs.Task;
    }

    /// <summary>
    /// Stop the authentication server
    /// </summary>
    public async Task StopAsync()
    {
        if (_server != null)
        {
            await _server.Stop();
        }
    }
}
