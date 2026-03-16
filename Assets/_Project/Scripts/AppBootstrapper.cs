using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AppBootstrapper: MonoBehaviour
{
    [SerializeField] private FlashcardController _flashcardController;
    private NetworkService _networkService = new NetworkService();

    private void OnEnable()
    {
        _flashcardController.OnRetryRequested += HandleRetry;
        _flashcardController.OnDeleteRequested += HandleDelete;
        _flashcardController.OnAddRequested += HandleAdd;
        _flashcardController.OnUpdateRequested += HandleUpdate;
    }

    private void OnDisable()
    {
        _flashcardController.OnRetryRequested -= HandleRetry;
        _flashcardController.OnDeleteRequested -= HandleDelete;
        _flashcardController.OnAddRequested -= HandleAdd;
        _flashcardController.OnUpdateRequested -= HandleUpdate;
    }

    private void HandleRetry()
    {
        StartAsync().Forget();
    }
    
    private void HandleDelete(string id)
    {
        DeleteCardAsync(id).Forget();
    }

    private async UniTaskVoid DeleteCardAsync(string id)
    {
        bool success = await _networkService.DeleteCardAsync(id);
        
        if (success)
        {
            StartAsync().Forget();
        }
        else
        {
            _flashcardController.ShowError("Failed to delete card");
        }
    }

    private void HandleAdd(Flashcard flashcard)
    {
        AddCardAsync(flashcard).Forget();
    }

    private void HandleUpdate(Flashcard flashcard)
    {
        UpdateCardAsync(flashcard).Forget();
    }

    private async UniTaskVoid AddCardAsync(Flashcard flashcard)
    {
        bool success = await _networkService.AddCardAsync(flashcard);
        
        if (success)
        {
            StartAsync().Forget();
        }
        else
        {
            _flashcardController.ShowError("Failed to add card");
        }
    }
    
    private async UniTaskVoid UpdateCardAsync(Flashcard flashcard)
    {
        bool success = await _networkService.UpdateCardAsync(flashcard);
        
        if (success)
        {
            StartAsync().Forget();
        }
        else
        {
            _flashcardController.ShowError("Failed to update card");
        }
    }

    public void Start()
    {
        StartAsync().Forget();
    }

    private async UniTaskVoid StartAsync()
    {
        _flashcardController.HideError();

        List<Flashcard> cards = await FetchCardsAsync();
        if (cards == null)
        {
            _flashcardController.ShowError("Failed to fetch cards");
        }
        
        _flashcardController.Initialize(cards);
    }

    private async UniTask<List<Flashcard>> FetchCardsAsync()
    {
        return await _networkService.FetchCardsAsync();
    }
}