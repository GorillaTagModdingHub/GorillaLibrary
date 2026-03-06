using GorillaLibrary.Models;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GorillaLibrary.Utilities;

public static class WebSocketUtility
{
    public static MelonEvent<WebSocketData> OnConnected = new(), OnDisconnect = new();
    public static MelonEvent<WebSocketData, Exception> OnError;

    private static readonly Dictionary<string, WebSocketData> _data = [];

    public static async Task<WebSocketData> Connect(string url, CancellationToken cancellationToken = default)
    {
        Uri uri = new(url);

        if (!_data.TryGetValue(url, out var data))
        {
            data = new(new ClientWebSocket(), CancellationTokenSource.CreateLinkedTokenSource(cancellationToken));
            _data.TryAdd(url, data);
        }

        if (data.Socket != null && (data.IsConnected || data.Socket.State == WebSocketState.Connecting)) return data;

        try
        {
            await data.Socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
            OnConnected.Invoke(data);
            await Task.Run(() => ReceiveMessagesAsync(data));
        }
        catch (Exception ex)
        {
            OnError.Invoke(data, ex);
            MelonLogger.Error(ex.Message);
        }

        return data;
    }

    private static async Task ReceiveMessagesAsync(WebSocketData data)
    {
        var buffer = new byte[1024 * 4];

        while (data.Socket.State == WebSocketState.Open)
        {
            var result = await data.Socket.ReceiveAsync(new ArraySegment<byte>(buffer), data.CancellationSource.Token);

            switch (result.MessageType)
            {
                case WebSocketMessageType.Text:
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    try
                    {
                        var obj = JObject.Parse(message);
                        var type = obj["type"].ToString();

                        if (!string.IsNullOrEmpty(type))
                        {
                            List<Action<JObject>> handlersCopy = null;

                            if (data.Subscribers.TryGetValue(type, out var handlers))
                            {
                                handlersCopy = [.. handlers];
                            }

                            if (handlersCopy != null)
                            {
                                foreach (var handler in handlersCopy)
                                {
                                    await Task.Run(() =>
                                    {
                                        try
                                        {
                                            handler.Invoke(obj);
                                        }
                                        catch (Exception ex)
                                        {
                                            OnError.Invoke(data, ex);
                                        }
                                    });
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Warning(
                                "received message without a 'type' field. make sure your server returns a type in the json message.");
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError.Invoke(data, ex);
                    }

                    break;

                case WebSocketMessageType.Close:
                    await data.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
            }
        }
    }

    public static async Task SendMessage(WebSocketData data, object payload, CancellationToken cancellationToken = default)
    {
        if (data == null || !data.IsConnected) return;

        var message = new JObject
        {
            ["payload"] = payload == null ? JValue.CreateNull() : JToken.FromObject(payload)
        };

        var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
        await data.Socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
    }

    public static void Subscribe(WebSocketData data, string messageType, Action<JObject> handler)
    {
        if (string.IsNullOrEmpty(messageType)) return;
        if (handler == null) return;

        if (!data.Subscribers.TryGetValue(messageType, out var list))
        {
            list = [];
            data.Subscribers.TryAdd(messageType, list);
        }

        list.Add(handler);
    }

    public static void Unsubscribe(WebSocketData data, string messageType, Action<JObject> handler)
    {
        if (string.IsNullOrEmpty(messageType)) return;
        if (handler == null) return;

        if (data.Subscribers.TryGetValue(messageType, out var list))
        {
            list.RemoveAll(h => h == handler);
            data.Subscribers.Remove(messageType);
        }
    }

    public static void ClearSubscribers(WebSocketData data)
    {
        data.Subscribers.Clear();
    }

    public static async Task Disconnect(WebSocketData data)
    {
        if (data == null) return;

        try
        {
            if (data.Socket != null && (data.IsConnected || data.Socket.State == WebSocketState.CloseReceived))
            {
                await data.Socket
                    .CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }

        catch (Exception ex)
        {
            OnError?.Invoke(data, ex);
        }

        finally
        {
            data.CancellationSource?.Cancel();
            data.Socket?.Dispose();

            OnDisconnect?.Invoke(data);

            for (int i = 0; i < _data.Count; i++)
            {
                var (url, wsd) = _data.ElementAt(i);
                if (wsd == data)
                {
                    _data.Remove(url);
                    break;
                }
            }
        }
    }
}
