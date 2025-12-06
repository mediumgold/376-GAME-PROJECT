using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    [Header("Game Duration (seconds)")]
    [SerializeField] private float gameDuration = 300f; // 5 minutes

    [Header("Win / Lose Dialogues")]
    [SerializeField] private DialogueObject winDialogue;
    [SerializeField] private DialogueObject loseDialogue;

    private Player player;
    private InputManager inputManager;

    private float timer;
    private bool gameEnded;
    private bool waitingForDialogueEnd;

    private void Awake()
    {
        // Find references if not assigned manually
        player = FindAnyObjectByType<Player>();
        inputManager = InputManager.Instance != null
            ? InputManager.Instance
            : FindAnyObjectByType<InputManager>();

        if (player == null)
        {
            Debug.LogError("GameFlowManager: No Player found in scene.");
        }
    }

    private void Start()
    {
        timer = gameDuration;
    }

    private void Update()
    {
        if (player == null) return;

        // If we've already entered win/lose state, just wait for dialogue to finish
        if (gameEnded)
        {
            if (waitingForDialogueEnd)
            {
                CheckDialogueFinished();
            }
            return;
        }

        // ----- Lose condition: hunger hits 0 before time is up -----
        if (player.m_currentHunger <= 0f)
        {
            TriggerLose();
            return;
        }

        // ----- Win condition: survive full duration with hunger > 0 -----
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            // Timer is up: if still alive (hunger > 0), win
            if (player.m_currentHunger > 0f)
            {
                TriggerWin();
            }
            else
            {
                // Edge case: timer hit 0 in same frame hunger dropped to 0
                TriggerLose();
            }
        }
    }

    private void TriggerWin()
    {
        gameEnded = true;
        waitingForDialogueEnd = true;

        if (inputManager != null)
        {
            inputManager.SetWinState(true);
        }

        // Play win SFX
        SoundManager.Instance?.PlaySFX("win_sound");

        if (winDialogue != null && DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowDialogue(winDialogue);
        }
        else
        {
            Debug.LogWarning("GameFlowManager: Win dialogue or DialogueUI missing, loading mainmenu immediately.");
            LoadMainMenu();
        }
    }

    private void TriggerLose()
    {
        gameEnded = true;
        waitingForDialogueEnd = true;

        if (inputManager != null)
        {
            inputManager.SetLoseState(true);
        }

        // Play lose SFX
        SoundManager.Instance?.PlaySFX("lose_sound");

        if (loseDialogue != null && DialogueUI.Instance != null)
        {
            DialogueUI.Instance.ShowDialogue(loseDialogue);
        }
        else
        {
            Debug.LogWarning("GameFlowManager: Lose dialogue or DialogueUI missing, loading mainmenu immediately.");
            LoadMainMenu();
        }
    }

    /// <summary>
    /// Checks if the dialogue UI has finished, then loads main menu.
    /// We piggyback on InputManager.InDialogue since your dialogue system
    /// already toggles that.
    /// </summary>
    private void CheckDialogueFinished()
    {
        if (inputManager == null)
        {
            // No input manager, just bail to menu
            LoadMainMenu();
            return;
        }

        // When dialogue closes, InputManager.InDialogue should go false
        if (!inputManager.InDialogue)
        {
            LoadMainMenu();
        }
    }

    private void LoadMainMenu()
    {
        waitingForDialogueEnd = false;

        // Optional: clear win/lose flags before leaving
        if (inputManager != null)
        {
            inputManager.SetWinState(false);
            inputManager.SetLoseState(false);
        }

        SceneManager.LoadScene("mainMenu");
    }
}
