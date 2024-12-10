using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;

public class TcpHandler
{
    public static ConcurrentDictionary<string, string> env = [];
    public TcpClient client;
    public NetworkStream stream;
    public Memory<byte> buffer;

    public TcpHandler(TcpClient _client)
    {
        client = _client;
        stream = client.GetStream();

        Span<byte> buf = new();
        _ = stream.Read(buf);
    }

    public Task HandleMessage() => buffer.ToString().Split(" ") switch
    {
    ["PING"] => HandlePing(),
    ["ECHO", ..] => HandleEcho(),
    ["SET", var key, var val] => HandleSet(key, val),
    ["SET", var key, var val, "px", var expiry] => HandleSet(key, val, expiry),
    ["GET", var key] => HandleGet(key),
        _ => throw new NotImplementedException(),
    };

    async Task HandlePing()
    {
        byte[] response = Encoding.ASCII.GetBytes("+PONG\r\n");
        await stream.WriteAsync(response, 0, response.Length);
        Console.WriteLine("Sent: PONG");
    }

    async Task HandleEcho()
    {
        await stream.WriteAsync(buffer);
    }


    async Task HandleSet(string key, string val, string expiry = "")
    {
        var ok = env.TryAdd(key, val) ? "+OK\r\n" : "$-1\r\n";

        var response = Encoding.ASCII.GetBytes(ok);

        if (expiry != ""); // TODO

        await stream.WriteAsync(response, 0, response.Length);
    }

    async Task HandleGet(string key)
    {
        env.TryGetValue(key, out string? value);

        var val = Encoding.ASCII.GetBytes("$3\r\n" + value ?? "$-1\r\n" + "\r\n");

        await stream.WriteAsync(val, 0, val.Length);
    }
};