using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text.Json;
using DiceMinion;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddJsonFile("discord_settings.json")
    .Build();

var discUrl = config["disc_url"];
var botToken = config["bot_token"];

HttpClient client = new HttpClient();
client.BaseAddress = new Uri(discUrl);
client.DefaultRequestHeaders.Add("User-Agent", "DiscordBot (https://localhost, 0.1.0)");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

var gatewayResponse = await client.GetFromJsonAsync<GetBotGatewayResponse>("gateway/bot");
Console.WriteLine(gatewayResponse?.Url);

if (gatewayResponse?.Url == null)
{
    Console.WriteLine("No gateway response found");
    return;
}

var discordGateway = new DiscordWebSocket(gatewayResponse.Url);
await discordGateway.Connect();

Console.WriteLine("Bot Started");
await Task.Delay(-1);