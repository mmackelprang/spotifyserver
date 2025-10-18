# Spotify Server REST API

A RESTful API for controlling Spotify playback and managing playlists, built with ASP.NET Core and featuring OpenAPI/Swagger documentation.

## Features

✓ **Complete REST API** for all SpotifyServer functionality
✓ **OpenAPI/Swagger Documentation** - Interactive API documentation
✓ **Shuffle Control** - Turn song shuffling on/off
✓ **Search, Playback, Playlists** - Full Spotify integration
✓ **Test Client** - Sample application demonstrating API usage

## Quick Start

### 1. Set Environment Variables

The API requires Spotify credentials. You can provide them via environment variables or appsettings.json.

#### Option A: Environment Variables (Recommended)

**Linux/macOS:**
```bash
export SPOTIFY_CLIENT_ID="your_client_id_here"
export SPOTIFY_CLIENT_SECRET="your_client_secret_here"
export SPOTIFY_REFRESH_TOKEN="your_refresh_token_here"  # Optional, for user features
```

**Windows (PowerShell):**
```powershell
$env:SPOTIFY_CLIENT_ID="your_client_id_here"
$env:SPOTIFY_CLIENT_SECRET="your_client_secret_here"
$env:SPOTIFY_REFRESH_TOKEN="your_refresh_token_here"  # Optional
```

#### Option B: appsettings.json

Edit `SpotifyServer.Api/appsettings.json`:

```json
{
  "Spotify": {
    "ClientId": "your_client_id_here",
    "ClientSecret": "your_client_secret_here",
    "RefreshToken": "your_refresh_token_here"
  }
}
```

**Note:** Environment variables take precedence over appsettings.json.

**Note:** To get a refresh token, run the original SpotifyServer sample app with option 2 (Authorization Code). It will display your refresh token after authentication.

### 2. Run the API Server

```bash
cd SpotifyServer.Api
dotnet run
```

The API will start at `http://localhost:5001`

### 3. Access Swagger UI

Open your browser and navigate to: **http://localhost:5001**

The Swagger UI provides interactive documentation where you can test all API endpoints.

### 4. Run the Test Client (Optional)

In a separate terminal:

```bash
cd SpotifyServer.ApiClient
dotnet run
```

This interactive demo will test various API endpoints.

## API Endpoints

### Search Operations

- `GET /api/search/tracks?query={query}&limit={limit}`
  - Search for tracks using custom query
  
- `GET /api/search/tracks/name/{trackName}?limit={limit}`
  - Search tracks by name
  
- `GET /api/search/tracks/artist/{artistName}?limit={limit}`
  - Search tracks by artist
  
- `GET /api/search/tracks/album/{albumName}?limit={limit}`
  - Search tracks by album

### Playlist Operations

- `GET /api/playlists` 🔒
  - Get current user's playlists
  
- `GET /api/playlists/{playlistId}/tracks`
  - Get tracks from a playlist

### Playback Control

- `POST /api/playback/play/track?trackUri={uri}&deviceId={deviceId}` 🔒
  - Play a specific track
  
- `POST /api/playback/play/playlist?playlistUri={uri}&deviceId={deviceId}` 🔒
  - Play a playlist
  
- `POST /api/playback/pause?deviceId={deviceId}` 🔒
  - Pause playback
  
- `POST /api/playback/resume?deviceId={deviceId}` 🔒
  - Resume playback
  
- `POST /api/playback/next?deviceId={deviceId}` 🔒
  - Skip to next track
  
- `POST /api/playback/previous?deviceId={deviceId}` 🔒
  - Skip to previous track

### Repeat Control

- `POST /api/playback/repeat/track?deviceId={deviceId}` 🔒
  - Enable single track repeat
  
- `POST /api/playback/repeat/context?deviceId={deviceId}` 🔒
  - Enable playlist/album repeat
  
- `POST /api/playback/repeat/off?deviceId={deviceId}` 🔒
  - Disable repeat

### Shuffle Control ⭐ NEW

- `POST /api/playback/shuffle/on?deviceId={deviceId}` 🔒
  - Enable shuffle mode
  
- `POST /api/playback/shuffle/off?deviceId={deviceId}` 🔒
  - Disable shuffle mode
  
- `POST /api/playback/shuffle?state={true|false}&deviceId={deviceId}` 🔒
  - Set shuffle mode on or off

### Currently Playing

- `GET /api/playback/currently-playing` 🔒
  - Get currently playing track information
  
- `GET /api/playback/state` 🔒
  - Get full playback state (includes shuffle and repeat state)
  
- `GET /api/devices` 🔒
  - Get available Spotify devices

### Recommendations

- `GET /api/recommendations/track/{trackId}?limit={limit}`
  - Get track recommendations based on a seed track
  
- `POST /api/playback/radio/track/{trackId}?limit={limit}&deviceId={deviceId}` 🔒
  - Play recommendations based on a seed track

🔒 = Requires user authentication (SPOTIFY_REFRESH_TOKEN must be set)

## Usage Examples

### Using cURL

**Search for tracks:**
```bash
curl "http://localhost:5001/api/search/tracks/name/Bohemian%20Rhapsody?limit=5"
```

**Enable shuffle:**
```bash
curl -X POST "http://localhost:5001/api/playback/shuffle/on"
```

**Disable shuffle:**
```bash
curl -X POST "http://localhost:5001/api/playback/shuffle/off"
```

**Get currently playing:**
```bash
curl "http://localhost:5001/api/playback/currently-playing"
```

**Get playback state (includes shuffle state):**
```bash
curl "http://localhost:5001/api/playback/state"
```

**Play next track:**
```bash
curl -X POST "http://localhost:5001/api/playback/next"
```

### Using C# HttpClient

```csharp
using var httpClient = new HttpClient();
var apiBase = "http://localhost:5001/api";

// Search for tracks
var searchResponse = await httpClient.GetAsync(
    $"{apiBase}/search/tracks/name/Imagine?limit=5");
var searchResult = await searchResponse.Content.ReadAsStringAsync();

// Enable shuffle
var shuffleResponse = await httpClient.PostAsync(
    $"{apiBase}/playback/shuffle/on", null);

// Get playback state
var stateResponse = await httpClient.GetAsync(
    $"{apiBase}/playback/state");
var state = await stateResponse.Content.ReadAsStringAsync();
```

### Using JavaScript/Fetch

```javascript
const apiBase = 'http://localhost:5001/api';

// Search for tracks
const searchResults = await fetch(
  `${apiBase}/search/tracks/name/Wonderwall?limit=5`
).then(r => r.json());

// Enable shuffle
await fetch(`${apiBase}/playback/shuffle/on`, {
  method: 'POST'
});

// Get playback state
const state = await fetch(`${apiBase}/playback/state`)
  .then(r => r.json());
console.log('Shuffle:', state.shuffleState);
console.log('Repeat:', state.repeatState);
```

## Authentication

The API supports two authentication modes:

1. **Client Credentials** (Default)
   - Limited to search and recommendations
   - Set only `SPOTIFY_CLIENT_ID` and `SPOTIFY_CLIENT_SECRET`
   
2. **User Authentication** (Full Features)
   - Required for playback control, playlists, and user-specific features
   - Set `SPOTIFY_CLIENT_ID`, `SPOTIFY_CLIENT_SECRET`, and `SPOTIFY_REFRESH_TOKEN`

### Getting a Refresh Token

1. Run the original SpotifyServer sample application:
   ```bash
   cd SpotifyServer
   dotnet run
   ```

2. Choose option 2 (Authorization Code)

3. Complete the browser authentication

4. The refresh token will be displayed in the console

5. Set it as an environment variable for the API

## Response Formats

All responses are in JSON format matching the Spotify Web API schema.

### Success Response Example
```json
{
  "message": "Shuffle enabled"
}
```

### Error Response Example
```json
{
  "error": "No active device found"
}
```

## OpenAPI Documentation

The API includes comprehensive OpenAPI (Swagger) documentation:

- **Swagger UI**: http://localhost:5001
- **OpenAPI JSON**: http://localhost:5001/swagger/v1/swagger.json

The Swagger UI allows you to:
- Browse all available endpoints
- See request/response schemas
- Test endpoints directly from your browser
- Generate client code in various languages

## Building from Source

```bash
# Build the API
cd SpotifyServer.Api
dotnet build

# Build the test client
cd SpotifyServer.ApiClient
dotnet build
```

## Project Structure

```
spotifyserver/
├── SpotifyServer/              # Core library
│   ├── SpotifyService.cs       # Main service (with shuffle methods)
│   ├── SpotifyAuthenticator.cs
│   └── SpotifyConfig.cs
├── SpotifyServer.Api/          # REST API
│   ├── Program.cs              # API endpoints and configuration
│   └── SpotifyServer.Api.csproj
└── SpotifyServer.ApiClient/    # Test client
    ├── Program.cs              # Demo application
    └── SpotifyServer.ApiClient.csproj
```

## Dependencies

The REST API uses:
- **ASP.NET Core 9.0** - Web framework
- **Swashbuckle.AspNetCore 7.2.0** - OpenAPI/Swagger generation
- **Microsoft.AspNetCore.OpenApi 9.0.0** - OpenAPI support
- **SpotifyServer** - Core Spotify functionality

## Troubleshooting

### "Unauthorized" Errors
- Make sure `SPOTIFY_REFRESH_TOKEN` is set for user-authenticated endpoints
- Verify your credentials are correct

### "No active device found"
- Open Spotify on a device (desktop, mobile, web player)
- Make sure the device is not private/offline

### API Won't Start
- Check that port 5001 is not already in use
- Verify environment variables are set correctly
- Check the console for detailed error messages

## License

This project is licensed under the MIT License - see the LICENSE file for details.
