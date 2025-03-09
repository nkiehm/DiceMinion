using System.Text;
using System.Text.Json;

namespace DiceMinion;

public abstract class OpCode
{
    public byte[] SerialToBytes()
    {
        var typeinfo = GetType();
        var serialized = JsonSerializer.Serialize(this, typeinfo);
        return Encoding.UTF8.GetBytes(serialized);
    }
}