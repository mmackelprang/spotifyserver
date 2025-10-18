using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Console.WriteLine("=== Spotify Server REST API Client Demo ===\n");

var apiBaseUrl = "http://localhost:5001/api";

Console.WriteLine("This demo application tests the Spotify Server REST API.");
Console.WriteLine($"API Base URL: {apiBaseUrl}");
Console.WriteLine("\nMake sure the Spotify Server API is running before using this client.");
Console.WriteLine("Start the API with: cd SpotifyServer.Api && dotnet run\n");

using var httpClient = new HttpClient();
httpClient.Timeout = TimeSpan.FromSeconds(30);

try
{
    // Demo: Search for tracks by name
    Console.WriteLine("=== Demo: Search for Tracks ===");
    Console.Write("Enter a song name to search (or press Enter to skip): ");
    var searchQuery = Console.ReadLine();
    
    if (!string.IsNullOrEmpty(searchQuery))
    {
        var encodedQuery = Uri.EscapeDataString(searchQuery);
        var searchUrl = $"{apiBaseUrl}/search/tracks/name/{encodedQuery}?limit=5";
        
        Console.WriteLine($"Calling: GET {searchUrl}");
        var response = await httpClient.GetAsync(searchUrl);
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var searchResult = JObject.Parse(content);
            var tracks = searchResult["tracks"]?["items"] as JArray;
            
            if (tracks != null && tracks.Count > 0)
            {
                Console.WriteLine($"\nFound {tracks.Count} results:");
                foreach (var track in tracks)
                {
                    var trackName = track["name"]?.ToString();
                    var artists = track["artists"] as JArray;
                    var artistNames = string.Join(", ", artists?.Select(a => a["name"]?.ToString()) ?? Array.Empty<string>());
                    var album = track["album"]?["name"]?.ToString();
                    var uri = track["uri"]?.ToString();
                    
                    Console.WriteLine($"  • {trackName} by {artistNames}");
                    Console.WriteLine($"    Album: {album}");
                    Console.WriteLine($"    URI: {uri}");
                }
            }
            else
            {
                Console.WriteLine("No tracks found.");
            }
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine(errorContent);
        }
    }

    // Demo: Get currently playing
    Console.WriteLine("\n=== Demo: Get Currently Playing ===");
    var currentlyPlayingUrl = $"{apiBaseUrl}/playback/currently-playing";
    
    Console.WriteLine($"Calling: GET {currentlyPlayingUrl}");
    var currentResponse = await httpClient.GetAsync(currentlyPlayingUrl);
    
    if (currentResponse.IsSuccessStatusCode)
    {
        var content = await currentResponse.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(content) || content == "null")
        {
            Console.WriteLine("No track currently playing.");
        }
        else
        {
            var currentlyPlaying = JObject.Parse(content);
            var item = currentlyPlaying["item"];
            
            if (item != null)
            {
                var trackName = item["name"]?.ToString();
                var artists = item["artists"] as JArray;
                var artistNames = string.Join(", ", artists?.Select(a => a["name"]?.ToString()) ?? Array.Empty<string>());
                var album = item["album"]?["name"]?.ToString();
                var durationMs = item["durationMs"]?.Value<int>() ?? 0;
                var progressMs = currentlyPlaying["progressMs"]?.Value<int>() ?? 0;
                
                Console.WriteLine($"Now Playing: {trackName} by {artistNames}");
                Console.WriteLine($"Album: {album}");
                Console.WriteLine($"Progress: {progressMs / 1000}s / {durationMs / 1000}s");
            }
            else
            {
                Console.WriteLine("No track currently playing.");
            }
        }
    }
    else if (currentResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("Unauthorized: This endpoint requires user authentication.");
        Console.WriteLine("Set SPOTIFY_REFRESH_TOKEN environment variable in the API.");
    }
    else
    {
        Console.WriteLine($"Error: {currentResponse.StatusCode}");
    }

    // Demo: Get playback state
    Console.WriteLine("\n=== Demo: Get Playback State ===");
    var playbackStateUrl = $"{apiBaseUrl}/playback/state";
    
    Console.WriteLine($"Calling: GET {playbackStateUrl}");
    var stateResponse = await httpClient.GetAsync(playbackStateUrl);
    
    if (stateResponse.IsSuccessStatusCode)
    {
        var content = await stateResponse.Content.ReadAsStringAsync();
        
        if (string.IsNullOrWhiteSpace(content) || content == "null")
        {
            Console.WriteLine("No playback state available.");
        }
        else
        {
            var playbackState = JObject.Parse(content);
            var shuffleState = playbackState["shuffleState"]?.Value<bool>() ?? false;
            var repeatState = playbackState["repeatState"]?.ToString() ?? "off";
            var device = playbackState["device"];
            var deviceName = device?["name"]?.ToString();
            var volume = device?["volumePercent"]?.Value<int>();
            
            Console.WriteLine($"Shuffle: {(shuffleState ? "ON" : "OFF")}");
            Console.WriteLine($"Repeat: {repeatState}");
            if (deviceName != null)
            {
                Console.WriteLine($"Device: {deviceName}");
            }
            if (volume.HasValue)
            {
                Console.WriteLine($"Volume: {volume}%");
            }
        }
    }
    else if (stateResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("Unauthorized: This endpoint requires user authentication.");
    }
    else
    {
        Console.WriteLine($"Error: {stateResponse.StatusCode}");
    }

    // Demo: Get available devices
    Console.WriteLine("\n=== Demo: Get Available Devices ===");
    var devicesUrl = $"{apiBaseUrl}/devices";
    
    Console.WriteLine($"Calling: GET {devicesUrl}");
    var devicesResponse = await httpClient.GetAsync(devicesUrl);
    
    if (devicesResponse.IsSuccessStatusCode)
    {
        var content = await devicesResponse.Content.ReadAsStringAsync();
        var devicesResult = JObject.Parse(content);
        var devices = devicesResult["devices"] as JArray;
        
        if (devices != null && devices.Count > 0)
        {
            Console.WriteLine($"Found {devices.Count} device(s):");
            foreach (var device in devices)
            {
                var name = device["name"]?.ToString();
                var type = device["type"]?.ToString();
                var isActive = device["isActive"]?.Value<bool>() ?? false;
                var id = device["id"]?.ToString();
                
                var activeStatus = isActive ? "ACTIVE" : "inactive";
                Console.WriteLine($"  • {name} ({type}) - {activeStatus}");
                Console.WriteLine($"    ID: {id}");
            }
        }
        else
        {
            Console.WriteLine("No devices found.");
        }
    }
    else if (devicesResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
    {
        Console.WriteLine("Unauthorized: This endpoint requires user authentication.");
    }
    else
    {
        Console.WriteLine($"Error: {devicesResponse.StatusCode}");
    }

    // Demo: Shuffle control
    Console.WriteLine("\n=== Demo: Shuffle Control ===");
    Console.Write("Do you want to test shuffle control? (y/n): ");
    var testShuffle = Console.ReadLine()?.ToLower();
    
    if (testShuffle == "y")
    {
        Console.Write("Enter shuffle state (on/off): ");
        var shuffleState = Console.ReadLine()?.ToLower();
        
        if (shuffleState == "on" || shuffleState == "off")
        {
            var shuffleUrl = $"{apiBaseUrl}/playback/shuffle/{shuffleState}";
            Console.WriteLine($"Calling: POST {shuffleUrl}");
            
            var shuffleResponse = await httpClient.PostAsync(shuffleUrl, null);
            
            if (shuffleResponse.IsSuccessStatusCode)
            {
                var content = await shuffleResponse.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                Console.WriteLine($"Success: {result["message"]}");
            }
            else if (shuffleResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized: This endpoint requires user authentication.");
            }
            else
            {
                Console.WriteLine($"Error: {shuffleResponse.StatusCode}");
                var errorContent = await shuffleResponse.Content.ReadAsStringAsync();
                Console.WriteLine(errorContent);
            }
        }
    }

    // Demo: Playback control
    Console.WriteLine("\n=== Demo: Playback Control ===");
    Console.WriteLine("Available playback controls:");
    Console.WriteLine("  1. Play/Pause");
    Console.WriteLine("  2. Next track");
    Console.WriteLine("  3. Previous track");
    Console.WriteLine("  4. Enable repeat (track)");
    Console.WriteLine("  5. Disable repeat");
    Console.Write("\nEnter choice (1-5) or press Enter to skip: ");
    
    var choice = Console.ReadLine();
    
    if (!string.IsNullOrEmpty(choice))
    {
        string? controlUrl = choice switch
        {
            "1" => $"{apiBaseUrl}/playback/pause",
            "2" => $"{apiBaseUrl}/playback/next",
            "3" => $"{apiBaseUrl}/playback/previous",
            "4" => $"{apiBaseUrl}/playback/repeat/track",
            "5" => $"{apiBaseUrl}/playback/repeat/off",
            _ => null
        };
        
        if (controlUrl != null)
        {
            Console.WriteLine($"Calling: POST {controlUrl}");
            var controlResponse = await httpClient.PostAsync(controlUrl, null);
            
            if (controlResponse.IsSuccessStatusCode)
            {
                var content = await controlResponse.Content.ReadAsStringAsync();
                var result = JObject.Parse(content);
                Console.WriteLine($"Success: {result["message"]}");
            }
            else if (controlResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                Console.WriteLine("Unauthorized: This endpoint requires user authentication.");
            }
            else
            {
                Console.WriteLine($"Error: {controlResponse.StatusCode}");
            }
        }
    }

    // Demo: Get recommendations
    Console.WriteLine("\n=== Demo: Get Recommendations ===");
    Console.Write("Enter a track ID to get recommendations (or press Enter to skip): ");
    var trackId = Console.ReadLine();
    
    if (!string.IsNullOrEmpty(trackId))
    {
        var recommendationsUrl = $"{apiBaseUrl}/recommendations/track/{trackId}?limit=10";
        Console.WriteLine($"Calling: GET {recommendationsUrl}");
        
        var recResponse = await httpClient.GetAsync(recommendationsUrl);
        
        if (recResponse.IsSuccessStatusCode)
        {
            var content = await recResponse.Content.ReadAsStringAsync();
            var recommendations = JObject.Parse(content);
            var tracks = recommendations["tracks"] as JArray;
            
            if (tracks != null && tracks.Count > 0)
            {
                Console.WriteLine($"\nGenerated {tracks.Count} recommendations:");
                foreach (var track in tracks)
                {
                    var trackName = track["name"]?.ToString();
                    var artists = track["artists"] as JArray;
                    var artistNames = string.Join(", ", artists?.Select(a => a["name"]?.ToString()) ?? Array.Empty<string>());
                    
                    Console.WriteLine($"  • {trackName} by {artistNames}");
                }
            }
        }
        else
        {
            Console.WriteLine($"Error: {recResponse.StatusCode}");
            var errorContent = await recResponse.Content.ReadAsStringAsync();
            Console.WriteLine(errorContent);
        }
    }

    Console.WriteLine("\n=== Available API Endpoints ===");
    Console.WriteLine("\nSearch Operations:");
    Console.WriteLine("  GET  /api/search/tracks?query={query}");
    Console.WriteLine("  GET  /api/search/tracks/name/{trackName}");
    Console.WriteLine("  GET  /api/search/tracks/artist/{artistName}");
    Console.WriteLine("  GET  /api/search/tracks/album/{albumName}");
    Console.WriteLine("\nPlaylist Operations:");
    Console.WriteLine("  GET  /api/playlists");
    Console.WriteLine("  GET  /api/playlists/{playlistId}/tracks");
    Console.WriteLine("\nPlayback Control:");
    Console.WriteLine("  POST /api/playback/play/track?trackUri={uri}");
    Console.WriteLine("  POST /api/playback/play/playlist?playlistUri={uri}");
    Console.WriteLine("  POST /api/playback/pause");
    Console.WriteLine("  POST /api/playback/resume");
    Console.WriteLine("  POST /api/playback/next");
    Console.WriteLine("  POST /api/playback/previous");
    Console.WriteLine("\nRepeat Control:");
    Console.WriteLine("  POST /api/playback/repeat/track");
    Console.WriteLine("  POST /api/playback/repeat/context");
    Console.WriteLine("  POST /api/playback/repeat/off");
    Console.WriteLine("\nShuffle Control:");
    Console.WriteLine("  POST /api/playback/shuffle/on");
    Console.WriteLine("  POST /api/playback/shuffle/off");
    Console.WriteLine("  POST /api/playback/shuffle?state={true|false}");
    Console.WriteLine("\nCurrently Playing:");
    Console.WriteLine("  GET  /api/playback/currently-playing");
    Console.WriteLine("  GET  /api/playback/state");
    Console.WriteLine("  GET  /api/devices");
    Console.WriteLine("\nRecommendations:");
    Console.WriteLine("  GET  /api/recommendations/track/{trackId}");
    Console.WriteLine("  POST /api/playback/radio/track/{trackId}");
    
    Console.WriteLine("\n✓ Demo completed successfully!");
    Console.WriteLine("\nNote: Endpoints marked with authentication requirement need SPOTIFY_REFRESH_TOKEN");
    Console.WriteLine("      to be set in the API server's environment variables.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"\n❌ Connection Error: {ex.Message}");
    Console.WriteLine("Make sure the Spotify Server API is running at http://localhost:5001");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
