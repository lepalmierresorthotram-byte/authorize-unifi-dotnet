using System;
using Newtonsoft.Json;

namespace AuthorizeUnifi.Models
{
    /// <summary>
    /// Represents a Unifi device/client connected to network
    /// </summary>
    public class UnifiDevice
    {
        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("mac")]
        public string MacAddress { get; set; }

        [JsonProperty("ip")]
        public string IpAddress { get; set; }

        [JsonProperty("hostname")]
        public string HostName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("site_id")]
        public string SiteId { get; set; }

        [JsonProperty("is_guest")]
        public bool IsGuest { get; set; }

        [JsonProperty("is_wired")]
        public bool IsWired { get; set; }

        [JsonProperty("signal_level")]
        public int SignalLevel { get; set; }

        [JsonProperty("uptime")]
        public long Uptime { get; set; }

        [JsonProperty("last_seen")]
        public long LastSeen { get; set; }

        public string Status { get; set; }
    }
}
