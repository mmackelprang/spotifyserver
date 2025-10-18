# Spotify Server

A modern C# service for integrating Spotify functionality into applications, built with .NET 9.0 and the actively maintained [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET) library.

## Features

✓ **REST API** ⭐ NEW
- Full-featured REST API with OpenAPI/Swagger documentation
- Control Spotify playback via HTTP endpoints
- Interactive API documentation at root endpoint

✓ **Search Functionality**
- Search for tracks by name, album, or artist
- Flexible search queries with support for filters

✓ **Playlist Management**
- Retrieve user playlists
- Get playlist track details

✓ **Playback Control**
- Play tracks or playlists
- Pause and resume playback
- Skip to next/previous tracks
- Control repeat modes (single track, context/playlist, or off)
- **Shuffle control** (enable/disable shuffle mode) ⭐ NEW

✓ **Radio/Recommendations**
- Generate "radio stations" based on seed tracks or artists
- Play recommended tracks directly

✓ **Currently Playing**
- Get metadata for currently playing tracks
- Retrieve playback state and device information

**Note**: Lyrics search is not available through the Spotify Web API.

## Prerequisites

1. **.NET 9.0 SDK** or later
2. **Spotify Developer Account** - Create one at [Spotify for Developers](https://developer.spotify.com/dashboard)
3. **Spotify Application** - Register your application to get Client ID and Client Secret

## Setup

### Option 1: Use the REST API (Recommended for most use cases)

The easiest way to use Spotify Server is through the REST API:

1. **Set environment variables** (see below)
2. **Run the API server:**
   ```bash
   cd SpotifyServer.Api
   dotnet run
   ```
3. **Access Swagger UI** at http://localhost:5001
4. **See [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for complete API guide**

### Option 2: Use as a Library

For programmatic access, use SpotifyServer as a library in your C# applications.

### 1. Create Spotify Application

1. Go to [Spotify Developer Dashboard](https://developer.spotify.com/dashboard)
2. Click "Create app"
3. Fill in the application details:
   - App name: Your application name
   - App description: Description of your application
   - Redirect URI: `http://localhost:5000/callback`
4. Save your **Client ID** and **Client Secret**

### 2. Configure Environment Variables

Set the following environment variables:

**Linux/macOS:**
```bash
export SPOTIFY_CLIENT_ID="your_client_id_here"
export SPOTIFY_CLIENT_SECRET="your_client_secret_here"
```

**Windows (Command Prompt):**
```cmd
set SPOTIFY_CLIENT_ID=your_client_id_here
set SPOTIFY_CLIENT_SECRET=your_client_secret_here
```

**Windows (PowerShell):**
```powershell
$env:SPOTIFY_CLIENT_ID="your_client_id_here"
$env:SPOTIFY_CLIENT_SECRET="your_client_secret_here"
```

### 3. Build and Run

```bash
cd SpotifyServer
dotnet build
dotnet run
```

## Usage

### Authentication Options

The application supports two authentication methods:

1. **Client Credentials** - Limited features, no user context
2. **Authorization Code** - Full features with user authentication (recommended)

### Example Code

```csharp
using SpotifyServer;
using SpotifyAPI.Web;

// Create configuration
var config = new SpotifyConfig
{
    ClientId = "your_client_id",
    ClientSecret = "your_client_secret"
};

// Authenticate using Authorization Code flow (requires browser)
var authenticator = new SpotifyAuthenticator(config);
var spotify = await authenticator.AuthenticateAsync();

// Create the service
var service = new SpotifyService(spotify);

// Search for tracks
var searchResults = await service.SearchByNameAsync("Song Name", limit: 10);

// Get user playlists
var playlists = await service.GetUserPlaylistsAsync();

// Play a track
await service.PlayTrackAsync("spotify:track:TRACK_ID");

// Control playback
await service.NextTrackAsync();
await service.PreviousTrackAsync();
await service.EnableTrackRepeatAsync();

// Generate and play radio from a track
await service.PlayRadioFromTrackAsync("TRACK_ID", limit: 50);

// Get currently playing
var currentlyPlaying = await service.GetCurrentlyPlayingAsync();
if (currentlyPlaying?.Item is FullTrack track)
{
    Console.WriteLine($"Now playing: {track.Name}");
}
```

## Available Service Methods

### Search Operations
- `SearchByNameAsync(trackName, limit)` - Search tracks by name
- `SearchByArtistAsync(artistName, limit)` - Search tracks by artist
- `SearchByAlbumAsync(albumName, limit)` - Search tracks by album
- `SearchTracksAsync(query, limit)` - General search with custom query

### Playlist Operations
- `GetUserPlaylistsAsync()` - Get current user's playlists
- `GetPlaylistTracksAsync(playlistId)` - Get tracks from a playlist

### Playback Control
- `PlayTrackAsync(trackUri, deviceId?)` - Play a specific track
- `PlayPlaylistAsync(playlistUri, deviceId?)` - Play a playlist
- `PausePlaybackAsync(deviceId?)` - Pause current playback
- `ResumePlaybackAsync(deviceId?)` - Resume playback
- `NextTrackAsync(deviceId?)` - Skip to next track
- `PreviousTrackAsync(deviceId?)` - Skip to previous track
- `EnableTrackRepeatAsync(deviceId?)` - Enable single track repeat
- `EnableContextRepeatAsync(deviceId?)` - Enable playlist/album repeat
- `DisableRepeatAsync(deviceId?)` - Disable repeat
- `EnableShuffleAsync(deviceId?)` - Enable shuffle mode ⭐ NEW
- `DisableShuffleAsync(deviceId?)` - Disable shuffle mode ⭐ NEW
- `SetShuffleAsync(state, deviceId?)` - Set shuffle mode on/off ⭐ NEW

### Currently Playing
- `GetCurrentlyPlayingAsync()` - Get currently playing track
- `GetPlaybackStateAsync()` - Get full playback state
- `GetAvailableDevicesAsync()` - Get available Spotify devices

### Radio/Recommendations
- `GetRecommendationsFromTrackAsync(trackId, limit)` - Get recommendations based on a track
- `GetRecommendationsFromTracksAsync(trackIds, limit)` - Get recommendations from multiple tracks
- `GetRecommendationsFromArtistsAsync(artistIds, limit)` - Get recommendations from artists
- `PlayRadioFromTrackAsync(trackId, limit, deviceId?)` - Play radio station from a track

## Dependencies

- **SpotifyAPI.Web** (7.2.1) - Main Spotify Web API client
- **SpotifyAPI.Web.Auth** (7.2.1) - Authentication support
- **Newtonsoft.Json** (13.0.3) - JSON serialization
- **EmbedIO** (3.5.2) - OAuth callback server

All dependencies are actively maintained and free of known security vulnerabilities.

## Library Information

This project uses [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET), a well-supported and actively developed open-source library for the Spotify Web API. The library:

- Supports .NET 5.0, 6.0, 7.0, 8.0, and 9.0
- Provides comprehensive access to Spotify's Web API
- Includes features for logging, retry handlers, and proxy support
- Is modular and easy to unit test

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Acknowledgments

- [SpotifyAPI-NET](https://github.com/JohnnyCrazy/SpotifyAPI-NET) by JohnnyCrazy
- [Spotify Web API](https://developer.spotify.com/documentation/web-api/) Documentation
