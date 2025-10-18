using SpotifyAPI.Web;

namespace SpotifyServer;

/// <summary>
/// Service class providing comprehensive Spotify functionality
/// </summary>
public class SpotifyService
{
    private readonly SpotifyClient _spotify;

    public SpotifyService(SpotifyClient client)
    {
        _spotify = client ?? throw new ArgumentNullException(nameof(client));
    }

    #region Search Operations

    /// <summary>
    /// Search for tracks by name, artist, or album
    /// </summary>
    /// <param name="query">Search query (e.g., "track:Song Name", "artist:Artist Name", "album:Album Name")</param>
    /// <param name="limit">Maximum number of results to return (default: 20)</param>
    /// <returns>Search results containing tracks</returns>
    public async Task<SearchResponse> SearchTracksAsync(string query, int limit = 20)
    {
        var searchRequest = new SearchRequest(SearchRequest.Types.Track, query)
        {
            Limit = limit
        };
        return await _spotify.Search.Item(searchRequest);
    }

    /// <summary>
    /// Search for tracks by name
    /// </summary>
    public async Task<SearchResponse> SearchByNameAsync(string trackName, int limit = 20)
    {
        return await SearchTracksAsync($"track:{trackName}", limit);
    }

    /// <summary>
    /// Search for tracks by artist
    /// </summary>
    public async Task<SearchResponse> SearchByArtistAsync(string artistName, int limit = 20)
    {
        return await SearchTracksAsync($"artist:{artistName}", limit);
    }

    /// <summary>
    /// Search for tracks by album
    /// </summary>
    public async Task<SearchResponse> SearchByAlbumAsync(string albumName, int limit = 20)
    {
        return await SearchTracksAsync($"album:{albumName}", limit);
    }

    #endregion

    #region Playlist Operations

    /// <summary>
    /// Get the current user's playlists
    /// </summary>
    /// <returns>List of user playlists</returns>
    public async Task<List<FullPlaylist>> GetUserPlaylistsAsync()
    {
        var playlists = new List<FullPlaylist>();
        var firstPage = await _spotify.Playlists.CurrentUsers();
        
        await foreach (var playlist in _spotify.Paginate(firstPage))
        {
            playlists.Add(playlist);
        }
        
        return playlists;
    }

    /// <summary>
    /// Get tracks from a specific playlist
    /// </summary>
    public async Task<List<PlaylistTrack<IPlayableItem>>> GetPlaylistTracksAsync(string playlistId)
    {
        var tracks = new List<PlaylistTrack<IPlayableItem>>();
        var firstPage = await _spotify.Playlists.GetItems(playlistId);
        
        await foreach (var track in _spotify.Paginate(firstPage))
        {
            tracks.Add(track);
        }
        
        return tracks;
    }

    #endregion

    #region Playback Control

    /// <summary>
    /// Play a specific track by URI
    /// </summary>
    public async Task PlayTrackAsync(string trackUri, string? deviceId = null)
    {
        var request = new PlayerResumePlaybackRequest
        {
            Uris = new List<string> { trackUri }
        };
        
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.ResumePlayback(request);
    }

    /// <summary>
    /// Play a playlist by URI
    /// </summary>
    public async Task PlayPlaylistAsync(string playlistUri, string? deviceId = null)
    {
        var request = new PlayerResumePlaybackRequest
        {
            ContextUri = playlistUri
        };
        
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.ResumePlayback(request);
    }

    /// <summary>
    /// Pause playback
    /// </summary>
    public async Task PausePlaybackAsync(string? deviceId = null)
    {
        var request = new PlayerPausePlaybackRequest();
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.PausePlayback(request);
    }

    /// <summary>
    /// Resume playback
    /// </summary>
    public async Task ResumePlaybackAsync(string? deviceId = null)
    {
        var request = new PlayerResumePlaybackRequest();
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.ResumePlayback(request);
    }

    /// <summary>
    /// Skip to next track
    /// </summary>
    public async Task NextTrackAsync(string? deviceId = null)
    {
        var request = new PlayerSkipNextRequest();
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.SkipNext(request);
    }

    /// <summary>
    /// Skip to previous track
    /// </summary>
    public async Task PreviousTrackAsync(string? deviceId = null)
    {
        var request = new PlayerSkipPreviousRequest();
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.SkipPrevious(request);
    }

    /// <summary>
    /// Set repeat mode
    /// </summary>
    /// <param name="state">off, track, or context (playlist/album)</param>
    public async Task SetRepeatModeAsync(PlayerSetRepeatRequest.State state, string? deviceId = null)
    {
        var request = new PlayerSetRepeatRequest(state);
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.SetRepeat(request);
    }

    /// <summary>
    /// Enable repeat for a single track
    /// </summary>
    public async Task EnableTrackRepeatAsync(string? deviceId = null)
    {
        await SetRepeatModeAsync(PlayerSetRepeatRequest.State.Track, deviceId);
    }

    /// <summary>
    /// Enable repeat for a playlist or album context
    /// </summary>
    public async Task EnableContextRepeatAsync(string? deviceId = null)
    {
        await SetRepeatModeAsync(PlayerSetRepeatRequest.State.Context, deviceId);
    }

    /// <summary>
    /// Disable repeat
    /// </summary>
    public async Task DisableRepeatAsync(string? deviceId = null)
    {
        await SetRepeatModeAsync(PlayerSetRepeatRequest.State.Off, deviceId);
    }

    /// <summary>
    /// Enable shuffle mode
    /// </summary>
    public async Task EnableShuffleAsync(string? deviceId = null)
    {
        var request = new PlayerShuffleRequest(true);
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.SetShuffle(request);
    }

    /// <summary>
    /// Disable shuffle mode
    /// </summary>
    public async Task DisableShuffleAsync(string? deviceId = null)
    {
        var request = new PlayerShuffleRequest(false);
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.SetShuffle(request);
    }

    /// <summary>
    /// Set shuffle mode
    /// </summary>
    /// <param name="state">true to enable shuffle, false to disable</param>
    public async Task SetShuffleAsync(bool state, string? deviceId = null)
    {
        if (state)
        {
            await EnableShuffleAsync(deviceId);
        }
        else
        {
            await DisableShuffleAsync(deviceId);
        }
    }

    #endregion

    #region Currently Playing

    /// <summary>
    /// Get information about the currently playing track
    /// </summary>
    public async Task<CurrentlyPlaying?> GetCurrentlyPlayingAsync()
    {
        return await _spotify.Player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest());
    }

    /// <summary>
    /// Get full playback state including device info
    /// </summary>
    public async Task<CurrentlyPlayingContext?> GetPlaybackStateAsync()
    {
        return await _spotify.Player.GetCurrentPlayback();
    }

    /// <summary>
    /// Get available devices
    /// </summary>
    public async Task<DeviceResponse> GetAvailableDevicesAsync()
    {
        return await _spotify.Player.GetAvailableDevices();
    }

    #endregion

    #region Radio/Recommendations

    /// <summary>
    /// Generate recommendations (radio station) based on a seed track
    /// </summary>
    /// <param name="seedTrackId">Spotify track ID to use as seed</param>
    /// <param name="limit">Number of recommendations to generate (default: 20)</param>
    /// <returns>List of recommended tracks</returns>
    public async Task<RecommendationsResponse> GetRecommendationsFromTrackAsync(string seedTrackId, int limit = 20)
    {
        var request = new RecommendationsRequest
        {
            Limit = limit
        };
        request.SeedTracks.Add(seedTrackId);
        
        return await _spotify.Browse.GetRecommendations(request);
    }

    /// <summary>
    /// Generate recommendations based on multiple seed tracks
    /// </summary>
    public async Task<RecommendationsResponse> GetRecommendationsFromTracksAsync(
        List<string> seedTrackIds, 
        int limit = 20)
    {
        if (seedTrackIds == null || seedTrackIds.Count == 0)
        {
            throw new ArgumentException("At least one seed track ID is required", nameof(seedTrackIds));
        }
        
        var request = new RecommendationsRequest
        {
            Limit = limit
        };
        
        foreach (var trackId in seedTrackIds)
        {
            request.SeedTracks.Add(trackId);
        }
        
        return await _spotify.Browse.GetRecommendations(request);
    }

    /// <summary>
    /// Generate recommendations based on artists
    /// </summary>
    public async Task<RecommendationsResponse> GetRecommendationsFromArtistsAsync(
        List<string> seedArtistIds, 
        int limit = 20)
    {
        if (seedArtistIds == null || seedArtistIds.Count == 0)
        {
            throw new ArgumentException("At least one seed artist ID is required", nameof(seedArtistIds));
        }
        
        var request = new RecommendationsRequest
        {
            Limit = limit
        };
        
        foreach (var artistId in seedArtistIds)
        {
            request.SeedArtists.Add(artistId);
        }
        
        return await _spotify.Browse.GetRecommendations(request);
    }

    /// <summary>
    /// Play recommendations (radio) from a seed track
    /// </summary>
    public async Task PlayRadioFromTrackAsync(string seedTrackId, int limit = 50, string? deviceId = null)
    {
        var recommendations = await GetRecommendationsFromTrackAsync(seedTrackId, limit);
        var trackUris = recommendations.Tracks.Select(t => t.Uri).ToList();
        
        var request = new PlayerResumePlaybackRequest
        {
            Uris = trackUris
        };
        
        if (deviceId != null)
        {
            request.DeviceId = deviceId;
        }
        
        await _spotify.Player.ResumePlayback(request);
    }

    #endregion

    #region Authentication Helpers

    /// <summary>
    /// Create a Spotify client using Client Credentials flow (no user context)
    /// </summary>
    public static async Task<SpotifyClient> CreateClientCredentialsClientAsync(string clientId, string clientSecret)
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new ClientCredentialsRequest(clientId, clientSecret);
        var response = await new OAuthClient(config).RequestToken(request);

        return new SpotifyClient(config.WithToken(response.AccessToken));
    }

    /// <summary>
    /// Create a Spotify client using Authorization Code flow (with user context)
    /// Requires user to authenticate via browser
    /// </summary>
    public static SpotifyClient CreateAuthorizationCodeClient(string accessToken)
    {
        var config = SpotifyClientConfig.CreateDefault().WithToken(accessToken);
        return new SpotifyClient(config);
    }

    /// <summary>
    /// Create a Spotify client using a refresh token
    /// </summary>
    public static async Task<SpotifyClient> CreateFromRefreshTokenAsync(
        string clientId, 
        string clientSecret, 
        string refreshToken)
    {
        var config = SpotifyClientConfig.CreateDefault();
        var request = new AuthorizationCodeRefreshRequest(clientId, clientSecret, refreshToken);
        var response = await new OAuthClient(config).RequestToken(request);

        return new SpotifyClient(config.WithToken(response.AccessToken));
    }

    #endregion
}
