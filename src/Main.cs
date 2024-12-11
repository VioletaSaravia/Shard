using System.Net;
using VioletaRedis.src;

var ip = IPAddress.Parse(Environment.GetEnvironmentVariable("VIOLETA_REDIS_IP") ?? "127.0.0.1");
var port = int.Parse(Environment.GetEnvironmentVariable("VIOLETA_REDIS_PORT") ?? "6379");
var replicaCount = int.Parse(Environment.GetEnvironmentVariable("VIOLETA_REDIS_REPLICA_COUNT") ?? "0");

foreach (var i in Enumerable.Range(1, replicaCount))
    _ = new Replica(ip, port + i).Run();

await new Server(ip, port).Run();