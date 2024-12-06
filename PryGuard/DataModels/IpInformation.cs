using System.Text.Json.Serialization;

namespace PryGuard.DataModels
{
    /// <summary>
    /// Represents information about an IP address, including geolocation and other details.
    /// </summary>
    public class IpInformation
    {
        /// <summary>
        /// The IP address.
        /// </summary>
        [JsonPropertyName("ip")]
        public string IpAddress { get; set; }

        /// <summary>
        /// The city associated with the IP address.
        /// </summary>
        [JsonPropertyName("city")]
        public string City { get; set; }

        /// <summary>
        /// The region associated with the IP address.
        /// </summary>
        [JsonPropertyName("region")]
        public string Region { get; set; }

        /// <summary>
        /// The country associated with the IP address.
        /// </summary>
        [JsonPropertyName("country")]
        public string Country { get; set; }

        /// <summary>
        /// The latitude and longitude associated with the IP address.
        /// </summary>
        [JsonPropertyName("loc")]
        public string Location { get; set; }

        /// <summary>
        /// The organization associated with the IP address.
        /// </summary>
        [JsonPropertyName("org")]
        public string Organization { get; set; }

        /// <summary>
        /// The postal code associated with the IP address.
        /// </summary>
        [JsonPropertyName("postal")]
        public string PostalCode { get; set; }

        /// <summary>
        /// The time zone associated with the IP address.
        /// </summary>
        [JsonPropertyName("timezone")]
        public string TimeZone { get; set; }
    }
}
