using UnityEngine;

public class AppBootstrapper: MonoBehaviour
{
    [SerializeField] private FlashcardController _flashcardController;
    
    public void Start()
    {
        NetworkService networkService = new NetworkService();
        _flashcardController.Initialize(networkService);
    }
}