using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Oauth2.v2;
using Google.Apis.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Net.Http;
using Google.Apis.Drive.v3.Data;
using System.IO;
using ExcelClone.Constants;

namespace ExcelClone.Services.GoogleDrive;

public class GoogleDriveService : IGoogleDriveService
{
    private static string _windowsClientId = Secrets.windowsGoogleDriveClientId; // UWP
    private static string _authURL = "https://accounts.google.com/o/oauth2/v2/auth";

    private static HttpListener? _listener = null;
    Oauth2Service? _oauth2Service;
    DriveService? _driveService;
    GoogleCredential? _credential;
    string? _email;

    public bool IsSignedIn => _credential != null;
    public string? Email => _email;

    public async Task Init()
    {
        var hasRefreshToken = await SecureStorage.GetAsync("refresh_token") is not null;
        if (!IsSignedIn && hasRefreshToken)
        {
            await SignIn();
        }
    }

    public async Task SignIn()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var expiresIn = Preferences.Get("access_token_epires_in", 0L);
        var isExpired = now > expiresIn - 10; // 10 second buffer
        var hasRefreshToken = await SecureStorage.GetAsync("refresh_token") is not null;

        if (isExpired && hasRefreshToken)
        {
            Trace.TraceInformation("Using refresh token");
            await RefreshToken();
        }
        else if (isExpired) // No refresh token
        {
            Trace.TraceInformation("Starting auth code flow");
            if (DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                await DoAuthCodeFlowWindows();
            }
            else
            {
                throw new NotImplementedException($"Auth flow for platform {DeviceInfo.Current.Platform} not implemented");
            }
        }

        var accesToken = await SecureStorage.GetAsync("access_token");
        _credential = GoogleCredential.FromAccessToken(accesToken);
        _oauth2Service = new Oauth2Service(new BaseClientService.Initializer
        {
            HttpClientInitializer = _credential,
            ApplicationName = "ExcelCloneUniversityProject"
        });
        _driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = _credential,
            ApplicationName = "ExcelCloneUniversityProject"
        });
        var userInfo = await _oauth2Service.Userinfo.Get().ExecuteAsync();
        _email = userInfo.Email;
    }

    public async Task<List<Google.Apis.Drive.v3.Data.File>> GetFiles(string? fileExtension = null)
    {
        var request = _driveService!.Files.List();
        var fileList = await request.ExecuteAsync();
        var files = new List<Google.Apis.Drive.v3.Data.File>();

        if (fileList.Files != null && fileList.Files.Count > 0)
        {
            foreach (var file in fileList.Files)
            {
                if (fileExtension is not null && !(file.FileExtension == fileExtension || file.Name.EndsWith(fileExtension)))
                {
                    continue;
                }
                files.Add(file);
            }
        }

        return files;
    }

    public async Task<string> DownloadFileAsync(string fileId)
    {
        if (_driveService is null)
            throw new InvalidOperationException("Google Drive not initialized or not signed in");

        using var memoryStream = new MemoryStream();

        var request = _driveService.Files.Get(fileId);
        await request.DownloadAsync(memoryStream);

        memoryStream.Position = 0;

        using var reader = new StreamReader(memoryStream);
        return await reader.ReadToEndAsync();
    }
    
    public async Task<Google.Apis.Drive.v3.Data.File> UploadFileAsync(string fileName, Stream content, string mimeType)
    {
        if (_driveService is null)
            throw new InvalidOperationException("Google Drive not initialized or not signed in");

        var fileMetadata = new Google.Apis.Drive.v3.Data.File
        {
            Name = fileName
        };

        var request = _driveService.Files.Create(fileMetadata, content, mimeType);
        request.Fields = "id, name, mimeType, size, createdTime, modifiedTime";
        await request.UploadAsync();

        var uploadedFile = request.ResponseBody;
        if (uploadedFile == null)
            throw new HttpRequestException("File upload failed: response was null");

        return uploadedFile;
    }

    public async Task SignOut()
    {
        await RevokeTokens();
    }

    private static async Task DoAuthCodeFlowWindows()
    {
        var authUrl = _authURL;
        var clientId = _windowsClientId;
        var localPort = 42135;
        var redirectUri = $"http://localhost:{localPort}";
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var parameters = GenerateAuthParameters(redirectUri, clientId, codeChallenge);
        var queryString = string.Join("&", parameters.Select(
            param => $"{WebUtility.UrlEncode(param.Key)}={WebUtility.UrlEncode(param.Value)}"
        ));
        var fullAuthUrl = $"{authUrl}?{queryString}";

        await Launcher.OpenAsync(fullAuthUrl);
        var authorizationCode = await StartLocalHttpServerAsync(localPort);

        await GetInitialToken(authorizationCode, redirectUri, clientId, codeVerifier);
    }

    private static Dictionary<string, string> GenerateAuthParameters(string redirectUri, string clientId, string codeChallenge)
    {
        return new Dictionary<string, string>
        {
            { "scope", string.Join(' ', [Oauth2Service.Scope.UserinfoProfile, Oauth2Service.Scope.UserinfoEmail,
                                        DriveService.Scope.Drive, DriveService.Scope.DriveFile, DriveService.Scope.DriveAppdata]) },
            { "access_type", "offline" },
            { "include_granted_scopes", "true" },
            { "response_type", "code" },
            { "redirect_uri", redirectUri },
            { "client_id", clientId },
            { "code_challenge_method", "S256" },
            { "code_challenge", codeChallenge },
        };
    }

    private static async Task GetInitialToken(string authorizationCode, string redirectUri, string clientId, string codeVerifier)
    {
        var tokenEndpoint = "https://oauth2.googleapis.com/token";
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", authorizationCode),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            ])
        };

        var response = await client.SendAsync(tokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Trace.TraceError($"Error requesting initial token: {responseBody}");
            throw new HttpRequestException("Failed to get initial token");
        }

        Trace.TraceInformation($"Access token: {responseBody}");
        var jsonToken = JsonObject.Parse(responseBody);
        var accessToken = jsonToken!["access_token"]!.ToString();
        var refreshToken = jsonToken!["refresh_token"]!.ToString();
        var accessTokenExpiresIn = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + int.Parse(jsonToken!["expires_in"]!.ToString());
        await SecureStorage.SetAsync("access_token", accessToken);
        await SecureStorage.SetAsync("refresh_token", refreshToken);
        Preferences.Set("access_token_epires_in", accessTokenExpiresIn);
    }

    private async Task RefreshToken()
    {
        var clientId = _windowsClientId;
        var tokenEndpoint = "https://oauth2.googleapis.com/token";
        var refreshToken = await SecureStorage.GetAsync("refresh_token");
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
        {
            Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("grant_type", "refresh_token"),
                    new KeyValuePair<string, string>("refresh_token", refreshToken!)
                ]
            )
        };

        var response = await client.SendAsync(tokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Trace.TraceError($"Error refreshing token: {responseBody}");
            throw new HttpRequestException("Failed to refresh token");
        }

        Trace.TraceInformation($"Refresh token: {responseBody}");
        var jsonToken = JsonObject.Parse(responseBody);
        var accessToken = jsonToken!["access_token"]!.ToString();
        var accessTokenExpiresIn = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + int.Parse(jsonToken!["expires_in"]!.ToString());
        await SecureStorage.SetAsync("access_token", accessToken);
        Preferences.Set("access_token_epires_in", accessTokenExpiresIn);
    }

    private async Task RevokeTokens()
    {
        var revokeEndpoint = "https://oauth2.googleapis.com/revoke";
        var access_token = await SecureStorage.GetAsync("access_token");
        var client = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, revokeEndpoint)
        {
            Content = new FormUrlEncodedContent(
                [
                    new KeyValuePair<string, string>("token", access_token!),
                ]
            )
        };

        var response = await client.SendAsync(tokenRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            Trace.TraceError($"Error revoking token: {responseBody}");
            throw new HttpRequestException("Failed to revoke token");
        }

        Trace.TraceInformation($"Revoke token: {responseBody}");
        SecureStorage.Remove("access_token");
        SecureStorage.Remove("refresh_token");
        Preferences.Remove("access_token_epires_in");

        _credential = null;
        _oauth2Service = null;
        _driveService = null;
    }

    private static async Task<string> StartLocalHttpServerAsync(int port)
    {
        if (_listener is null)
        {
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://localhost:{port}/");
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                Trace.TraceError($"Port {port} unavailable");
                throw;
            }
        }
        else if (!_listener.IsListening)
        {
            _listener.Start();
        }

        Trace.TraceInformation($"Listening on http://localhost:{port}/...");
        var context = await _listener.GetContextAsync();

        var code = context.Request.QueryString["code"];
        var response = context.Response;
        var responseString = "Authorization complete. You can close this window.";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();

        _listener.Stop();
        _listener = null;

        if (code is null)
        {
            throw new HttpRequestException("Auth ode not returned");
        }

        return code;
    }

    private static string GenerateCodeVerifier()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32]; // Length can vary, e.g., 43-128 characters
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier));
        return Convert.ToBase64String(hash)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}