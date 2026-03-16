using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FlashcardController : MonoBehaviour
{
    public event Action OnRetryRequested;
    public event Action<int> OnDeleteRequested;
    public event Action<Flashcard> OnAddRequested;
    
    private Label _polishLabel;
    private TextField _polishInput;
    private Label _spanishLabel;
    private TextField _spanishInput;
    private Label _errorLabel;
    private Button _reconnectButton;
    private Button _revealButton;
    private Button _addButton;
    private Button _deleteButton;
    private Button _saveButton;
    private VisualElement _errorsContainer;
    private VisualElement _addCardScreen;
    
    private List<Flashcard> _allCards;
    private int _currentIndex;

    private VisualElement _root => GetComponent<UIDocument>().rootVisualElement;

    private void Awake()
    {
        _polishLabel = _root.Q<Label>("label-pl");
        _spanishLabel = _root.Q<Label>("label-es");
        _revealButton = _root.Q<Button>("btn-reveal");
        _addButton = _root.Q<Button>("btn-add");
        _saveButton = _root.Q<Button>("btn-save");
        _polishInput = _root.Q<TextField>("input-pl");
        _spanishInput = _root.Q<TextField>("input-es");
        _addCardScreen = _root.Q<VisualElement>("add-card-screen");
        _deleteButton = _root.Q<Button>("btn-delete");
        _errorLabel = _root.Q<Label>("error-message");
        _reconnectButton = _root.Q<Button>("btn-retry");
        _errorsContainer = _root.Q<VisualElement>("errors-container");

        if (_spanishLabel != null)
        {
            _spanishLabel.style.opacity = 0;
            _spanishLabel.style.display = DisplayStyle.Flex;
        }

        _errorsContainer.style.opacity = 0;
        _errorsContainer.style.display = DisplayStyle.Flex;
        _revealButton.clicked += OnRevealButtonClicked;
        _deleteButton.clicked += OnDeleteButtonClicked;
        _addButton.clicked += OnAddCardButtonClicked;
        _saveButton.clicked += OnAddButtonClicked;

    _reconnectButton.clicked += () =>
        {
            OnRetryRequested?.Invoke();
        };
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
        int idToDelete = _allCards[_currentIndex].Id;
        OnDeleteRequested?.Invoke(idToDelete);
    }

    private void OnAddCardButtonClicked()
    {
        _addCardScreen.style.display = DisplayStyle.Flex;
    }
    
    private void OnAddButtonClicked()
    {
        string polish = _polishInput.text;
        string spanish = _spanishInput.text;

        var newFlashcard = new Flashcard
        {
            Polish = polish,
            Spanish = spanish,
        };
        
        OnAddRequested?.Invoke(newFlashcard);
        
        _addCardScreen.style.display = DisplayStyle.None;
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

    public void Initialize(List<Flashcard> cards)
    {
        _allCards = cards;
        _currentIndex = 0;
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

    public void ShowError(string failedToFetchCards)
    {
        _root.Q<VisualElement>("card-container").style.opacity = 0;
        _root.Q<VisualElement>("errors-container").style.opacity = 1;
        
        _errorLabel.text = failedToFetchCards;
    }
    
    public void HideError()
    {
        _root.Q<VisualElement>("card-container").style.opacity = 1;
        _root.Q<VisualElement>("errors-container").style.opacity = 0;
    }
}
