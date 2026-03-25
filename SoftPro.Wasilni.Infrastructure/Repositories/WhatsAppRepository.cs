using SoftPro.Wasilni.Application.Abstracts.Repositories;
using System.Text;
using System.Text.Json;

namespace SoftPro.Wasilni.Infrastructure.Repositories;

public class WhatsAppRepository(HttpClient httpClient) : IWhatsAppRepository
{
    public async Task SendCode(string phonenumber, string code)
    {
        httpClient.DefaultRequestHeaders.Add("x-password", "0937712618");
        var payload = new
        {
            phone = string.Concat("963", phonenumber.AsSpan(1)),
            message = "Hello this is you verification code for Tawsela : " + code
        };

        // Serialize to JSON
        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Send POST request
        var response = await httpClient.PostAsync("https://whatsapp-web-otp-wy1q.onrender.com/whatsapp/send/", content);

        // Throw if not successful
        response.EnsureSuccessStatusCode();
    }

}