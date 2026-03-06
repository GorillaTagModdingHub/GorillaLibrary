using GorillaLibrary.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine.Networking;

namespace GorillaLibrary.Utilities;

public static class WebRequestUtility
{
    public static async void SendRequest(string url, WebRequest model, Action<string> onSuccess, Action<Exception> onError)
    {
        UnityWebRequest request = null;

        try
        {
            string payloadString = model.PostData != null ? JsonConvert.SerializeObject(model.PostData) : string.Empty;

            switch (model.Method)
            {
                case RequestMethod.Get:
                    request = UnityWebRequest.Get(url);
                    break;

                case RequestMethod.Post:
                    request = new(url, "POST")
                    {
                        uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadString)),
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    break;

                case RequestMethod.Put:
                    request = UnityWebRequest.Put(url, payloadString);
                    break;

                case RequestMethod.Delete:
                    request = UnityWebRequest.Delete(url);
                    break;

                case RequestMethod.Patch:
                    request = new(url, "PATCH")
                    {
                        uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(payloadString)),
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    break;

                case RequestMethod.Head:
                    request = UnityWebRequest.Head(url);
                    break;

                case RequestMethod.Options:
                    request = new(url, "OPTIONS")
                    {
                        downloadHandler = new DownloadHandlerBuffer()
                    };
                    break;

                case RequestMethod.Trace:
                    request = new(url, "TRACE")
                    {
                        downloadHandler = new DownloadHandlerBuffer()
                    };
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
                    onSuccess?.Invoke(request.downloadHandler?.text ?? string.Empty);
                }
                else
                {
                    onError?.Invoke(new Exception($"{request.error}: {request.downloadHandler?.text}"));
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
