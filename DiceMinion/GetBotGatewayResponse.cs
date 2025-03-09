using System.Text.Json;

namespace DiceMinion;

public class GetBotGatewayResponse
{
    public required Uri Url {get; set;}
    public JsonElement SessionStartLimit {get; set;}
    public int Shards {get; set;}
}