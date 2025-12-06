using UnityEngine;
using System.Collections;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [SerializeField] private TMP_Text textBox;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private OptionSelectorUI optionUI;

    private InputManager inputManager;
    private TypewriterEffect typewriterEffect;
    private NPC currentNPC;

    public void SetCurrentNPC(NPC npc)
    {
        currentNPC = npc;
    }

    private void Awake()
    {
        Instance = this;
        inputManager = FindAnyObjectByType<InputManager>();
    }

    private void Start()
    {
        optionUI.Initialize(textBox);
        typewriterEffect = GetComponent<TypewriterEffect>();
        CloseDialogueBox();
    }

    public void ShowDialogue(DialogueObject dialogueObject)
    {
        StopAllCoroutines();

        dialogueBox.SetActive(true);
        inputManager.SetDialogueMode(true);

        StartCoroutine(RunDialogue(dialogueObject));
    }

    private IEnumerator RunDialogue(DialogueObject dialogueObject)
    {
        foreach (string dialogue in dialogueObject.Dialogue)
        {
            yield return typewriterEffect.Run(dialogue, textBox);
            yield return new WaitUntil(() => inputManager.onFoot.Interact.WasPerformedThisFrame());
        }

        if (dialogueObject.hasChoices && dialogueObject.choices != null && dialogueObject.choices.Length > 0)
        {
            ShowChoices(dialogueObject.choices);
        }
        else
        {
            EndDialogue();
        }
    }

    private void ShowChoices(string[] choices)
    {
        inputManager.SetOptionSelectMode(true);
        optionUI.ShowOptions(choices, OnChoiceSelected);
    }

    private void OnChoiceSelected(int index)
    {
        inputManager.TriggerInteractCooldown(0.25f);
        inputManager.SetOptionSelectMode(false);

        EndDialogue();

        if (currentNPC == null)
            return;

        // BUY option (index 0). Check if player inventory is full.
        if (index == 0)
        {

            NPCInventory inv = currentNPC.NPCInventory;
            inv.GenerateShopInventoryIfEmpty();

            ShopUI.Instance.ShowBuy(inv.ActiveShopItems);
            inputManager.SetShopMode(true);
        }
        else if (index == 1)
        {
            Inventory playerInv = inputManager.PlayerInventory;

            // take a snapshot of player items for the Sell grid
            var itemsList = new System.Collections.Generic.List<Item>(playerInv.Items.Count);
            foreach (var it in playerInv.Items)
            {
                itemsList.Add(it);
            }

            ShopUI.Instance.ShowSell(itemsList);
            inputManager.SetShopMode(true);
        }
    }



    public void OptionSelect_Move(int dir)
    {
        optionUI.MoveSelection(dir);
    }

    public void OptionSelect_Confirm()
    {
        optionUI.Confirm();
    }

    private void EndDialogue()
    {
        inputManager.SetDialogueMode(false);
        CloseDialogueBox();
    }

    private void CloseDialogueBox()
    {
        dialogueBox.SetActive(false);
        textBox.text = string.Empty;
    }

    public void HandleCloseFromInput()
    {
        // Stop any running dialogue coroutine (typewriter, waiting for input, etc.)
        StopAllCoroutines();

        // Make sure option-select mode is turned off
        if (inputManager != null)
        {
            inputManager.SetOptionSelectMode(false);
            inputManager.SetDialogueMode(false);
        }

        // Hide the dialogue box and clear text
        CloseDialogueBox();
    }
}
