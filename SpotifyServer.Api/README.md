# Spotify Server REST API

A RESTful API for controlling Spotify playback and managing playlists.

## Quick Start

1. **Set environment variables:**
   ```bash
   export SPOTIFY_CLIENT_ID="your_client_id"
   export SPOTIFY_CLIENT_SECRET="your_client_secret"
   export SPOTIFY_REFRESH_TOKEN="your_refresh_token"  # Optional
   ```

2. **Run the API:**
   ```bash
   dotnet run
   ```

3. **Access Swagger UI:**
   Open http://localhost:5001 in your browser

## Features

- ✅ Complete REST API for Spotify functionality
- ✅ OpenAPI/Swagger documentation
- ✅ Shuffle control (on/off)
- ✅ Search, playback, playlists
- ✅ Interactive documentation

## Documentation

See [API_DOCUMENTATION.md](../API_DOCUMENTATION.md) for complete API reference.

## Authentication

The API requires:
- `SPOTIFY_CLIENT_ID` - Your Spotify app client ID
- `SPOTIFY_CLIENT_SECRET` - Your Spotify app client secret
- `SPOTIFY_REFRESH_TOKEN` - (Optional) For user-authenticated features

To get a refresh token, run the SpotifyServer sample app and choose option 2.

## Endpoints

- Search: `/api/search/tracks/*`
- Playlists: `/api/playlists/*`
- Playback: `/api/playback/*`
- Shuffle: `/api/playback/shuffle/*`
- Recommendations: `/api/recommendations/*`
- Devices: `/api/devices`

See Swagger UI at http://localhost:5001 for interactive documentation.
