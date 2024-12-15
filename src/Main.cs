using System.Net;
using Shard.src;
using static Shard.src.Node;

var nodeType = Enum.Parse<NodeType>(Environment.GetEnvironmentVariable("SHARD_NODE_TYPE") ?? "Follower");
var ip = IPAddress.Parse(Environment.GetEnvironmentVariable("SHARD_IP") ?? "127.0.0.1");
var port = int.Parse(Environment.GetEnvironmentVariable("SHARD_PORT") ?? "6379");

await new Node(ip, port, nodeType).Run();