using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;

public enum GameModes
{
    PL_TO_ES,
    ES_TO_PL
}

public class AppBootstrapper: MonoBehaviour
{
    [SerializeField] private FlashcardController _flashcardController;
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private VisualTreeAsset _menuAsset;
    [SerializeField] private VisualTreeAsset _flashcardAsset;
    
    private NetworkService _networkService;
    private IAssetService _assetService;
    private VisualElement _root;
    private event Action _clickedPolishToSpanish;
    private event Action _clickedSpanishToPolish;

    private void OnEnable()
    {
        if (_flashcardController != null) _flashcardController.OnBackRequested += ShowMenu;
    }

    private void OnDisable()
    {
        if (_flashcardController != null) _flashcardController.OnBackRequested -= ShowMenu;
    }

    public void Start()
    {
        _networkService = new NetworkService();
        _assetService = new AddressableAssetService();
        _root = _uiDocument.rootVisualElement;

        _flashcardController.OnBackRequested += ShowMenu;
        
        ShowMenu();
    }

    private void ShowMenu()
    {
        FullUiRootClearing();
        _root.Add(_menuAsset.CloneTree());

        _clickedPolishToSpanish = () => HandleStart(GameModes.PL_TO_ES);
        _root.Q<Button>("btn-polish-to-spanish").clicked += _clickedPolishToSpanish;

        _clickedSpanishToPolish = () => HandleStart(GameModes.ES_TO_PL);
        _root.Q<Button>("btn-spanish-to-polish").clicked += _clickedSpanishToPolish;
    }

    private void HandleStart(GameModes gameMode)
    {
        _root.Q<Button>("btn-polish-to-spanish").clicked -= _clickedPolishToSpanish;
        _root.Q<Button>("btn-spanish-to-polish").clicked -= _clickedSpanishToPolish;
        StartGame(gameMode);
    }

    private void StartGame(GameModes gameMode)
    {
        FullUiRootClearing();
        _root.Add(_flashcardAsset.CloneTree());

        if (gameMode == GameModes.PL_TO_ES)
        {
            _root.AddToClassList("theme-pl");
        }
        else if (gameMode == GameModes.ES_TO_PL)
        {
            _root.AddToClassList("theme-es");
        }
        
        _flashcardController.Initialize(_networkService, _assetService, gameMode);
    }

    private void FullUiRootClearing()
    {
        _root.Clear();
        _root.RemoveFromClassList("theme-es");
        _root.RemoveFromClassList("theme-pl");
    }
}