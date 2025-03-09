using System.Text.Json;

namespace DiceMinion;

public class GatewayMessage
{
    public string? type { get; set; }
    public ulong? s { get; set; }
    public int op { get; set; }
    public JsonElement d { get; set; }

    public T? ParseData<T>()
    {
        return d.Deserialize<T>();
    }
}