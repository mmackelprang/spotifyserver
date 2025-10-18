using SpotifyServer;
using SpotifyAPI.Web;

Console.WriteLine("=== Spotify Server Demo ===\n");

// Check if credentials are provided via environment variables
var clientId = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
{
    Console.WriteLine("Spotify API credentials not found in environment variables.");
    Console.WriteLine("\nTo use this application, you need to:");
    Console.WriteLine("1. Create a Spotify application at https://developer.spotify.com/dashboard");
    Console.WriteLine("2. Set the following environment variables:");
    Console.WriteLine("   - SPOTIFY_CLIENT_ID: Your application's Client ID");
    Console.WriteLine("   - SPOTIFY_CLIENT_SECRET: Your application's Client Secret");
    Console.WriteLine("3. Configure redirect URI in your Spotify app: http://localhost:5000/callback");
    Console.WriteLine("\nExample usage (Linux/macOS):");
    Console.WriteLine("  export SPOTIFY_CLIENT_ID=\"your_client_id\"");
    Console.WriteLine("  export SPOTIFY_CLIENT_SECRET=\"your_client_secret\"");
    Console.WriteLine("\nExample usage (Windows):");
    Console.WriteLine("  set SPOTIFY_CLIENT_ID=your_client_id");
    Console.WriteLine("  set SPOTIFY_CLIENT_SECRET=your_client_secret");
    Console.WriteLine("\n=== Available Features ===");
    Console.WriteLine("✓ Search for songs by name, album, or artist");
    Console.WriteLine("✓ Get user playlists");
    Console.WriteLine("✓ Play songs or playlists");
    Console.WriteLine("✓ Generate radio stations from any song");
    Console.WriteLine("✓ Control playback (play/pause/next/previous)");
    Console.WriteLine("✓ Set repeat mode (track/context/off)");
    Console.WriteLine("✓ Get currently playing track metadata");
    Console.WriteLine("\nNote: Lyrics search is not available via Spotify API");
    return;
}

try
{
    // Create configuration
    var config = new SpotifyConfig
    {
        ClientId = clientId,
        ClientSecret = clientSecret
    };

    Console.WriteLine("Choose authentication method:");
    Console.WriteLine("1. Client Credentials (no user context - limited features)");
    Console.WriteLine("2. Authorization Code (full features - requires browser login)");
    Console.Write("\nEnter choice (1 or 2): ");
    
    var choice = Console.ReadLine();
    SpotifyClient? spotify = null;
    
    if (choice == "1")
    {
        Console.WriteLine("\nAuthenticating with Client Credentials...");
        spotify = await SpotifyService.CreateClientCredentialsClientAsync(clientId, clientSecret);
        Console.WriteLine("✓ Authenticated successfully (Client Credentials)");
        Console.WriteLine("\nNote: Playback features require user authentication (option 2)");
    }
    else if (choice == "2")
    {
        Console.WriteLine("\nStarting authentication flow...");
        Console.WriteLine("A browser window will open for you to log in to Spotify.");
        Console.WriteLine("Please authorize the application.");
        
        var authenticator = new SpotifyAuthenticator(config);
        spotify = await authenticator.AuthenticateAsync();
        Console.WriteLine("✓ Authenticated successfully!");
        
        if (!string.IsNullOrEmpty(config.RefreshToken))
        {
            Console.WriteLine($"\nYour refresh token (save this for future use):");
            Console.WriteLine(config.RefreshToken);
        }
    }
    else
    {
        Console.WriteLine("Invalid choice. Exiting.");
        return;
    }

    if (spotify == null)
    {
        Console.WriteLine("Failed to create Spotify client.");
        return;
    }

    var service = new SpotifyService(spotify);

    // Demo: Search for tracks
    Console.WriteLine("\n=== Demo: Search for Tracks ===");
    Console.Write("Enter a song name to search (or press Enter to skip): ");
    var searchQuery = Console.ReadLine();
    
    if (!string.IsNullOrEmpty(searchQuery))
    {
        var searchResults = await service.SearchByNameAsync(searchQuery, 5);
        
        if (searchResults.Tracks?.Items != null && searchResults.Tracks.Items.Count > 0)
        {
            Console.WriteLine($"\nFound {searchResults.Tracks.Items.Count} results:");
            foreach (var track in searchResults.Tracks.Items)
            {
                var artists = string.Join(", ", track.Artists.Select(a => a.Name));
                Console.WriteLine($"  • {track.Name} by {artists} (Album: {track.Album.Name})");
                Console.WriteLine($"    URI: {track.Uri}");
            }
        }
        else
        {
            Console.WriteLine("No tracks found.");
        }
    }

    // Demo: Get user playlists (only available with user authentication)
    if (choice == "2")
    {
        Console.WriteLine("\n=== Demo: User Playlists ===");
        try
        {
            var playlists = await service.GetUserPlaylistsAsync();
            Console.WriteLine($"Found {playlists.Count} playlists:");
            foreach (var playlist in playlists.Take(10))
            {
                Console.WriteLine($"  • {playlist.Name} ({playlist.Tracks?.Total ?? 0} tracks)");
                Console.WriteLine($"    URI: {playlist.Uri}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting playlists: {ex.Message}");
        }

        // Demo: Get currently playing
        Console.WriteLine("\n=== Demo: Currently Playing ===");
        try
        {
            var currentlyPlaying = await service.GetCurrentlyPlayingAsync();
            if (currentlyPlaying?.Item is FullTrack track)
            {
                var artists = string.Join(", ", track.Artists.Select(a => a.Name));
                Console.WriteLine($"Now Playing: {track.Name} by {artists}");
                Console.WriteLine($"Album: {track.Album.Name}");
                Console.WriteLine($"Duration: {track.DurationMs / 1000} seconds");
                Console.WriteLine($"Popularity: {track.Popularity}/100");
            }
            else
            {
                Console.WriteLine("No track currently playing.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting currently playing: {ex.Message}");
        }

        // Demo: Get available devices
        Console.WriteLine("\n=== Demo: Available Devices ===");
        try
        {
            var devices = await service.GetAvailableDevicesAsync();
            if (devices.Devices != null && devices.Devices.Count > 0)
            {
                Console.WriteLine($"Found {devices.Devices.Count} device(s):");
                foreach (var device in devices.Devices)
                {
                    var activeStatus = device.IsActive ? "ACTIVE" : "inactive";
                    Console.WriteLine($"  • {device.Name} ({device.Type}) - {activeStatus}");
                    Console.WriteLine($"    ID: {device.Id}");
                }
            }
            else
            {
                Console.WriteLine("No devices found. Please open Spotify on a device to see it here.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting devices: {ex.Message}");
        }

        // Demo: Recommendations (Radio)
        Console.WriteLine("\n=== Demo: Generate Radio Station ===");
        Console.Write("Enter a track ID to generate radio (or press Enter to skip): ");
        var trackId = Console.ReadLine();
        
        if (!string.IsNullOrEmpty(trackId))
        {
            try
            {
                var recommendations = await service.GetRecommendationsFromTrackAsync(trackId, 10);
                Console.WriteLine($"\nGenerated {recommendations.Tracks.Count} recommendations:");
                foreach (var recTrack in recommendations.Tracks)
                {
                    var artists = string.Join(", ", recTrack.Artists.Select(a => a.Name));
                    Console.WriteLine($"  • {recTrack.Name} by {artists}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating recommendations: {ex.Message}");
            }
        }
    }

    Console.WriteLine("\n=== Available Service Methods ===");
    Console.WriteLine("Search Operations:");
    Console.WriteLine("  - SearchByNameAsync(trackName)");
    Console.WriteLine("  - SearchByArtistAsync(artistName)");
    Console.WriteLine("  - SearchByAlbumAsync(albumName)");
    Console.WriteLine("\nPlaylist Operations:");
    Console.WriteLine("  - GetUserPlaylistsAsync()");
    Console.WriteLine("  - GetPlaylistTracksAsync(playlistId)");
    Console.WriteLine("\nPlayback Control:");
    Console.WriteLine("  - PlayTrackAsync(trackUri)");
    Console.WriteLine("  - PlayPlaylistAsync(playlistUri)");
    Console.WriteLine("  - PausePlaybackAsync()");
    Console.WriteLine("  - ResumePlaybackAsync()");
    Console.WriteLine("  - NextTrackAsync()");
    Console.WriteLine("  - PreviousTrackAsync()");
    Console.WriteLine("  - EnableTrackRepeatAsync()");
    Console.WriteLine("  - EnableContextRepeatAsync()");
    Console.WriteLine("  - DisableRepeatAsync()");
    Console.WriteLine("\nCurrently Playing:");
    Console.WriteLine("  - GetCurrentlyPlayingAsync()");
    Console.WriteLine("  - GetPlaybackStateAsync()");
    Console.WriteLine("  - GetAvailableDevicesAsync()");
    Console.WriteLine("\nRadio/Recommendations:");
    Console.WriteLine("  - GetRecommendationsFromTrackAsync(trackId)");
    Console.WriteLine("  - PlayRadioFromTrackAsync(trackId)");
    
    Console.WriteLine("\n✓ Demo completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
