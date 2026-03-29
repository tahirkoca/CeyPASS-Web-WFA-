using CeyPASS.Business.Abstractions;
using CeyPASS.DataAccess.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CeyPASS.Business.Services
{
    public class FcmPushService : IPushNotificationService
    {
        private readonly IUserDeviceTokenRepository _tokenRepository;
        private readonly ILogger<FcmPushService> _logger;
        private readonly IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient();

        public FcmPushService(
            IUserDeviceTokenRepository tokenRepository,
            ILogger<FcmPushService> logger,
            IConfiguration configuration)
        {
            _tokenRepository = tokenRepository;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendPushToUserAsync(string? personelId, string? kullaniciId, string title, string body, object? data = null)
        {
            var tokens = _tokenRepository.GetTokensByUser(personelId, kullaniciId);
            if (tokens == null || tokens.Count == 0) return;

            foreach (var token in tokens)
            {
                await SendPushToTokenAsync(token, title, body, data);
            }
        }

        public async Task SendPushToTokenAsync(string token, string title, string body, object? data = null)
        {
            _logger.LogInformation("Push Notification Sending to {Token}: {Title} - {Body}", token, title, body);

            // TODO: FCM HTTP v1 API Implementation
            // Real implementation requires a Service Account JSON and Bearer Token.
            // For now, we simulate the success in logs to confirm the logic works.
            
            // var fcmServerKey = _configuration["Fcm:ServerKey"];
            // if (string.IsNullOrEmpty(fcmServerKey)) return;

            await Task.CompletedTask;
        }
    }
}
