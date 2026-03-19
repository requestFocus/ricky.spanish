using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class FlashcardController : MonoBehaviour
{
    private VisualElement _root => GetComponent<UIDocument>().rootVisualElement;
    private Label _polishLabel;
    private Label _spanishLabel;
    private Label _errorLabel;
    private TextField _polishInput;
    private TextField _spanishInput;
    private Button _reconnectButton;
    private Button _revealButton;
    private Button _addButton;
    private Button _deleteButton;
    private Button _saveUpdateButton;
    private Button _cancelCardChangeButton;
    private Button _editButton;
    
    private VisualElement _errorsContainer;
    private VisualElement _addUpdateCardScreen;
    private VisualElement _spinnerOverlay;

    private List<Flashcard> _allCards;
    private int _currentIndex;
    private string _currentEditedId;

    private NetworkService _networkService;

    public void Initialize(NetworkService networkService)
    {
        _networkService = networkService;
        _currentIndex = 0; // randomize it

        AssignUiElements();
        EnableButtons();
        
        InitializeAsync().Forget();
    }

    private void AssignUiElements()
    {
        _polishLabel = _root.Q<Label>("label-pl");
        _spanishLabel = _root.Q<Label>("label-es");
        _revealButton = _root.Q<Button>("btn-reveal");
        _addButton = _root.Q<Button>("btn-add");
        _editButton = _root.Q<Button>("btn-update");
        _saveUpdateButton = _root.Q<Button>("btn-save");
        _cancelCardChangeButton = _root.Q<Button>("btn-cancel");
        _polishInput = _root.Q<TextField>("input-pl");
        _spanishInput = _root.Q<TextField>("input-es");
        _addUpdateCardScreen = _root.Q<VisualElement>("add-update-card-screen");
        _deleteButton = _root.Q<Button>("btn-delete");
        _errorLabel = _root.Q<Label>("error-message");
        _reconnectButton = _root.Q<Button>("btn-retry");
        _errorsContainer = _root.Q<VisualElement>("errors-container");
        _spinnerOverlay = _root.Q<VisualElement>("spinner-overlay");
        
        if (_spanishLabel != null)
        {
            _spanishLabel.style.opacity = 0;
            _spanishLabel.style.display = DisplayStyle.Flex;
        }

        _errorsContainer.style.opacity = 0;
        _errorsContainer.style.display = DisplayStyle.Flex;
    }

    private async UniTaskVoid InitializeAsync()
    {
        await FetchCardsAsync();
        DisplayCurrentCard();
    }
    
    private void EnableButtons()
    {
        if (_revealButton != null) _revealButton.clicked += OnRevealButtonClicked;
        if (_deleteButton != null) _deleteButton.clicked += OnDeleteButtonClicked;
        if (_addButton != null) _addButton.clicked += OnAddCardButtonClicked;
        if (_saveUpdateButton != null) _saveUpdateButton.clicked += OnSaveButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked += OnRetryButtonClicked;
        if (_editButton != null) _editButton.clicked += OnEditCardButtonClicked;
        if (_cancelCardChangeButton != null) _cancelCardChangeButton.clicked += OnCancelCardChangeClicked;
    }

    private void OnDisable()
    {
        if (_revealButton != null) _revealButton.clicked -= OnRevealButtonClicked;
        if (_deleteButton != null) _deleteButton.clicked -= OnDeleteButtonClicked;
        if (_addButton != null) _addButton.clicked -= OnAddCardButtonClicked;
        if (_saveUpdateButton != null) _saveUpdateButton.clicked -= OnSaveButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked -= OnRetryButtonClicked;
        if (_editButton != null) _editButton.clicked -= OnEditCardButtonClicked;
        if (_cancelCardChangeButton != null) _cancelCardChangeButton.clicked -= OnCancelCardChangeClicked;
    }

    private void OnRevealButtonClicked()
    {
        if (_spanishLabel.style.opacity == 0)
        {
            _spanishLabel.style.opacity = 1;
            _revealButton.text = "Hide";
        }
        else
        {
            ShowNextCard();
        }
    }

    private void OnDeleteButtonClicked()
    {
        string idToDelete = _allCards[_currentIndex].Id;
        DeleteCardAsync(idToDelete).Forget();
    }

    private void OnAddCardButtonClicked()
    {
        _currentEditedId = null;
        
        _polishInput.value = "";
        _spanishInput.value = "";
        _addUpdateCardScreen.style.backgroundColor = Color.limeGreen;
        _addUpdateCardScreen.style.display = DisplayStyle.Flex;
    }

    private void OnEditCardButtonClicked()
    {
        Flashcard currentCard = _allCards[_currentIndex];
        _currentEditedId = currentCard.Id;
        
        _polishInput.value = currentCard.Polish;
        _spanishInput.value = currentCard.Spanish;
        
        _saveUpdateButton.text = "Update";
        _addUpdateCardScreen.style.backgroundColor = Color.deepSkyBlue;
        _addUpdateCardScreen.style.display = DisplayStyle.Flex;
    }
    
    private void OnCancelCardChangeClicked()
    {
        _addUpdateCardScreen.style.display = DisplayStyle.None;
    }

    private void OnRetryButtonClicked()
    {
        FetchCardsAsync().Forget();
    }
    
    private void OnSaveButtonClicked()
    {
        string polish = _polishInput.text;
        string spanish = _spanishInput.text;

        var flashcard = new Flashcard 
        {
            Polish = polish,
            Spanish = spanish,
        };
        
        if (_currentEditedId != null)
        {
            flashcard.Id = _currentEditedId;
            UpdateCardAsync(flashcard).Forget();
        }
        else
        {
            AddCardAsync(flashcard).Forget();
        }

        _addUpdateCardScreen.style.display = DisplayStyle.None;
    }

    private void ToggleSpinner(bool show)
    {
        _spinnerOverlay.style.visibility = show ? Visibility.Visible : Visibility.Hidden;
    }

    private void ShowNextCard()
    {
        _currentIndex++;
        if (_currentIndex >= _allCards.Count)
        {
            _currentIndex = 0;
        }
        DisplayCurrentCard();
    }

    private void DisplayCurrentCard()
    {
        if (_allCards == null)
            return;
        
        Flashcard card = _allCards[_currentIndex];
        SetupCard(card);
    }

    private void SetupCard(Flashcard flashcard)
    {
        _polishLabel.text = flashcard.Polish;
        _spanishLabel.text = flashcard.Spanish;

        _spanishLabel.style.opacity = 0;
        _revealButton.text = "Reveal";
    }

    private void ShowError(string failedToFetchCards)
    {
        _root.Q<VisualElement>("card-container").style.opacity = 0;
        _root.Q<VisualElement>("errors-container").style.opacity = 1;
        
        _errorLabel.text = failedToFetchCards;
    }

    private void HideError()
    {
        _root.Q<VisualElement>("card-container").style.opacity = 1;
        _root.Q<VisualElement>("errors-container").style.opacity = 0;
    }
    
    private async UniTask FetchCardsAsync()
    {
        ToggleSpinner(true);
        try
        {
            _allCards = await _networkService.FetchCardsAsync();
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to fetch cards");
        }
        finally
        {
            ToggleSpinner(false);
        }

        if (_allCards != null)
        {
            HideError();
        }
    }
    
    private async UniTaskVoid DeleteCardAsync(string id)
    {
        ToggleSpinner(true);
        try
        {
            await _networkService.DeleteCardAsync(id);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to delete card");
        }
        finally
        {
            ToggleSpinner(false);
        }

        InitializeAsync().Forget();
    }
    
    private async UniTaskVoid AddCardAsync(Flashcard flashcard)
    {
        ToggleSpinner(true);
        try
        {
            await _networkService.AddCardAsync(flashcard);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to add card");
        }
        finally
        {
            ToggleSpinner(false);
        }

        InitializeAsync().Forget();
    }
    
    private async UniTaskVoid UpdateCardAsync(Flashcard flashcard)
    {
        ToggleSpinner(true);
        try
        {
            await _networkService.UpdateCardAsync(flashcard);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to update card");
        }
        finally
        {
            ToggleSpinner(false);
        }

        InitializeAsync().Forget();
    }
}
