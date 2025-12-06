using UnityEngine;

public class NPC : Interactable
{
    
    [SerializeField] public enum NPCType
    {
        good,
        bad,
        pawn,
        dealer,
        Convenience
    }
    [SerializeField] private DialogueObject NPCdialogue;
    private DialogueUI dialogueUI;
    public NPCInventory NPCInventory;

    [SerializeField] private NPCType npcType;
    public NPCType Type => npcType;

    private void Awake()
    {
        dialogueUI = FindAnyObjectByType<DialogueUI>();
        NPCInventory = GetComponent<NPCInventory>();
    }
    protected override void Interact()
    {
        Debug.Log("interacted with " + gameObject.name + "");
        DialogueUI.Instance.SetCurrentNPC(this);
        dialogueUI.ShowDialogue(NPCdialogue);

    }
        // NEW: used by the spawner to set this NPC up each run
    public void Configure(NPCType type, DialogueObject dialogue)
    {
        npcType = type;
        NPCdialogue = dialogue;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
