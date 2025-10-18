# Spotify Server API Client

A test/demo application for the Spotify Server REST API.

## Purpose

This application demonstrates how to interact with the Spotify Server REST API using HTTP requests. It provides examples for:

- Searching for tracks
- Getting currently playing information
- Controlling playback (play, pause, next, previous)
- Managing shuffle mode
- Getting recommendations

## Usage

1. **Start the API server first:**
   ```bash
   cd ../SpotifyServer.Api
   dotnet run
   ```

2. **In a new terminal, run the client:**
   ```bash
   cd ../SpotifyServer.ApiClient
   dotnet run
   ```

3. **Follow the interactive prompts** to test various API endpoints

## Features Demonstrated

- ✅ Track search
- ✅ Currently playing information
- ✅ Playback state (shuffle, repeat)
- ✅ Available devices
- ✅ Shuffle control
- ✅ Playback control
- ✅ Recommendations

## Code Examples

The client uses standard `HttpClient` to make requests:

```csharp
// Search for tracks
var response = await httpClient.GetAsync(
    $"{apiBaseUrl}/search/tracks/name/{trackName}?limit=5");

// Enable shuffle
var shuffleResponse = await httpClient.PostAsync(
    $"{apiBaseUrl}/playback/shuffle/on", null);

// Get playback state
var stateResponse = await httpClient.GetAsync(
    $"{apiBaseUrl}/playback/state");
```

## Requirements

- Spotify Server API must be running at http://localhost:5001
- API must have valid Spotify credentials configured

## Documentation

See [API_DOCUMENTATION.md](../API_DOCUMENTATION.md) for complete API reference.
