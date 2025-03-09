namespace DiceMinion;

public class Op1Heartbeat (ulong? lastSeqNum) : OpCode
{
    public int op { get; set; } = 1;
    public ulong? d { get; set; } = lastSeqNum;
}