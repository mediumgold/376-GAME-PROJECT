using UnityEngine;
using UnityEngine.UIElements;

public class EscapeChoiceUI : MonoBehaviour
{
    public static EscapeChoiceUI Instance { get; private set; }

    [Header("UI Element Names")]
    [SerializeField] private string escapeRootName = "EscapeChoice";
    [SerializeField] private string monsterButtonName = "Monster";
    [SerializeField] private string hallucinationButtonName = "Hallucination";

    [Header("Hunger Drain")]
    [SerializeField] private float hungerTickInterval = 1f;   // seconds between drains
    [SerializeField] private float hungerPerTick = -10f;      // -10 hunger per tick

    [Header("Optional USS Class For Selected Button")]
    [SerializeField] private string selectedClass = "shop-selected";

    [Header("Dialogue for escape outcomes")]
    [SerializeField] private DialogueObject monsterCorrectDialogue;        // bad NPC + chose Monster
    [SerializeField] private DialogueObject monsterWrongDialogue;          // good NPC + chose Monster
    [SerializeField] private DialogueObject hallucinationCorrectDialogue;  // good NPC + chose Hallucination
    [SerializeField] private DialogueObject hallucinationWrongDialogue;    // bad NPC + chose Hallucination


    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement escapeRoot;
    private Button monsterButton;
    private Button hallucinationButton;

    private int currentIndex;          // 0 = Monster, 1 = Hallucination
    private bool isOpen;
    private float hungerTimer;

    private Player player;
    private Manananggal currentMonster;
    private bool npcWasGood;           // true = flashed NPC is good, false = bad

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("EscapeChoiceUI: No UIDocument found on this GameObject.");
            return;
        }

        root = uiDoc.rootVisualElement;
        escapeRoot = root.Q<VisualElement>(escapeRootName);
        if (escapeRoot == null)
        {
            Debug.LogError($"EscapeChoiceUI: Could not find VisualElement #{escapeRootName} in UXML.");
            return;
        }

        monsterButton = escapeRoot.Q<Button>(monsterButtonName);
        hallucinationButton = escapeRoot.Q<Button>(hallucinationButtonName);

        if (monsterButton == null || hallucinationButton == null)
        {
            Debug.LogError("EscapeChoiceUI: Could not find Monster / Hallucination buttons. Check their name attributes.");
        }

        escapeRoot.style.display = DisplayStyle.None;
        player = FindAnyObjectByType<Player>();
    }

    private void Update()
    {
        if (!isOpen || player == null)
            return;

        // If the player died while in escape (e.g., from hunger drain),
        // immediately force-close this UI.
        if (player.m_currentHunger <= 0f)
        {
            CloseDueToDeath();
            return;
        }

        hungerTimer -= Time.deltaTime;
        if (hungerTimer <= 0f)
        {
            player.changeHunger(hungerPerTick);
            SoundManager.Instance?.PlaySFX("player_damaged");
            // Trigger red flash when hunger drains
            FlashManager.Instance?.FlashDamage();
            hungerTimer = hungerTickInterval;

            // Re-check death after the drain
            if (player.m_currentHunger <= 0f)
            {
                CloseDueToDeath();
                return;
            }
        }
    }

    /// <summary>
    /// Called by Manananggal when it touches the player.
    /// </summary>
    public void Open(Manananggal monster, bool npcIsGood)
    {
        if (escapeRoot == null) return;

        currentMonster = monster;
        npcWasGood = npcIsGood;

        isOpen = true;
        hungerTimer = hungerTickInterval;

        escapeRoot.style.display = DisplayStyle.Flex;

        currentIndex = 0;
        UpdateSelectionVisual();

        if (InputManager.Instance != null)
            InputManager.Instance.SetEscapeChoiceMode(true);
    }

    public void MoveSelection(int direction)
    {
        if (!isOpen) return;

        currentIndex += direction;   // direction is -1 or +1
        if (currentIndex < 0) currentIndex = 1;
        if (currentIndex > 1) currentIndex = 0;

        UpdateSelectionVisual();
    }

    public void ConfirmSelection()
    {
        if (!isOpen || player == null)
            return;

        bool choseMonster = (currentIndex == 0);

        // ----- Apply gameplay effects -----
        if (npcWasGood)
        {
            // Flashed NPC is GOOD → hallucination
            if (choseMonster)
            {
                // You attack an innocent
                player.changeHunger(-20f);
                SoundManager.Instance?.PlaySFX("player_damaged");
                Debug.Log("EscapeChoice: You thought the real person was the Manannanggal. The person slapped you -20 health.");
            }
            else
            {
                // You correctly treat it as hallucination
                player.changeHunger(+20f);
                SoundManager.Instance?.PlaySFX("player_damaged");
                Debug.Log("EscapeChoice: You didnt fall victim to your hallucination. +20 health.");
            }
        }
        else
        {
            // Flashed NPC is BAD → real monster
            if (choseMonster)
            {
                // Correct call: it's a monster
                player.changeHunger(+20f);
                SoundManager.Instance?.PlaySFX("player_damaged");
                Debug.Log("EscapeChoice: You saw through the Manananggal's tricks, you grabbed some of its flesh and ate it. +20 health.");
            }
            else
            {
                // You thought it was just a hallucination
                player.changeHunger(-50f);
                SoundManager.Instance?.PlaySFX("player_damaged");
                Debug.Log("EscapeChoice: You thought it was a hallucination but it was actually the Manannanggal -50 health.");
            }
        }

        // ----- If this killed the player, bail out of escape UI immediately -----
        if (player.m_currentHunger <= 0f)
        {
            // Dialogue/game-over for death is handled by GameFlowManager,
            // we just need to get rid of the escape UI.
            CloseDueToDeath();
            return;
        }

        // ----- Trigger dialogue for this outcome (single, correct call) -----
        PlayEscapeDialogue(npcWasGood, choseMonster);

        Close();
    }

    private void Close()
    {
        if (!isOpen) return;

        isOpen = false;

        if (escapeRoot != null)
            escapeRoot.style.display = DisplayStyle.None;

        if (InputManager.Instance != null)
            InputManager.Instance.SetEscapeChoiceMode(false);

        // Normal escape resolution: tell the monster we're done with the mini-game
        if (currentMonster != null)
        {
            currentMonster.OnEscapeChoiceResolved();
            currentMonster = null;
        }
    }

    /// <summary>
    /// Special close path when the player dies (hunger <= 0) during escape.
    /// We nuke the UI and clear escape mode, but we DON'T resolve the monster –
    /// the global lose logic will take over and load mainmenu.
    /// </summary>
    private void CloseDueToDeath()
    {
        if (!isOpen) return;

        isOpen = false;

        if (escapeRoot != null)
            escapeRoot.style.display = DisplayStyle.None;

        if (InputManager.Instance != null)
            InputManager.Instance.SetEscapeChoiceMode(false);

        // Leave currentMonster alone; scene/game-over flow will handle the rest.
        currentMonster = null;
    }

    private void UpdateSelectionVisual()
    {
        if (monsterButton == null || hallucinationButton == null)
            return;

        if (!string.IsNullOrEmpty(selectedClass))
        {
            monsterButton.RemoveFromClassList(selectedClass);
            hallucinationButton.RemoveFromClassList(selectedClass);

            if (currentIndex == 0)
                monsterButton.AddToClassList(selectedClass);
            else
                hallucinationButton.AddToClassList(selectedClass);
        }

        // Give focus to help with keyboard navigation
        if (currentIndex == 0)
            monsterButton.Focus();
        else
            hallucinationButton.Focus();
    }

    private void PlayEscapeDialogue(bool npcIsGood, bool choseMonster)
    {
        if (DialogueUI.Instance == null)
            return;

        DialogueObject dialogueToPlay = null;

        if (npcIsGood)
        {
            // Good NPC = hallucination
            dialogueToPlay = choseMonster
                ? monsterWrongDialogue          // you attacked an innocent hallucination
                : hallucinationCorrectDialogue; // you correctly guessed hallucination
        }
        else
        {
            // Bad NPC = real monster
            dialogueToPlay = choseMonster
                ? monsterCorrectDialogue        // you correctly attacked the monster
                : hallucinationWrongDialogue;   // you trusted a real monster
        }

        if (dialogueToPlay != null)
        {
            DialogueUI.Instance.ShowDialogue(dialogueToPlay);
        }
    }
}
