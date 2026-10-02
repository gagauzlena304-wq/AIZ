using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ToDoListApp.Services
{
    public class GoogleOAuthService
    {
        // ========================================================================
        // INSTRUCTIONS FOR USER:
        // 1. Go to Google Cloud Console (https://console.cloud.google.com/)
        // 2. Create a New Project.
        // 3. Go to "APIs & Services" -> "Credentials".
        // 4. Click "Create Credentials" -> "OAuth client ID".
        // 5. Select Application Type: "Desktop app" (or "iOS/Android" if testing).
        // 6. Copy the desktop application's Client ID below.
        // Installed applications are public OAuth clients; do not embed a client secret.
        // ========================================================================
        private const string ClientId = "668618493716-pcevd0j2bjrb7pe5qic81g0vbtbgfb16.apps.googleusercontent.com";

        private const string AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        private const string TokenEndpoint = "https://oauth2.googleapis.com/token";
        private const string UserInfoEndpoint = "https://www.googleapis.com/oauth2/v3/userinfo";


        public async Task<GoogleUserInfo> AuthenticateAsync()
        {
            if (ClientId.Contains("YOUR_CLIENT_ID_HERE"))
            {
                throw new Exception("Please configure your Google Client ID in GoogleOAuthService.cs");
            }

            // 1. Generate state and PKCE challenge
            string state = Guid.NewGuid().ToString("N");
            string codeVerifier = GenerateCodeVerifier();
            string codeChallenge = GenerateCodeChallenge(codeVerifier);

            // 2. Start TcpListener on a dynamic random port
            var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            string redirectUri = $"http://127.0.0.1:{port}/";

            // 3. Create Authorization Request
            string authUrl = $"{AuthorizationEndpoint}?client_id={ClientId}&response_type=code&scope=openid%20profile%20email&redirect_uri={Uri.EscapeDataString(redirectUri)}&state={state}&code_challenge={codeChallenge}&code_challenge_method=S256";

            // 4. Open Browser
            _ = Windows.System.Launcher.LaunchUriAsync(new Uri(authUrl));

            // 5. Wait for the redirect via TCP
            using var client = await listener.AcceptTcpClientAsync();
            using var stream = client.GetStream();
            using var reader = new System.IO.StreamReader(stream, Encoding.UTF8);

            string? requestLine = await reader.ReadLineAsync();
            
            // Read remaining headers to not hang the browser
            string? headerLine;
            while (!string.IsNullOrEmpty(headerLine = await reader.ReadLineAsync())) { }

            string responseString = "HTTP/1.1 200 OK\r\nContent-Type: text/html\r\nConnection: close\r\n\r\n<html><body style='font-family: sans-serif; text-align: center; padding-top: 50px;'><h2>Authentication successful!</h2><p>You can close this tab and return to the To-Do app.</p></body></html>";
            var responseBytes = Encoding.UTF8.GetBytes(responseString);
            await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
            
            client.Close();
            listener.Stop();

            if (string.IsNullOrEmpty(requestLine)) throw new Exception("Empty request from browser");

            // 6. Extract code
            string[] parts = requestLine.Split(' ');
            if (parts.Length < 2) throw new Exception("Invalid request from browser");
            string pathAndQuery = parts[1];
            
            var uri = new Uri("http://localhost" + pathAndQuery);
            var queryParams = ParseQueryString(uri.Query);

            if (!queryParams.TryGetValue("state", out string? returnedState) || returnedState != state)
            {
                throw new Exception("Invalid state parameter");
            }
            if (!queryParams.TryGetValue("code", out string? code))
            {
                throw new Exception("No code returned from Google");
            }

            // 7. Exchange code for tokens
            using var httpClient = new HttpClient();
            var tokenRequestBody = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code_verifier", codeVerifier)
            });

            var tokenResponse = await httpClient.PostAsync(TokenEndpoint, tokenRequestBody);
            var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
            
            if (!tokenResponse.IsSuccessStatusCode)
            {
                throw new Exception($"Failed to get token: {tokenContent}");
            }

            var tokenDict = JsonSerializer.Deserialize<JsonElement>(tokenContent);
            string accessToken = tokenDict.GetProperty("access_token").GetString();

            // 8. Get User Info
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            var userInfoResponse = await httpClient.GetAsync(UserInfoEndpoint);
            var userInfoContent = await userInfoResponse.Content.ReadAsStringAsync();
            
            return JsonSerializer.Deserialize<GoogleUserInfo>(userInfoContent);
        }

        private static string GenerateCodeVerifier()
        {
            const uint length = 32;
            byte[] bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Base64UrlencodeNoPadding(bytes);
        }

        private static string GenerateCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            byte[] challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Base64UrlencodeNoPadding(challengeBytes);
        }

        private static string Base64UrlencodeNoPadding(byte[] buffer)
        {
            string base64 = Convert.ToBase64String(buffer);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }

        private static Dictionary<string, string> ParseQueryString(string query)
        {
            var dict = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(query)) return dict;
            
            if (query.StartsWith("?")) query = query.Substring(1);
            
            foreach (var pair in query.Split('&'))
            {
                var kvp = pair.Split('=');
                if (kvp.Length == 2)
                {
                    dict[Uri.UnescapeDataString(kvp[0])] = Uri.UnescapeDataString(kvp[1]);
                }
            }
            return dict;
        }
    }

    public class GoogleUserInfo
    {
        public string sub { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string picture { get; set; }
        public string email { get; set; }
        public bool email_verified { get; set; }
    }
}
