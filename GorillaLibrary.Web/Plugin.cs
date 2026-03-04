using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GorillaLibrary.Web;
using MelonLoader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

[assembly: MelonInfo(typeof(Plugin), "GorillaLibrary.Web", "1.0.0", "Lapis/dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonColor(255, 85, 82, 235)]
[assembly: MelonAdditionalCredits("GorillaLibrary Open-Source Contributors")]

namespace GorillaLibrary.Web
{
    public class WebSocketManager
    {
        private ClientWebSocket _socket;
        private CancellationTokenSource _cts;

        public static event Action OnConnected;
        public static event Action OnDisconnect;
        public static event Action<Exception> OnError;

        private static readonly Dictionary<string, List<Action<JObject>>> _subscribers =
            new Dictionary<string, List<Action<JObject>>>();

        public bool IsConnected => _socket?.State == WebSocketState.Open;

        public async Task Connect(string url, CancellationToken cancellationToken = default)
        {
            Uri uri = new Uri(url);

            if (_socket != null && (IsConnected || _socket.State == WebSocketState.Connecting)) return;

            _socket = new ClientWebSocket();
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            try
            {
                await _socket.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
                OnConnected.Invoke();
                await Task.Run(() => ReceiveMessagesAsync(_socket, _cts.Token));
            }
            catch (Exception ex)
            {
                OnError.Invoke(ex);
                MelonLogger.Error(ex.Message);
            }
        }

        private static async Task ReceiveMessagesAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

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
                                if (_subscribers.TryGetValue(type, out var handlers))
                                {
                                    handlersCopy = new List<Action<JObject>>(handlers);
                                }

                                if (handlersCopy != null)
                                {
                                    foreach (var handler in handlersCopy)
                                    {
                                        Task.Run(() =>
                                        {
                                            try
                                            {
                                                handler.Invoke(obj);
                                            }
                                            catch (Exception ex)
                                            {
                                                OnError.Invoke(ex);
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
                            OnError.Invoke(ex);
                        }

                        break;

                    case WebSocketMessageType.Close:
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing",
                            CancellationToken.None);
                        break;
                }
            }
        }

        public async Task SendMessage(object payload, CancellationToken cancellationToken = default)
        {
            if (!IsConnected) return;

            var message = new JObject
            {
                ["payload"] = payload == null ? JValue.CreateNull() : JToken.FromObject(payload)
            };

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await _socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, cancellationToken)
                .ConfigureAwait(false);
        }

        public void Subscribe(string messageType, Action<JObject> handler)
        {
            if (string.IsNullOrEmpty(messageType)) return;
            if (handler == null) return;

            if (!_subscribers.TryGetValue(messageType, out var list))
            {
                list = new List<Action<JObject>>();
                _subscribers[messageType] = list;
            }

            list.Add(handler);
        }

        public void Unsubscribe(string messageType, Action<JObject> handler)
        {
            if (string.IsNullOrEmpty(messageType)) return;
            if (handler == null) return;

            if (_subscribers.TryGetValue(messageType, out var list))
            {
                list.RemoveAll(h => h == handler);
                _subscribers.Remove(messageType);
            }
        }

        public void ClearSubscribers()
        {
            _subscribers.Clear();
        }

        public async Task Disconnect()
        {
            try
            {
                if (_socket != null && (_socket.State == WebSocketState.Open ||
                                        _socket.State == WebSocketState.CloseReceived))
                {
                    await _socket
                        .CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None)
                        .ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
            }
            finally
            {
                _cts?.Cancel();
                _socket?.Dispose();
                _socket = null;
                OnDisconnect?.Invoke();
            }
        }
    }

    public class HttpRequestModel
    {
        public ERequestType Method { get; set; }
        public JObject PostData { get; set; }
        public string ContentType { get; set; } = "application/json";
        public Dictionary<string, string> Headers { get; set; }
    }

    public class WebRequestManager
    {
        public async void SendRequest(string url, HttpRequestModel model, Action<string> onSuccess, Action<Exception> onError)
        {
            UnityWebRequest request = null;
            
            try
            {
                string payloadString = model.PostData != null ? JsonConvert.SerializeObject(model.PostData) : string.Empty;
                
                switch (model.Method)
                {
                    case ERequestType.Get:
                        request = UnityWebRequest.Get(url);
                        break;

                    case ERequestType.Post:
                        request = new UnityWebRequest(url, "POST");
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadString));
                        request.downloadHandler = new DownloadHandlerBuffer();
                        break;

                    case ERequestType.Put:
                        request = UnityWebRequest.Put(url, payloadString);
                        break;

                    case ERequestType.Delete:
                        request = UnityWebRequest.Delete(url);
                        break;

                    case ERequestType.Patch:
                        request = new UnityWebRequest(url, "PATCH");
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadString));
                        request.downloadHandler = new DownloadHandlerBuffer();
                        break;

                    case ERequestType.Head:
                        request = UnityWebRequest.Head(url);
                        break;

                    case ERequestType.Options:
                        request = new UnityWebRequest(url, "OPTIONS");
                        request.downloadHandler = new DownloadHandlerBuffer();
                        break;

                    case ERequestType.Trace:
                        request = new UnityWebRequest(url, "TRACE");
                        request.downloadHandler = new DownloadHandlerBuffer();
                        break;
                }
                
                if (request != null)
                {
                    request.SetRequestHeader("Content-Type", model.ContentType);
                    
                    if (model.Headers != null)
                    {
                        foreach (var header in model.Headers)
                        {
                            request.SetRequestHeader(header.Key, header.Value);
                        }
                    }
                    
                    await request.SendWebRequest();

                    if (request.result == UnityWebRequest.Result.Success)
                    {
                        onSuccess.Invoke(request.downloadHandler?.text ?? string.Empty);
                    }
                    else
                    {
                        onError.Invoke(new Exception($"{request.error}: {request.downloadHandler?.text}"));
                    }
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke(ex);
            }
            finally
            {
                request?.Dispose();
            }
        }
    }
    public class Plugin : MelonMod
    {
        // :3
    }
}