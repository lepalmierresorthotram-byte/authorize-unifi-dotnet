using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AuthorizeUnifi.Models;
using AuthorizeUnifi.Utils;
using Newtonsoft.Json;

namespace AuthorizeUnifi.Services
{
    /// <summary>
    /// Service for communicating with Unifi Controller API
    /// </summary>
    public class UnifiApiService
    {
        private readonly string _controllerUrl;
        private readonly string _username;
        private readonly string _password;
        private readonly string _site;
        private string _authToken;
        private HttpClientHandler _handler;
        private HttpClient _httpClient;

        public UnifiApiService()
        {
            _controllerUrl = ConfigurationManager.AppSettings["UnifiControllerUrl"];
            _username = ConfigurationManager.AppSettings["UnifiUsername"];
            _password = ConfigurationManager.AppSettings["UnifiPassword"];
            _site = ConfigurationManager.AppSettings["UnifiSite"] ?? "default";

            InitializeHttpClient();
        }

        private void InitializeHttpClient()
        {
            // Disable SSL certificate validation for self-signed certs
            _handler = new HttpClientHandler();
            _handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;
            
            _httpClient = new HttpClient(_handler);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        /// <summary>
        /// Authenticate with Unifi Controller
        /// </summary>
        public async Task<bool> AuthenticateAsync()
        {
            try
            {
                var loginUrl = $"{_controllerUrl}/api/auth/login";
                var credentials = new
                {
                    username = _username,
                    password = _password
                };

                var json = JsonConvert.SerializeObject(credentials);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(loginUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInfo("Successfully authenticated with Unifi Controller");
                    return true;
                }
                else
                {
                    Logger.LogError($"Authentication failed: {response.StatusCode} - {response.Content}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Authentication error", ex);
                return false;
            }
        }

        /// <summary>
        /// Authorize a guest user on the network
        /// </summary>
        public async Task<bool> AuthorizeGuestAsync(string macAddress, int durationMinutes = 60, 
            long uploadLimit = 0, long downloadLimit = 0)
        {
            try
            {
                if (!await AuthenticateAsync())
                {
                    return false;
                }

                var authorizeUrl = $"{_controllerUrl}/api/s/{_site}/cmd/hotspot";
                var authData = new
                {
                    cmd = "authorize-guest",
                    mac = macAddress,
                    minutes = durationMinutes,
                    up_limit = uploadLimit,
                    down_limit = downloadLimit,
                    bytes_up_limit = uploadLimit * 1024 * 1024,
                    bytes_down_limit = downloadLimit * 1024 * 1024
                };

                var json = JsonConvert.SerializeObject(authData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(authorizeUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInfo($"Guest authorized: {macAddress} for {durationMinutes} minutes");
                    return true;
                }
                else
                {
                    Logger.LogError($"Guest authorization failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error authorizing guest {macAddress}", ex);
                return false;
            }
        }

        /// <summary>
        /// Revoke guest authorization
        /// </summary>
        public async Task<bool> RevokeGuestAsync(string macAddress)
        {
            try
            {
                if (!await AuthenticateAsync())
                {
                    return false;
                }

                var revokeUrl = $"{_controllerUrl}/api/s/{_site}/cmd/hotspot";
                var revokeData = new
                {
                    cmd = "revoke-voucher",
                    mac = macAddress
                };

                var json = JsonConvert.SerializeObject(revokeData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(revokeUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    Logger.LogInfo($"Guest revoked: {macAddress}");
                    return true;
                }
                else
                {
                    Logger.LogError($"Guest revocation failed: {response.StatusCode}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error revoking guest {macAddress}", ex);
                return false;
            }
        }

        /// <summary>
        /// Get list of connected clients
        /// </summary>
        public async Task<List<UnifiDevice>> GetClientsAsync()
        {
            try
            {
                if (!await AuthenticateAsync())
                {
                    return null;
                }

                var clientsUrl = $"{_controllerUrl}/api/s/{_site}/stat/sta";
                var response = await _httpClient.GetAsync(clientsUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var unifiResponse = JsonConvert.DeserializeObject<UnifiResponse<List<UnifiDevice>>>(json);
                    
                    Logger.LogInfo($"Retrieved {unifiResponse.Data.Count} clients");
                    return unifiResponse.Data;
                }
                else
                {
                    Logger.LogError($"Failed to get clients: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting clients", ex);
                return null;
            }
        }

        /// <summary>
        /// Get guest networks
        /// </summary>
        public async Task<dynamic> GetGuestNetworksAsync()
        {
            try
            {
                if (!await AuthenticateAsync())
                {
                    return null;
                }

                var guestUrl = $"{_controllerUrl}/api/s/{_site}/rest/guestnetwork";
                var response = await _httpClient.GetAsync(guestUrl);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Logger.LogInfo("Retrieved guest networks");
                    return JsonConvert.DeserializeObject(json);
                }
                else
                {
                    Logger.LogError($"Failed to get guest networks: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error getting guest networks", ex);
                return null;
            }
        }

        /// <summary>
        /// Check device connection status
        /// </summary>
        public async Task<bool> CheckDeviceStatusAsync(string macAddress)
        {
            try
            {
                var clients = await GetClientsAsync();
                if (clients == null) return false;

                var device = clients.FirstOrDefault(c => c.MacAddress.Equals(macAddress, StringComparison.OrdinalIgnoreCase));
                return device != null && device.Uptime > 0;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error checking device status", ex);
                return false;
            }
        }
    }
}
