using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class StartGameLoader : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "Garden";
        
    private PlayerInput playerInput;
    private InputAction startGameAction;

    private void Awake()
    {
        // Create input action for starting the game
        startGameAction = new InputAction("StartGame", InputActionType.Button, "<Keyboard>/space");
        startGameAction.performed += OnStartGamePerformed;
    }

    private void OnEnable()
    {
        startGameAction.Enable();
    }

    private void OnDisable()
    {
        startGameAction.Disable();
    }

    private void OnDestroy()
    {
        startGameAction.performed -= OnStartGamePerformed;
        startGameAction.Dispose();
    }

    private void OnStartGamePerformed(InputAction.CallbackContext context)
    {
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        SceneManager.LoadScene(gameSceneName);
    }
}