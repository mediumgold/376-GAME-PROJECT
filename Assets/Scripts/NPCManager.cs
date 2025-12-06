using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public static NPCManager Instance { get; private set; }

    [Header("Spawn points for regular NPCs (20)")]
    [SerializeField] private Transform[] spawnPoints;   // 20 transforms you place in scene

    [Header("NPC visual prefabs (10 unique textures)")]
    [SerializeField] private NPC[] npcPrefabs;          // 10 prefabs, each with NPC script + different texture

    [Header("Dialogue Objects")]
    [SerializeField] private DialogueObject[] goodDialogues; // 5 unique ScriptableObjects for GOOD NPCs
    [SerializeField] private DialogueObject badDialogue;     // 1 shared ScriptableObject for all BAD NPCs

    // Runtime list of the actual NPC instances in the scene
    private readonly List<NPC> spawnedNpcs = new List<NPC>();
    public IReadOnlyList<NPC> SpawnedNpcs => spawnedNpcs;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (npcPrefabs == null || npcPrefabs.Length != 10)
        {
            Debug.LogError("NPCManager: npcPrefabs must contain exactly 10 NPC prefabs.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length < npcPrefabs.Length)
        {
            Debug.LogError("NPCManager: need at least as many spawn points as NPC prefabs.");
            return;
        }

        if (goodDialogues == null || goodDialogues.Length != 5)
        {
            Debug.LogError("NPCManager: goodDialogues must contain exactly 5 DialogueObjects.");
            return;
        }

        if (badDialogue == null)
        {
            Debug.LogError("NPCManager: badDialogue is not assigned.");
            return;
        }

        SpawnAndAssignNPCs();
    }

    private void SpawnAndAssignNPCs()
    {
        spawnedNpcs.Clear();

        int npcCount = npcPrefabs.Length;

        // 1) Randomize which spawn points get used, then take the first 10.
        int[] spawnIndices = BuildIndexArray(spawnPoints.Length);
        Shuffle(spawnIndices);

        // 2) Randomize prefab order so textures aren't tied to specific spawns.
        NPC[] shuffledPrefabs = (NPC[])npcPrefabs.Clone();
        Shuffle(shuffledPrefabs);

        // 3) Build type list: 5 good, 5 bad, then shuffle so type is random per NPC each run.
        NPC.NPCType[] typeAssignments = new NPC.NPCType[npcCount];
        for (int i = 0; i < npcCount; i++)
        {
            typeAssignments[i] = (i < 5) ? NPC.NPCType.good : NPC.NPCType.bad;
        }
        Shuffle(typeAssignments);

        // 4) Shuffle the good dialogues so which good NPC gets which is random.
        DialogueObject[] shuffledGoodDialogues = (DialogueObject[])goodDialogues.Clone();
        Shuffle(shuffledGoodDialogues);
        int goodDialogueIndex = 0;

        // 5) Instantiate NPCs with randomized spawn, type, and dialogue.
        for (int i = 0; i < npcCount; i++)
        {
            int spawnIndex = spawnIndices[i]; // first 10 indices after shuffle
            Transform spawn = spawnPoints[spawnIndex];

            NPC prefab = shuffledPrefabs[i];
            NPC npcInstance = Instantiate(prefab, spawn.position, spawn.rotation);

            NPC.NPCType type = typeAssignments[i];
            DialogueObject dialogueForThisNPC;

            if (type == NPC.NPCType.good)
            {
                // assign one of the 5 good dialogues, each used once
                dialogueForThisNPC = shuffledGoodDialogues[goodDialogueIndex];
                goodDialogueIndex++;
            }
            else
            {
                // all bad NPCs share the same dialogue
                dialogueForThisNPC = badDialogue;
            }

            // Your existing configure method on NPC
            npcInstance.Configure(type, dialogueForThisNPC);

            // Track this instance so Manananggal can use it for the flash
            spawnedNpcs.Add(npcInstance);
        }
    }

    // ---------- Utility helpers ----------

    private int[] BuildIndexArray(int length)
    {
        int[] arr = new int[length];
        for (int i = 0; i < length; i++)
        {
            arr[i] = i;
        }
        return arr;
    }

    private void Shuffle<T>(T[] array)
    {
        for (int i = 0; i < array.Length - 1; i++)
        {
            int swapIndex = Random.Range(i, array.Length);
            T tmp = array[i];
            array[i] = array[swapIndex];
            array[swapIndex] = tmp;
        }
    }
}
