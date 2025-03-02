using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

var config = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .AddJsonFile("discord_settings.json")
    .Build();

var disc_url = config["disc_url"];
var bot_token = config["bot_token"];

HttpClient client = new HttpClient();
client.BaseAddress = new Uri(disc_url);
client.DefaultRequestHeaders.Add("User-Agent", "DiscordBot (https://localhost, 0.1.0)");
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", bot_token);

var response = await client.GetAsync("gateway/bot");
var responseBody = await response.Content.ReadAsStringAsync();

Console.WriteLine(response);
Console.WriteLine(responseBody);