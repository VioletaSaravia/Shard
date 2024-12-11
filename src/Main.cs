using System.Net;
using VioletaRedis.src;

int IP = 6379;

foreach (var i in Enumerable.Range(1, 4))
    _ = new Replica(IPAddress.Any, IP + i).Run();

await new Server(IPAddress.Any, IP).Run();