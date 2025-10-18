# Example Usage

This document provides examples of how to use the Spotify Server service.

## Basic Setup

```csharp
using SpotifyServer;
using SpotifyAPI.Web;

// Method 1: Client Credentials (no user context)
var spotify = await SpotifyService.CreateClientCredentialsClientAsync(
    "your_client_id", 
    "your_client_secret"
);
var service = new SpotifyService(spotify);

// Method 2: Authorization Code (full access with user context)
var config = new SpotifyConfig
{
    ClientId = "your_client_id",
    ClientSecret = "your_client_secret",
    RedirectUri = "http://localhost:5000/callback"
};

var authenticator = new SpotifyAuthenticator(config);
var authenticatedClient = await authenticator.AuthenticateAsync();
var userService = new SpotifyService(authenticatedClient);
```

## Search Examples

```csharp
// Search by track name
var trackResults = await service.SearchByNameAsync("Bohemian Rhapsody", limit: 5);
foreach (var track in trackResults.Tracks.Items)
{
    Console.WriteLine($"{track.Name} by {string.Join(", ", track.Artists.Select(a => a.Name))}");
}

// Search by artist
var artistResults = await service.SearchByArtistAsync("Queen", limit: 10);

// Search by album
var albumResults = await service.SearchByAlbumAsync("A Night at the Opera");

// Custom search query
var customResults = await service.SearchTracksAsync("track:Imagine artist:John Lennon");
```

## Playlist Operations

```csharp
// Get user's playlists (requires user authentication)
var playlists = await userService.GetUserPlaylistsAsync();
foreach (var playlist in playlists)
{
    Console.WriteLine($"Playlist: {playlist.Name} ({playlist.Tracks?.Total} tracks)");
}

// Get tracks from a specific playlist
var playlistTracks = await userService.GetPlaylistTracksAsync("playlist_id");
foreach (var item in playlistTracks)
{
    if (item.Track is FullTrack track)
    {
        Console.WriteLine($"  - {track.Name}");
    }
}
```

## Playback Control

```csharp
// Get available devices
var devices = await userService.GetAvailableDevicesAsync();
var deviceId = devices.Devices?.FirstOrDefault()?.Id;

// Play a specific track
await userService.PlayTrackAsync("spotify:track:3n3Ppam7vgaVa1iaRUc9Lp", deviceId);

// Play a playlist
await userService.PlayPlaylistAsync("spotify:playlist:37i9dQZF1DXcBWIGoYBM5M", deviceId);

// Playback controls
await userService.PausePlaybackAsync(deviceId);
await userService.ResumePlaybackAsync(deviceId);
await userService.NextTrackAsync(deviceId);
await userService.PreviousTrackAsync(deviceId);

// Repeat controls
await userService.EnableTrackRepeatAsync(deviceId);  // Repeat current track
await userService.EnableContextRepeatAsync(deviceId); // Repeat playlist/album
await userService.DisableRepeatAsync(deviceId);       // No repeat
```

## Currently Playing

```csharp
// Get currently playing track
var currentlyPlaying = await userService.GetCurrentlyPlayingAsync();
if (currentlyPlaying?.Item is FullTrack track)
{
    Console.WriteLine($"Now playing: {track.Name}");
    Console.WriteLine($"Artist: {string.Join(", ", track.Artists.Select(a => a.Name))}");
    Console.WriteLine($"Album: {track.Album.Name}");
    Console.WriteLine($"Duration: {TimeSpan.FromMilliseconds(track.DurationMs)}");
    Console.WriteLine($"Progress: {TimeSpan.FromMilliseconds(currentlyPlaying.ProgressMs ?? 0)}");
}

// Get full playback state
var playback = await userService.GetPlaybackStateAsync();
if (playback != null)
{
    Console.WriteLine($"Shuffle: {playback.ShuffleState}");
    Console.WriteLine($"Repeat: {playback.RepeatState}");
    Console.WriteLine($"Device: {playback.Device?.Name}");
    Console.WriteLine($"Volume: {playback.Device?.VolumePercent}%");
}
```

## Radio/Recommendations

```csharp
// Generate recommendations from a track
var recommendations = await service.GetRecommendationsFromTrackAsync(
    "3n3Ppam7vgaVa1iaRUc9Lp", // Track ID
    limit: 20
);

Console.WriteLine("Recommended tracks:");
foreach (var track in recommendations.Tracks)
{
    Console.WriteLine($"  - {track.Name} by {string.Join(", ", track.Artists.Select(a => a.Name))}");
}

// Generate from multiple tracks
var multiTrackRecs = await service.GetRecommendationsFromTracksAsync(
    new List<string> { "track_id_1", "track_id_2" },
    limit: 10
);

// Generate from artists
var artistRecs = await service.GetRecommendationsFromArtistsAsync(
    new List<string> { "artist_id_1", "artist_id_2" },
    limit: 15
);

// Play radio from a track (requires user authentication)
await userService.PlayRadioFromTrackAsync("3n3Ppam7vgaVa1iaRUc9Lp", limit: 50, deviceId);
```

## Error Handling

```csharp
try
{
    var results = await service.SearchByNameAsync("test");
}
catch (APIUnauthorizedException ex)
{
    Console.WriteLine("Authentication failed. Check your credentials.");
}
catch (APIException ex)
{
    Console.WriteLine($"Spotify API error: {ex.Response?.StatusCode} - {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Getting Spotify URIs and IDs

Spotify URIs and IDs can be obtained from:
1. The Spotify web player URL (e.g., `https://open.spotify.com/track/3n3Ppam7vgaVa1iaRUc9Lp`)
2. Search results (all items include `Id` and `Uri` properties)
3. The Spotify desktop/mobile app (Share → Copy Spotify URI)

```csharp
// From search results
var results = await service.SearchByNameAsync("Bohemian Rhapsody", 1);
var track = results.Tracks.Items.FirstOrDefault();
if (track != null)
{
    Console.WriteLine($"Track ID: {track.Id}");
    Console.WriteLine($"Track URI: {track.Uri}");
    
    // Use the URI to play the track
    await userService.PlayTrackAsync(track.Uri);
}
```

## Refresh Token Storage

For long-running applications, store the refresh token to avoid repeated authentication:

```csharp
var config = new SpotifyConfig
{
    ClientId = "your_client_id",
    ClientSecret = "your_client_secret",
    RefreshToken = "stored_refresh_token" // Load from secure storage
};

// If you have a refresh token, use it directly
if (!string.IsNullOrEmpty(config.RefreshToken))
{
    var spotify = await SpotifyService.CreateFromRefreshTokenAsync(
        config.ClientId,
        config.ClientSecret,
        config.RefreshToken
    );
    var service = new SpotifyService(spotify);
}
else
{
    // Otherwise, authenticate and save the refresh token
    var authenticator = new SpotifyAuthenticator(config);
    await authenticator.AuthenticateAsync();
    
    // Save config.RefreshToken to secure storage for next time
    SaveRefreshToken(config.RefreshToken);
}
```

## Complete Example Application

```csharp
using SpotifyServer;
using SpotifyAPI.Web;

public class Program
{
    public static async Task Main(string[] args)
    {
        var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");
        
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            Console.WriteLine("Please set SPOTIFY_CLIENT_ID and SPOTIFY_CLIENT_SECRET environment variables.");
            return;
        }
        
        // Authenticate
        var config = new SpotifyConfig
        {
            ClientId = clientId,
            ClientSecret = clientSecret
        };
        
        var authenticator = new SpotifyAuthenticator(config);
        var spotify = await authenticator.AuthenticateAsync();
        var service = new SpotifyService(spotify);
        
        // Search for a track
        Console.Write("Enter a song name to search: ");
        var query = Console.ReadLine();
        
        var results = await service.SearchByNameAsync(query!, 5);
        
        Console.WriteLine($"\nFound {results.Tracks?.Items?.Count ?? 0} results:");
        foreach (var track in results.Tracks?.Items ?? new List<FullTrack>())
        {
            Console.WriteLine($"  {track.Name} by {string.Join(", ", track.Artists.Select(a => a.Name))}");
        }
        
        // Get currently playing
        var currentlyPlaying = await service.GetCurrentlyPlayingAsync();
        if (currentlyPlaying?.Item is FullTrack currentTrack)
        {
            Console.WriteLine($"\nCurrently playing: {currentTrack.Name}");
            
            // Generate radio from currently playing track
            Console.WriteLine("\nGenerating radio station...");
            var recommendations = await service.GetRecommendationsFromTrackAsync(currentTrack.Id!, 10);
            
            Console.WriteLine("Recommended tracks:");
            foreach (var recTrack in recommendations.Tracks)
            {
                Console.WriteLine($"  - {recTrack.Name}");
            }
        }
        else
        {
            Console.WriteLine("\nNothing currently playing.");
        }
    }
}
```
