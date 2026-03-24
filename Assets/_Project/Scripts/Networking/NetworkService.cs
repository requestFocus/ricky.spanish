
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
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during GET: {e.Message}");
            return null;
        }
    }
    
    public async UniTask DeleteCardAsync(string id)
    {
        var requestUrl = $"{Url}/{id}";
        try
        {
            using var request = UnityWebRequest.Delete(requestUrl);
    
            await request.SendWebRequest().ToUniTask();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not delete {id}: {request.error}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during DELETE: {e.Message}");
            throw;
        }
    }

    public async UniTask AddCardAsync(Flashcard flashcard)
    {
        try
        {
            string json = JsonConvert.SerializeObject(flashcard); 

            using var request = new UnityWebRequest(Url, "POST");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not add card: {request.error}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during POST: {e.Message}");
            throw;
        }
    }

    public async UniTask UpdateCardAsync(Flashcard flashcard)
    {
        try
        {
            string json = JsonConvert.SerializeObject(flashcard); 
            
            string requestUrl = $"{Url}/{flashcard.Id}";
            using var request = new UnityWebRequest(requestUrl, "PUT");
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not update card: {request.error}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during PUT: {e.Message}");
            throw;
        }
    }

    public async UniTask MarkAsFavouriteAsync(string id, bool state)
    {
        try
        {
            string requestUrl = $"{Url}/{id}";
            
            using var request = new UnityWebRequest(requestUrl, "PATCH");
            var patchData = new Dictionary<string, bool>
            {
                { "IsFavourite", state }
            };
            string json = JsonConvert.SerializeObject(patchData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not patch card: {request.error}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during PATCH: {e.Message}");
            throw;
        }
    }

    public async UniTask SaveNoteAsync(string id, string note)
    {
        try
        {
            string requestUrl = $"{Url}/{id}";
            
            using var request = new UnityWebRequest(requestUrl, "PATCH");
            var patchData = new Dictionary<string, string>
            {
                { "Note", note }
            };
            string json = JsonConvert.SerializeObject(patchData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);

            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[NetworkService] Could not patch card: {request.error}");
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            Debug.LogError($"[NetworkService] Exception during PATCH: {e.Message}");
            throw;
        }
    }
}

