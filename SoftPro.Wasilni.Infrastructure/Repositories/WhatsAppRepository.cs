using Microsoft.Extensions.Logging;
using SoftPro.Wasilni.Application.Abstracts.Repositories;
using System.Text;
using System.Text.Json;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class WhatsAppRepository(HttpClient httpClient, ILogger logger) : IWhatsAppRepository
{
    public async Task<bool> SendCode(string phonenumber, string code, CancellationToken cancellationToken)
    {
        try
        {
            httpClient.DefaultRequestHeaders.Add("x-password", "0937712618"); // Todo : Phonenumber add from this service ??!!
            var payload = new
            {
                phone = string.Concat("963", phonenumber.AsSpan(1)),
                message = "Hello this is you verification code for Tawsela : " + code
            };

            // Serialize to JSON
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Send POST request
            var response = await httpClient.PostAsync("https://whatsapp-web-otp-production-c90b.up.railway.app/whatsapp/send", content, cancellationToken);

            // Throw if not successful
            response.EnsureSuccessStatusCode();
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError("Some bugs in whatsapp service: {a}", ex.Message);
            return false;
        }
    }

}