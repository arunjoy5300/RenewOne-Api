using System.Text.Json.Serialization;

namespace RenewOne.Application.DTOs.Request.RC
{
    public class RcValidationRequest
    {
        [JsonPropertyName("api_id")]
        public string ApiId { get; set; } = string.Empty;

        [JsonPropertyName("api_key")]
        public string ApiKey { get; set; } = string.Empty;

        [JsonPropertyName("token_id")]
        public string TokenId { get; set; } = string.Empty;

        [JsonPropertyName("reg_no")]
        public string RegNo { get; set; } = string.Empty;
    }
}
