using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class FlashcardController : MonoBehaviour
{
    public event Action OnBackRequested;
    
    private VisualElement _root => GetComponent<UIDocument>().rootVisualElement;
    
    private Label _topLabel;
    private Label _bottomLabel;
    private Label _errorLabel;
    private TextField _polishInput;
    private TextField _spanishInput;
    private Button _reconnectButton;
    private Button _revealButton;
    private Button _addFlashcardButton;
    private Button _deleteButton;
    private Button _saveUpdateButton;
    private Button _cancelCardChangeButton;
    private Button _editButton;
    private Button _favouriteButton;
    private Button _addNoteButton;
    private Button _backCardButton;
    private Button _backNoteButton;
    private Button _saveNoteButton;
    
    private VisualElement _errorsContainer;
    private VisualElement _addUpdateCardScreen;
    private VisualElement _spinnerOverlay;
    private VisualElement _cardScreen;
    private VisualElement _noteScreen;
    private TextField _noteTextfield;

    private List<Flashcard> _allCards;
    private int _currentDrawnCardIndex;
    private string _originalNote;
    private string _currentEditedId;
    private GameModes _gameMode;

    private NetworkService _networkService;
    private IAssetService _assetService;
    
    private Sprite _favouriteSprite;

    public void Initialize(
        NetworkService networkService,
        IAssetService assetService,
        GameModes gameMode)
    {
        _networkService = networkService;
        _assetService = assetService;
        _gameMode = gameMode;

        AssignUiElements();
        EnableButtons();
        
        InitializeAsync().Forget();
    }

    private void AssignUiElements()
    {
        _topLabel = _root.Q<Label>("label-pl");
        _bottomLabel = _root.Q<Label>("label-es");
        _revealButton = _root.Q<Button>("btn-reveal");
        _addFlashcardButton = _root.Q<Button>("btn-add-flashcard");
        _addNoteButton = _root.Q<Button>("btn-add-note");
        _editButton = _root.Q<Button>("btn-update");
        _backCardButton = _root.Q<Button>("btn-card-back");
        _backNoteButton = _root.Q<Button>("btn-note-back");
        _favouriteButton = _root.Q<Button>("btn-favourite");
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
        _cardScreen = _root.Q<VisualElement>("card-screen");
        _noteScreen = _root.Q<VisualElement>("note-screen");
        _noteTextfield = _root.Q<TextField>("note-textfield");
        _saveNoteButton = _root.Q<Button>("btn-save-note");
        
        if (_bottomLabel != null)
        {
            _bottomLabel.style.opacity = 0;
            _bottomLabel.style.display = DisplayStyle.Flex;
        }

        _errorsContainer.style.opacity = 0;
        _errorsContainer.style.display = DisplayStyle.Flex;

        _cardScreen.style.display = DisplayStyle.Flex;
        _noteScreen.style.display = DisplayStyle.None;
    }

    private async UniTaskVoid InitializeAsync()
    {
        _favouriteSprite = await _assetService.LoadSprite("Icon/FavouriteIcon");

        if (_favouriteButton != null && _favouriteSprite != null)
        {
            _favouriteButton.style.backgroundImage = new StyleBackground(_favouriteSprite);
        }
        
        _allCards = await FetchCardsAsync();
        DisplayRandomCard(_allCards);
    }
    
    private void EnableButtons()
    {
        if (_revealButton != null) _revealButton.clicked += OnRevealButtonClicked;
        if (_deleteButton != null) _deleteButton.clicked += OnDeleteButtonClicked;
        if (_addFlashcardButton != null) _addFlashcardButton.clicked += OnAddFlashcardCardButtonClicked;
        if (_saveUpdateButton != null) _saveUpdateButton.clicked += OnSaveFlashcardButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked += OnRetryButtonClicked;
        if (_editButton != null) _editButton.clicked += OnEditCardButtonClicked;
        if (_cancelCardChangeButton != null) _cancelCardChangeButton.clicked += OnCancelCardChangeClicked;
        if (_favouriteButton != null) _favouriteButton.clicked += OnFavouriteButtonClicked;
        if (_backCardButton != null) _backCardButton.clicked += OnBackCardButtonClicked;
        if (_backNoteButton != null) _backNoteButton.clicked += OnBackNoteButtonClicked; 
        if (_addNoteButton != null) _addNoteButton.clicked += OnAddNoteButtonClicked;
        if (_saveNoteButton != null) _saveNoteButton.clicked += OnSaveNoteButtonClicked;
    }

    private void OnDisable()
    {
        if (_revealButton != null) _revealButton.clicked -= OnRevealButtonClicked;
        if (_deleteButton != null) _deleteButton.clicked -= OnDeleteButtonClicked;
        if (_addFlashcardButton != null) _addFlashcardButton.clicked -= OnAddFlashcardCardButtonClicked;
        if (_saveUpdateButton != null) _saveUpdateButton.clicked -= OnSaveFlashcardButtonClicked;
        if (_reconnectButton != null) _reconnectButton.clicked -= OnRetryButtonClicked;
        if (_editButton != null) _editButton.clicked -= OnEditCardButtonClicked;
        if (_cancelCardChangeButton != null) _cancelCardChangeButton.clicked -= OnCancelCardChangeClicked;
        if (_favouriteButton != null) _favouriteButton.clicked -= OnFavouriteButtonClicked;
        if (_backCardButton != null) _backCardButton.clicked -= OnBackCardButtonClicked;
        if (_backNoteButton != null) _backNoteButton.clicked -= OnBackNoteButtonClicked;
        if (_addNoteButton != null) _addNoteButton.clicked -= OnAddNoteButtonClicked;
        if (_saveNoteButton != null) _saveNoteButton.clicked += OnSaveNoteButtonClicked;
        
        if (_favouriteSprite != null) _assetService.ReleaseAsset(_favouriteSprite);
    }

    private void OnRevealButtonClicked()
    {
        if (_bottomLabel.style.opacity == 0)
        {
            _bottomLabel.style.opacity = 1;
            _revealButton.text = "Hide";
        }
        else
        {
            DisplayRandomCard(_allCards);
        }
    }
    
    private void OnFavouriteButtonClicked()
    {
        string idToMark = _allCards[_currentDrawnCardIndex].Id;
        bool currentState = _allCards[_currentDrawnCardIndex].IsFavourite;
        MarkAsFavouriteAsync(idToMark, !currentState).Forget();
    }

    private void OnBackCardButtonClicked()
    {
        OnBackRequested?.Invoke();
    }

    private void OnBackNoteButtonClicked()
    {
        ToggleCardNoteScreens(true);
    }

    private void OnAddNoteButtonClicked()
    {
        ToggleCardNoteScreens(false);
    }

    private void ToggleCardNoteScreens(bool showCard) // zmien na enuma
    {
        _cardScreen.style.display = showCard ? DisplayStyle.Flex : DisplayStyle.None;
        _noteScreen.style.display = showCard ? DisplayStyle.None : DisplayStyle.Flex;

        if (!showCard)
        {
            _saveNoteButton.style.opacity = 0;
            _noteTextfield.RegisterValueChangedCallback(n => CheckIfNoteChanged(n.newValue));
            
            _noteTextfield.value = _allCards[_currentDrawnCardIndex].Note;
            _noteTextfield.Focus();
            _noteTextfield.SelectNone(); 
            _noteTextfield.cursorIndex = _noteTextfield.value?.Length ?? 0;
        }
    }

    private void CheckIfNoteChanged(string newNote)
    {
        bool isChanged = _originalNote != newNote;
        _saveNoteButton.style.opacity = isChanged ? 1 : 0;
    }

    private void OnSaveNoteButtonClicked()
    {
        string idToMark = _allCards[_currentDrawnCardIndex].Id;
        string note = _noteTextfield.value;
        SaveNoteAsync(idToMark, note).Forget();
    }

    private void OnDeleteButtonClicked()
    {
        string idToDelete = _allCards[_currentDrawnCardIndex].Id;
        DeleteCardAsync(idToDelete).Forget();
    }

    private void OnAddFlashcardCardButtonClicked()
    {
        _currentEditedId = null;
        
        _polishInput.value = "";
        _spanishInput.value = "";
        _addUpdateCardScreen.style.backgroundColor = Color.limeGreen;
        _addUpdateCardScreen.style.display = DisplayStyle.Flex;
    }

    private void OnEditCardButtonClicked()
    {
        Flashcard currentCard = _allCards[_currentDrawnCardIndex];
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
    
    private void OnSaveFlashcardButtonClicked()
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

    private void DisplayRandomCard(List<Flashcard> cards)
    {
        if (cards == null) 
            return;
        
        int randomIndex = UnityEngine.Random.Range(0, _allCards.Count);
        while (randomIndex == _currentDrawnCardIndex)
        {
            randomIndex = UnityEngine.Random.Range(0, _allCards.Count);
        }

        _currentDrawnCardIndex = randomIndex;
        Flashcard card = _allCards[randomIndex];
        _originalNote = _allCards[_currentDrawnCardIndex].Note;
        SetupCard(card);
    }

    private void SetupCard(Flashcard flashcard)
    {
        if (_gameMode == GameModes.PL_TO_ES)
        {
            _topLabel.text = flashcard.Polish;
            _bottomLabel.text = flashcard.Spanish;
        }
        else
        {
            _topLabel.text = flashcard.Spanish;
            _bottomLabel.text = flashcard.Polish;
        }

        _bottomLabel.style.opacity = 0;
        _revealButton.text = "Reveal";
        
        _favouriteButton.style.unityBackgroundImageTintColor = _allCards[_currentDrawnCardIndex].IsFavourite ? Color.red : Color.gray;
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
    
    private async UniTask<List<Flashcard>> FetchCardsAsync()
    {
        var allCards = new List<Flashcard>();
        ToggleSpinner(true);
        try
        {
            allCards = await _networkService.FetchCardsAsync();
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

        if (allCards != null)
        {
            HideError();
        }

        return allCards;
    }

    private async UniTaskVoid MarkAsFavouriteAsync(string id, bool state)
    {
        ToggleSpinner(true);
        try
        {
            await _networkService.MarkAsFavouriteAsync(id, state);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to mark as favourite");
        }
        finally
        {
            ToggleSpinner(false);
        }
        
        _allCards = await FetchCardsAsync();
        Flashcard card = _allCards[_currentDrawnCardIndex];
        SetupCard(card);
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

    private async UniTask SaveNoteAsync(string id, string note)
    {
        ToggleSpinner(true);
        try
        {
            await _networkService.SaveNoteAsync(id, note);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Operation canceled");
        }
        catch (Exception)
        {
            ShowError("Failed to save note");
        }
        finally
        {
            ToggleSpinner(false);
        }

        _originalNote = _noteTextfield.value;
        CheckIfNoteChanged(_noteTextfield.value);
    }
}
