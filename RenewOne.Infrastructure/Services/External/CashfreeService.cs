using Microsoft.Extensions.Configuration;
using RenewOne.Application.DTOs.Response.Service;
using RenewOne.Application.Interfaces.Services.External.Cashfree;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RenewOne.Infrastructure.Services.External
{
    public class CashfreeService : ICashfreeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CashfreeService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }
        public async Task<CashfreeCreateOrderResponse> CreateOrderAsync(CashfreeCreateOrderRequest request)
        {
            var cf = _configuration.GetSection("Cashfree");

            var payload = new
            {
                order_id = request.OrderId,

                order_currency = "INR",

                order_amount = request.OrderAmount,

                customer_details = new
                {
                    customer_id = request.CustomerId,
                    customer_name = request.CustomerName,
                    customer_email = request.CustomerEmail,
                    customer_phone = request.CustomerPhone
                },

                order_note = request.OrderNote
            };

            var message = new HttpRequestMessage(
                HttpMethod.Post,
                $"{cf["BaseUrl"]}/orders");

            message.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            message.Headers.Add("x-client-id", cf["ClientId"]);
            message.Headers.Add("x-client-secret", cf["ClientSecret"]);
            message.Headers.Add("x-api-version", cf["ApiVersion"]);

            message.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.SendAsync(message);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(responseContent);
            }

            return JsonSerializer.Deserialize<CashfreeCreateOrderResponse>(
                responseContent,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
        }

        public bool VerifyWebhookSignature(string payload,string timestamp,string signature)
        {
            var secret = _configuration["Cashfree:ClientSecret"];

            var signedPayload = timestamp + payload;

            using var hmac = new HMACSHA256(
                Encoding.UTF8.GetBytes(secret));

            var hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(signedPayload));

            var expectedSignature =
                Convert.ToBase64String(hash);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expectedSignature),
                Encoding.UTF8.GetBytes(signature));
        }
    }
}
