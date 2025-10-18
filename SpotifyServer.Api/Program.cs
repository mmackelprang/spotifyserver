using SpotifyServer;
using SpotifyAPI.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Spotify Server API",
        Version = "v1",
        Description = "A REST API for controlling Spotify playback and managing playlists",
        Contact = new OpenApiContact
        {
            Name = "Spotify Server",
            Url = new Uri("https://github.com/mmackelprang/spotifyserver")
        }
    });
});

// Register SpotifyService as a singleton
builder.Services.AddSingleton<SpotifyService>(sp =>
{
    var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
    var refreshToken = Environment.GetEnvironmentVariable("SPOTIFY_REFRESH_TOKEN");

    if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
    {
        throw new InvalidOperationException(
            "SPOTIFY_CLIENT_ID and SPOTIFY_CLIENT_SECRET environment variables must be set");
    }

    SpotifyClient spotify;
    
    if (!string.IsNullOrEmpty(refreshToken))
    {
        // Use refresh token if available
        spotify = SpotifyService.CreateFromRefreshTokenAsync(clientId, clientSecret, refreshToken).Result;
    }
    else
    {
        // Fall back to client credentials
        spotify = SpotifyService.CreateClientCredentialsClientAsync(clientId, clientSecret).Result;
    }

    return new SpotifyService(spotify);
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Spotify Server API v1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at root
});

app.UseHttpsRedirection();

// Search endpoints
app.MapGet("/api/search/tracks", async (SpotifyService service, string query, int limit = 20) =>
{
    var result = await service.SearchTracksAsync(query, limit);
    return Results.Ok(result);
})
.WithName("SearchTracks")
.WithOpenApi(operation => new(operation)
{
    Summary = "Search for tracks",
    Description = "Search for tracks using a custom query (e.g., 'track:Song Name artist:Artist Name')"
});

app.MapGet("/api/search/tracks/name/{trackName}", async (SpotifyService service, string trackName, int limit = 20) =>
{
    var result = await service.SearchByNameAsync(trackName, limit);
    return Results.Ok(result);
})
.WithName("SearchTracksByName")
.WithOpenApi(operation => new(operation)
{
    Summary = "Search tracks by name",
    Description = "Search for tracks by track name"
});

app.MapGet("/api/search/tracks/artist/{artistName}", async (SpotifyService service, string artistName, int limit = 20) =>
{
    var result = await service.SearchByArtistAsync(artistName, limit);
    return Results.Ok(result);
})
.WithName("SearchTracksByArtist")
.WithOpenApi(operation => new(operation)
{
    Summary = "Search tracks by artist",
    Description = "Search for tracks by artist name"
});

app.MapGet("/api/search/tracks/album/{albumName}", async (SpotifyService service, string albumName, int limit = 20) =>
{
    var result = await service.SearchByAlbumAsync(albumName, limit);
    return Results.Ok(result);
})
.WithName("SearchTracksByAlbum")
.WithOpenApi(operation => new(operation)
{
    Summary = "Search tracks by album",
    Description = "Search for tracks by album name"
});

// Playlist endpoints
app.MapGet("/api/playlists", async (SpotifyService service) =>
{
    try
    {
        var playlists = await service.GetUserPlaylistsAsync();
        return Results.Ok(playlists);
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetUserPlaylists")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get user playlists",
    Description = "Get the current user's playlists (requires user authentication)"
});

app.MapGet("/api/playlists/{playlistId}/tracks", async (SpotifyService service, string playlistId) =>
{
    try
    {
        var tracks = await service.GetPlaylistTracksAsync(playlistId);
        return Results.Ok(tracks);
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetPlaylistTracks")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get playlist tracks",
    Description = "Get tracks from a specific playlist"
});

// Playback control endpoints
app.MapPost("/api/playback/play/track", async (SpotifyService service, string trackUri, string? deviceId = null) =>
{
    try
    {
        await service.PlayTrackAsync(trackUri, deviceId);
        return Results.Ok(new { message = "Track playback started" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("PlayTrack")
.WithOpenApi(operation => new(operation)
{
    Summary = "Play a track",
    Description = "Play a specific track by URI (requires user authentication)"
});

app.MapPost("/api/playback/play/playlist", async (SpotifyService service, string playlistUri, string? deviceId = null) =>
{
    try
    {
        await service.PlayPlaylistAsync(playlistUri, deviceId);
        return Results.Ok(new { message = "Playlist playback started" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("PlayPlaylist")
.WithOpenApi(operation => new(operation)
{
    Summary = "Play a playlist",
    Description = "Play a playlist by URI (requires user authentication)"
});

app.MapPost("/api/playback/pause", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.PausePlaybackAsync(deviceId);
        return Results.Ok(new { message = "Playback paused" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("PausePlayback")
.WithOpenApi(operation => new(operation)
{
    Summary = "Pause playback",
    Description = "Pause current playback (requires user authentication)"
});

app.MapPost("/api/playback/resume", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.ResumePlaybackAsync(deviceId);
        return Results.Ok(new { message = "Playback resumed" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("ResumePlayback")
.WithOpenApi(operation => new(operation)
{
    Summary = "Resume playback",
    Description = "Resume current playback (requires user authentication)"
});

app.MapPost("/api/playback/next", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.NextTrackAsync(deviceId);
        return Results.Ok(new { message = "Skipped to next track" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("NextTrack")
.WithOpenApi(operation => new(operation)
{
    Summary = "Next track",
    Description = "Skip to next track (requires user authentication)"
});

app.MapPost("/api/playback/previous", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.PreviousTrackAsync(deviceId);
        return Results.Ok(new { message = "Skipped to previous track" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("PreviousTrack")
.WithOpenApi(operation => new(operation)
{
    Summary = "Previous track",
    Description = "Skip to previous track (requires user authentication)"
});

// Repeat control endpoints
app.MapPost("/api/playback/repeat/track", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.EnableTrackRepeatAsync(deviceId);
        return Results.Ok(new { message = "Track repeat enabled" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("EnableTrackRepeat")
.WithOpenApi(operation => new(operation)
{
    Summary = "Enable track repeat",
    Description = "Enable repeat for current track (requires user authentication)"
});

app.MapPost("/api/playback/repeat/context", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.EnableContextRepeatAsync(deviceId);
        return Results.Ok(new { message = "Context repeat enabled" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("EnableContextRepeat")
.WithOpenApi(operation => new(operation)
{
    Summary = "Enable context repeat",
    Description = "Enable repeat for playlist/album (requires user authentication)"
});

app.MapPost("/api/playback/repeat/off", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.DisableRepeatAsync(deviceId);
        return Results.Ok(new { message = "Repeat disabled" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("DisableRepeat")
.WithOpenApi(operation => new(operation)
{
    Summary = "Disable repeat",
    Description = "Disable repeat mode (requires user authentication)"
});

// Shuffle control endpoints
app.MapPost("/api/playback/shuffle/on", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.EnableShuffleAsync(deviceId);
        return Results.Ok(new { message = "Shuffle enabled" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("EnableShuffle")
.WithOpenApi(operation => new(operation)
{
    Summary = "Enable shuffle",
    Description = "Enable shuffle mode (requires user authentication)"
});

app.MapPost("/api/playback/shuffle/off", async (SpotifyService service, string? deviceId = null) =>
{
    try
    {
        await service.DisableShuffleAsync(deviceId);
        return Results.Ok(new { message = "Shuffle disabled" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("DisableShuffle")
.WithOpenApi(operation => new(operation)
{
    Summary = "Disable shuffle",
    Description = "Disable shuffle mode (requires user authentication)"
});

app.MapPost("/api/playback/shuffle", async (SpotifyService service, bool state, string? deviceId = null) =>
{
    try
    {
        await service.SetShuffleAsync(state, deviceId);
        return Results.Ok(new { message = $"Shuffle {(state ? "enabled" : "disabled")}" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("SetShuffle")
.WithOpenApi(operation => new(operation)
{
    Summary = "Set shuffle mode",
    Description = "Set shuffle mode on or off (requires user authentication)"
});

// Currently playing endpoints
app.MapGet("/api/playback/currently-playing", async (SpotifyService service) =>
{
    try
    {
        var currentlyPlaying = await service.GetCurrentlyPlayingAsync();
        return Results.Ok(currentlyPlaying);
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetCurrentlyPlaying")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get currently playing track",
    Description = "Get information about the currently playing track (requires user authentication)"
});

app.MapGet("/api/playback/state", async (SpotifyService service) =>
{
    try
    {
        var playbackState = await service.GetPlaybackStateAsync();
        return Results.Ok(playbackState);
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetPlaybackState")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get playback state",
    Description = "Get full playback state including device info (requires user authentication)"
});

app.MapGet("/api/devices", async (SpotifyService service) =>
{
    try
    {
        var devices = await service.GetAvailableDevicesAsync();
        return Results.Ok(devices);
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
})
.WithName("GetAvailableDevices")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get available devices",
    Description = "Get available Spotify devices (requires user authentication)"
});

// Recommendations endpoints
app.MapGet("/api/recommendations/track/{trackId}", async (SpotifyService service, string trackId, int limit = 20) =>
{
    try
    {
        var recommendations = await service.GetRecommendationsFromTrackAsync(trackId, limit);
        return Results.Ok(recommendations);
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("GetRecommendationsFromTrack")
.WithOpenApi(operation => new(operation)
{
    Summary = "Get recommendations from track",
    Description = "Generate recommendations based on a seed track"
});

app.MapPost("/api/playback/radio/track/{trackId}", async (SpotifyService service, string trackId, int limit = 50, string? deviceId = null) =>
{
    try
    {
        await service.PlayRadioFromTrackAsync(trackId, limit, deviceId);
        return Results.Ok(new { message = "Radio playback started" });
    }
    catch (APIUnauthorizedException)
    {
        return Results.Unauthorized();
    }
    catch (APIException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("PlayRadioFromTrack")
.WithOpenApi(operation => new(operation)
{
    Summary = "Play radio from track",
    Description = "Play recommendations based on a seed track (requires user authentication)"
});

Console.WriteLine("Starting Spotify Server API...");
Console.WriteLine("Swagger UI available at: http://localhost:5001");
Console.WriteLine("\nRequired environment variables:");
Console.WriteLine("  - SPOTIFY_CLIENT_ID");
Console.WriteLine("  - SPOTIFY_CLIENT_SECRET");
Console.WriteLine("  - SPOTIFY_REFRESH_TOKEN (optional, for user-authenticated features)");

app.Run("http://localhost:5001");
