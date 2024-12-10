using System.Net;
using System.Net.Sockets;

TcpListener server = new(IPAddress.Any, 6379);
server.Start();

while (true)
{
    try
    {
        using TcpClient client = await server.AcceptTcpClientAsync();
        await new TcpHandler(client).HandleMessage();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
    }

}
