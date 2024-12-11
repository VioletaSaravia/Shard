using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace VioletaRedis.src;

public class Server
{
    public enum ValueType : short
    {
        String,
        List,
        Set,
        SortedSet,
        Hash,
        Zipmap,
        Ziplist,
        Intset,
        SortedSetinZiplist,
        HashmapinZiplist,
        ListinQuicklist
    }

    public record StoreValue(byte[] Value, DateTime? Expiry = null)
    {
        public ValueType Encoding;
        public byte[] Value = Value;
        public DateTime? Expiry = Expiry;
    }

    public ConcurrentDictionary<string, StoreValue> Data = [];

    public ConcurrentDictionary<string, string> Config = new()
    {
        // dir - the path to the directory where the RDB file is stored (example: /tmp/redis-data)
        ["dir"] = "./",
        // dbfilename - the name of the RDB file (example: rdbfile)
        ["dbfilename"] = ""
    };

    readonly TcpListener listener;

    public Server(IPAddress ip, int port)
    {
        if (Config["dbfilename"] != "")
            LoadDataFromFile(Path.Combine(Config["dir"], Config["dbfilename"]));
        listener = new(ip, port);
        listener.Start();
    }

    private void LoadDataFromFile(string path)
    {
        using FileStream f = new(path, FileMode.Open, FileAccess.Read);

        var buf = new byte[8];
        for (int b = f.ReadByte(); b != 0xFF; b = f.ReadByte())
        {
            var expiryLength = b == 0xFD ? 4 : b == 0xFC ? 8 : 0;
            f.ReadExactly(buf, 0, expiryLength);
            DateTime timeout = DateTime.Parse(buf.Reverse().ToString() ?? "");

            var valueType = (ValueType)f.ReadByte();

            var keyLength = ParseLength(f);
            var key = new byte[keyLength];
            f.ReadExactly(key);

            var valueLength = ParseLength(f);
            var value = new byte[valueLength];
            f.ReadExactly(value);

            if (timeout > DateTime.UtcNow)
                Data.TryAdd(key.ToString() ?? throw new UnreachableException(), new StoreValue(value, timeout));
        }
    }

    private static int ParseLength(FileStream f)
    {
        var buf = new byte[8];
        int length = 0;
        var lengthEncoding = f.ReadByte();

        switch (lengthEncoding & 0b11)
        {
            case 0b00:
                length = lengthEncoding >> 2;
                break;
            case 0b01:
                length = (lengthEncoding >> 2) & (f.ReadByte() << 8);
                break;
            case 0b10:
                f.ReadExactly(buf, 0, 4);
                length = int.Parse(buf.Reverse().ToArray());
                break;
            case 0b11:
                switch (lengthEncoding >> 2)
                {
                    case 0:
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    default:
                        break;
                };
                throw new NotImplementedException();
        }

        return length;
    }

    public async Task Run()
    {
        while (true)
        {
            try
            {
                Console.WriteLine("Server waiting!!!");
                using TcpClient client = await listener.AcceptTcpClientAsync();
                await new TcpHandler(client, this).HandleMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}