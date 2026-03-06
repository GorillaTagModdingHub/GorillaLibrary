using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

namespace GorillaLibrary.Models;

public class WebSocketData(ClientWebSocket socket, CancellationTokenSource cts)
{
    public bool IsConnected => Socket?.State == WebSocketState.Open;

    public readonly ClientWebSocket Socket = socket;

    public readonly CancellationTokenSource CancellationSource = cts;

    public readonly Dictionary<string, List<Action<JObject>>> Subscribers = [];
}
