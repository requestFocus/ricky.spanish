
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkService
{
    private const string Url = "http://localhost:3000/cards";

    public async UniTask<List<Flashcard>> FetchCardsAsync()
    {
        using var request = UnityWebRequest.Get(Url);

        try
        {
            await request.SendWebRequest().ToUniTask();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Connection Error: {request.error}");
                return null;
            }

            string json = request.downloadHandler.text;
            return JsonConvert.DeserializeObject<List<Flashcard>>(json);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception: {e.Message}");
            return null;
        }
    }
    
    public async UniTask<bool> DeleteCardAsync(int id)
    {
        var requestUrl = $"{Url}/{id}";
        try
        {
            using var request = UnityWebRequest.Delete(requestUrl);
    
            await request.SendWebRequest().ToUniTask();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not delete {id}: {request.error}");
                return false;
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception: {e.Message}");
            return false;
        }
    }

    public async UniTask<bool> AddCardAsync(Flashcard flashcard)
    {
        try
        {
            string json = JsonConvert.SerializeObject(flashcard, new JsonSerializerSettings 
            { 
                StringEscapeHandling = StringEscapeHandling.Default 
            });
            Debug.Log($"[NetworkService] Sending: {json}");

            using var request = new UnityWebRequest(Url, "POST");

            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            
            Debug.Log($"{request.downloadHandler.text}");
            return request.result == UnityWebRequest.Result.Success;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception: {e.Message}");
            return false;
        }
    }
}

