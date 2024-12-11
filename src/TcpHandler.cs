using System.Net.Sockets;
using System.Text;
using static VioletaRedis.src.Server;

namespace VioletaRedis.src;

public struct TcpHandler
{
    readonly Server Server;
    public TcpClient Client;
    public NetworkStream Stream;
    public Memory<byte> Buffer;


    public TcpHandler(TcpClient _client, Server server)
    {
        Client = _client;
        Stream = Client.GetStream();
        Server = server;

        Span<byte> buf = new();
        _ = Stream.Read(buf);
    }

    public readonly Task HandleMessage() => Buffer.ToString().Split(" ") switch
    {
    ["PING"] => HandlePing(),
    ["ECHO", ..] => HandleEcho(),
    ["SET", var key, var val] => HandleSet(key, Encoding.ASCII.GetBytes(val)),
    ["SET", var key, var val, "px", var expiry] => HandleSet(key, Encoding.ASCII.GetBytes(val), TimeSpan.FromMilliseconds(double.Parse(expiry))),
    ["GET", var key] => HandleGet(key),
    ["CONFIG", "GET", var key] => HandleConfigGet(key),
    ["CONFIG", "SET", var key, var val] => HandleConfigSet(key, val),
    ["KEYS", var pattern] => HandleGetKeys(pattern),
        _ => throw new NotImplementedException(),
    };

    async readonly Task HandlePing()
    {
        byte[] response = Encoding.ASCII.GetBytes("+PONG\r\n");
        await Stream.WriteAsync(response, 0, response.Length);
        Console.WriteLine("Sent: PONG");
    }

    async readonly Task HandleEcho()
    {
        await Stream.WriteAsync(Buffer);
    }


    async readonly Task HandleSet(string key, byte[] val, TimeSpan? expiry = null)
    {
        var ok = Server.Data.TryAdd(key, new StoreValue(val, DateTime.Now + expiry)) ? "+OK\r\n" : "$-1\r\n";

        var response = Encoding.ASCII.GetBytes(ok);


        await Stream.WriteAsync(response, 0, response.Length);
    }

    async readonly Task HandleGet(string key)
    {
        Server.Data.TryGetValue(key, out StoreValue? value);
        if (value is null) return;

        string response = value.Expiry > DateTime.Now
            ? "$3\r\n" + value.Value ?? "$-1\r\n" + "\r\n"
            : "$-1\r\n";

        var responseBytes = Encoding.ASCII.GetBytes(response);
        await Stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    async readonly Task HandleConfigSet(string key, string val)
    {
        var ok = Server.Config.TryAdd(key, val);

        var responseBytes = Encoding.ASCII.GetBytes(ok ? "+OK\r\n" : "$-1\r\n");
        await Stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    async readonly Task HandleConfigGet(string key)
    {
        Server.Config.TryGetValue(key, out string? value);

        string response = "$3\r\n" + value ?? "$-1\r\n" + "\r\n";

        var responseBytes = Encoding.ASCII.GetBytes(response);
        await Stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }

    async readonly Task HandleGetKeys(string pattern)
    {
        string[] keys = [];
        if (pattern == "*") keys = [.. Server.Data.Keys];

        var responseBytes = Encoding.ASCII.GetBytes(keys.ToString() ?? "");
        await Stream.WriteAsync(responseBytes, 0, responseBytes.Length);
    }
};