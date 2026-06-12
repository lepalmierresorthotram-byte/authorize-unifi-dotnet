using System;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using AuthorizeUnifi.Utils;

namespace AuthorizeUnifi.Services
{
    /// <summary>
    /// Service for guest authorization workflows
    /// </summary>
    public class AuthorizationService
    {
        private readonly UnifiApiService _unifiApiService;

        public AuthorizationService()
        {
            _unifiApiService = new UnifiApiService();
        }

        /// <summary>
        /// Get MAC address from IP
        /// </summary>
        public string GetMacAddressFromIp(string ipAddress)
        {
            try
            {
                var arp = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "arp",
                        Arguments = $"-a {ipAddress}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    }
                };

                arp.Start();
                string output = arp.StandardOutput.ReadToEnd();
                arp.WaitForExit();

                // Parse ARP output to extract MAC address
                var lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    if (line.Contains(ipAddress))
                    {
                        var parts = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 2)
                        {
                            string mac = parts[parts.Length - 1].Trim();
                            if (IsValidMacAddress(mac))
                            {
                                Logger.LogInfo($"Found MAC for IP {ipAddress}: {mac}");
                                return mac;
                            }
                        }
                    }
                }

                Logger.LogWarning($"Could not find MAC for IP {ipAddress}");
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error getting MAC address for {ipAddress}", ex);
                return null;
            }
        }

        /// <summary>
        /// Authorize guest with duration and limits
        /// </summary>
        public async Task<AuthorizationResult> AuthorizeGuestAsync(string ipAddress, string macAddress, 
            int durationMinutes = 60, long uploadLimitMB = 0, long downloadLimitMB = 0)
        {
            var result = new AuthorizationResult();

            try
            {
                // Validate MAC address
                if (string.IsNullOrEmpty(macAddress))
                {
                    macAddress = GetMacAddressFromIp(ipAddress);
                }

                if (!IsValidMacAddress(macAddress))
                {
                    result.Success = false;
                    result.Message = "Invalid MAC address format";
                    Logger.LogWarning($"Invalid MAC address: {macAddress}");
                    return result;
                }

                // Authorize via Unifi API
                bool authorized = await _unifiApiService.AuthorizeGuestAsync(
                    macAddress,
                    durationMinutes,
                    uploadLimitMB,
                    downloadLimitMB
                );

                if (authorized)
                {
                    result.Success = true;
                    result.Message = $"Guest authorized successfully for {durationMinutes} minutes";
                    result.MacAddress = macAddress;
                    result.IpAddress = ipAddress;
                    result.AuthorizedTime = DateTime.Now;
                    result.ExpirationTime = DateTime.Now.AddMinutes(durationMinutes);

                    Logger.LogInfo($"Guest authorization successful: IP={ipAddress}, MAC={macAddress}");
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to authorize guest on Unifi controller";
                    Logger.LogError($"Unifi authorization failed for {macAddress}");
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred during authorization";
                Logger.LogError("Authorization service error", ex);
            }

            return result;
        }

        /// <summary>
        /// Revoke guest access
        /// </summary>
        public async Task<AuthorizationResult> RevokeGuestAsync(string macAddress)
        {
            var result = new AuthorizationResult();

            try
            {
                if (!IsValidMacAddress(macAddress))
                {
                    result.Success = false;
                    result.Message = "Invalid MAC address format";
                    return result;
                }

                bool revoked = await _unifiApiService.RevokeGuestAsync(macAddress);

                if (revoked)
                {
                    result.Success = true;
                    result.Message = "Guest access revoked successfully";
                    result.MacAddress = macAddress;
                    Logger.LogInfo($"Guest revoked: {macAddress}");
                }
                else
                {
                    result.Success = false;
                    result.Message = "Failed to revoke guest on Unifi controller";
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred during revocation";
                Logger.LogError("Revocation service error", ex);
            }

            return result;
        }

        /// <summary>
        /// Validate MAC address format
        /// </summary>
        private bool IsValidMacAddress(string macAddress)
        {
            if (string.IsNullOrEmpty(macAddress))
                return false;

            var mac = macAddress.Replace(":", "").Replace("-", "");
            return mac.Length == 12 && System.Text.RegularExpressions.Regex.IsMatch(mac, "^[0-9A-Fa-f]{12}$");
        }
    }

    /// <summary>
    /// Result of authorization operation
    /// </summary>
    public class AuthorizationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public DateTime AuthorizedTime { get; set; }
        public DateTime ExpirationTime { get; set; }
    }
}
