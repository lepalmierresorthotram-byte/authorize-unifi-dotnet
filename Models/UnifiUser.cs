using System;
using Newtonsoft.Json;

namespace AuthorizeUnifi.Models
{
    /// <summary>
    /// Represents a Unifi user/guest authorization response
    /// </summary>
    public class UnifiUser
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("minutes")]
        public int Minutes { get; set; }

        [JsonProperty("up_limit")]
        public long UpLimit { get; set; }

        [JsonProperty("down_limit")]
        public long DownLimit { get; set; }

        [JsonProperty("bytes_up_limit")]
        public long BytesUpLimit { get; set; }

        [JsonProperty("bytes_down_limit")]
        public long BytesDownLimit { get; set; }

        public string MacAddress { get; set; }
        public string IpAddress { get; set; }
        public string HostName { get; set; }
        public DateTime AuthorizedTime { get; set; }
        public string Status { get; set; }
    }

    /// <summary>
    /// Unifi API response wrapper
    /// </summary>
    public class UnifiResponse<T>
    {
        [JsonProperty("meta")]
        public dynamic Meta { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }
}
