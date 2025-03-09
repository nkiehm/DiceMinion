using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DiceMinion;

public class DiscordWebSocket
{
    private const int ReceiveBufferSize = 8192;
    
    private ClientWebSocket? _client;
    private readonly Uri _gatewayUri;
    private CancellationTokenSource? _cts;
    
    private ulong? _lastSeqNum;
    private ulong? _hbInt;

    public DiscordWebSocket(Uri gatewayUri)
    {
        _gatewayUri = gatewayUri;
        _cts = new CancellationTokenSource();
    }

    public async Task Connect()
    {
        _client = new ClientWebSocket();
        await _client.ConnectAsync(_gatewayUri, _cts.Token);
        _ = Task.Run(ReceiveLoop);
    }
    
    public async Task Close()
    {
        if (_client == null) return;

        if (_client.State == WebSocketState.Open)
        {
            await _client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
            if (_cts != null) await _cts.CancelAsync();
        }
        
        _client.Dispose();
        _client = null;
        _cts?.Dispose();
        _cts = null;
    }
    
    private async Task ReceiveLoop()
    {
        if (_client == null) return;
        if (_cts == null) return;
        
        var receiveBuffer = new Memory<byte>(new byte[ReceiveBufferSize]);
        while (true)
        {
            var outputBuffer = new MemoryStream();
            ValueWebSocketReceiveResult result;
            do
            {
                result = await _client.ReceiveAsync(receiveBuffer, _cts.Token);
                if (result.MessageType == WebSocketMessageType.Close) break;
                outputBuffer.Write(receiveBuffer.ToArray(), 0, result.Count);
            } while (!result.EndOfMessage);

            outputBuffer.Position = 0;
            HandleMessage(outputBuffer);
        }
    }
    
    private async Task HandleMessage(MemoryStream message)
    {
        var gatewayMessage = JsonSerializer.Deserialize<GatewayMessage>(message.ToArray());
        if (gatewayMessage == null) return;

        _lastSeqNum = gatewayMessage.s;
        switch (gatewayMessage.op)
        {
            case 1:
                var heartbeat = new Op1Heartbeat(_lastSeqNum);
                await _client.SendAsync(
                    heartbeat.SerialToBytes(),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
                );
                break;
            case 10:
                var op10Hello = gatewayMessage?.ParseData<Op10Hello>();
                if (op10Hello == null) return;
                _hbInt = op10Hello.heartbeat_interval;
                
                var jitter = Random.Shared.NextDouble();
                var firstBeat = op10Hello?.heartbeat_interval * jitter;

                _ = Task.Run(async () =>
                {
                    await Task.Delay((int)firstBeat!);
                    var heartbeat = new Op1Heartbeat(_lastSeqNum);
                    await _client.SendAsync(
                        heartbeat.SerialToBytes(),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None
                    );
                   _ = BeginHeartbeat();
                });
                break;
            default:
                Console.WriteLine("Unknown Op code: " + gatewayMessage.op);
                return;
        }
    }

    private async Task BeginHeartbeat()
    {
        while (true)
        {
            await Task.Delay((int)_hbInt!);
            var heartbeat = new Op1Heartbeat(_lastSeqNum);
            await _client.SendAsync(
                heartbeat.SerialToBytes(),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }
}