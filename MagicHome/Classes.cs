using Newtonsoft.Json;

namespace HSPI_MagicHome
{
    internal class Classes
    {
        internal class CADataSendResult
        {

            [JsonProperty("Result")]
            public string Result { get; set; }
        }

        internal class IpInfo
        {

            [JsonProperty("ip")]
            public string Ip { get; set; }

            [JsonProperty("hostname")]
            public string Hostname { get; set; }

            [JsonProperty("city")]
            public string City { get; set; }

            [JsonProperty("region")]
            public string Region { get; set; }

            [JsonProperty("country")]
            public string Country { get; set; }

            [JsonProperty("loc")]
            public string Loc { get; set; }

            [JsonProperty("org")]
            public string Org { get; set; }

            [JsonProperty("postal")]
            public string Postal { get; set; }
        }

        internal class HS3PluginDataEntry
        {

            [JsonProperty("Plugin")]
            public string Plugin { get; set; }

            [JsonProperty("publicIp")]
            public string publicIp { get; set; }

            [JsonProperty("TimeofEntry")]
            public string TimeofEntry { get; set; }

            [JsonProperty("ip")]
            public string ip { get; set; }

            [JsonProperty("Hostname")]
            public string Hostname { get; set; }

            [JsonProperty("macAddress")]
            public string macAddress { get; set; }

            [JsonProperty("isRegistered")]
            public string isRegistered { get; set; }

            [JsonProperty("isLicenced")]
            public string isLicenced { get; set; }

            [JsonProperty("registrationMode")]
            public string registrationMode { get; set; }

            [JsonProperty("pluginVersion")]
            public string pluginVersion { get; set; }
        }
    }
}