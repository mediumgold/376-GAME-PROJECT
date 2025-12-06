using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Manananggal : MonoBehaviour
{
    [Header("References")]
    private Animator anim;
    private Player player;
    private Renderer[] renderers;
    private InputManager inputManager;

    [Header("Spawn Logic")]
    [SerializeField] private float spawnCheckInterval = 20f;
    [SerializeField] private float maxSpawnDistance = 30f;
    [SerializeField] private float minSpawnDistance = 8f;

    [Header("Heights")]
    [SerializeField] private float groundY = 7f;
    [SerializeField] private float flyHeight = 3f;

    [Header("Chase Logic")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float flyDistance = 18f;
    [SerializeField] private float walkDistance = 10f;

    [Header("Lunge Attack")]
    [Tooltip("Speed when in scary/lunge range")]
    [SerializeField] private float lungeSpeed = 12f;
    [Tooltip("Distance at which the monster lunges (should be <= walkDistance)")]
    [SerializeField] private float lungeDistance = 5f;

    [Header("Escape Sequence")]
    [Tooltip("How long (seconds) the NPC 'flash' stands where the monster was.")]
    [SerializeField] private float npcFlashDuration = 2f;
    [SerializeField] private float inYourFaceDuration = 0.5f;

    // Animator parameters
    private readonly int flyHash   = Animator.StringToHash("isFlying");
    private readonly int walkHash  = Animator.StringToHash("isWalking");
    private readonly int scaryHash = Animator.StringToHash("isScaryWalk");

    private float spawnTimer;
    private float inYourFaceTimer;
    private bool isActive;

    // Escape sequence state
    private bool inEscapeSequence;
    private bool flashStarted;
    private NPC[] npcPool;                  // references to spawned NPCs from NPCManager
    private NPC activeFlashNpc;
    private bool flashNpcIsGood;
    private float flashTimer;
    private Vector3 flashOriginalPosition;
    private Quaternion flashOriginalRotation;

    // Lunge state for jumpscare SFX
    private bool wasLunging;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        player = FindAnyObjectByType<Player>();
        renderers = GetComponentsInChildren<Renderer>();

        inputManager = InputManager.Instance != null
            ? InputManager.Instance
            : FindAnyObjectByType<InputManager>();

        spawnTimer = spawnCheckInterval;

        var col = GetComponent<CapsuleCollider>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void Update()
    {
        if (player == null) return;

        // If we are *already* in the escape sequence, let that logic run regardless of UI.
        if (inEscapeSequence)
        {
            // Handle in-your-face delay before starting the flash and hiding the monster visuals
            if (inYourFaceTimer > 0f)
            {
                inYourFaceTimer -= Time.deltaTime;
                if (inYourFaceTimer <= 0f && !flashStarted)
                {
                    SetVisualsActive(false);
                    StartEscapeFlash();
                }
            }
            else if (!flashStarted)
            {
                // Safety: if timer is zero or negative for any reason, ensure flash still starts
                SetVisualsActive(false);
                StartEscapeFlash();
            }

            HandleFlashNpcLifetime();
            return;
        }

        // If player is in dialogue / options / shop, freeze the monster completely.
        if (IsPlayerBusy())
        {
            return;
        }

        if (!isActive)
        {
            // Count down to next possible spawn
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnCheckInterval;
                TrySpawnNearPlayer();
            }
            return;
        }

        ChaseAndAnimate();
    }

    private bool IsPlayerBusy()
    {
        if (inputManager == null) return false;

        // Block monster movement / spawning while player is in:
        // - Dialogue
        // - Option selection (dialogue options)
        // - Shop UI
        return inputManager.InDialogue
            || inputManager.InOptionSelect
            || inputManager.InShop;
    }

    private void TrySpawnNearPlayer()
    {
        float highFrac = player.m_currentHigh / Mathf.Max(1f, player.maxHigh);
        float lostFrac = 1f - highFrac;

        float chance = GetSpawnChance(lostFrac);
        if (chance <= 0f) return;
        if (Random.value > chance) return;

        float spawnRadius = Mathf.Lerp(maxSpawnDistance, minSpawnDistance, Mathf.Clamp01(lostFrac));

        Vector2 rand2D = Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(rand2D.x, 0f, rand2D.y);

        Vector3 spawnPos = player.transform.position + dir * spawnRadius;
        spawnPos.y = groundY + flyHeight;

        transform.position = spawnPos;
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));

        isActive = true;
        SetVisualsActive(true);

        // Play spawn SFX when the monster appears
        SoundManager.Instance?.PlaySFX("monster_spawn");

        Debug.Log($"Manananggal spawned at distance ~{spawnRadius:0.0}, lostFrac={lostFrac:0.00}, chance={chance:P0}");
    }

    private float GetSpawnChance(float lostFrac)
    {
        if (lostFrac < 0.25f) return 0f;
        if (lostFrac < 0.50f) return 0.10f;
        if (lostFrac < 0.75f) return 0.20f;
        if (lostFrac < 0.90f) return 0.50f;
        return 1.0f;
    }

    private void ChaseAndAnimate()
    {
        Vector3 playerPos = player.transform.position;
        Vector3 currentPos = transform.position;

        Vector3 toPlayerFlat = new Vector3(playerPos.x - currentPos.x, 0f, playerPos.z - currentPos.z);
        float flatDist = toPlayerFlat.magnitude;

        bool fly   = flatDist > flyDistance;
        bool walk  = flatDist <= flyDistance && flatDist > walkDistance;
        bool scary = flatDist <= walkDistance;
        bool lunging = flatDist <= lungeDistance;

        float targetY = fly ? groundY + flyHeight : groundY;
        Vector3 targetPos = new Vector3(playerPos.x, targetY, playerPos.z);

        float currentSpeed = lunging ? lungeSpeed : moveSpeed;
        transform.position = Vector3.MoveTowards(currentPos, targetPos, currentSpeed * Time.deltaTime);

        Vector3 lookTarget = new Vector3(playerPos.x, transform.position.y, playerPos.z);
        transform.LookAt(lookTarget);

        anim.SetBool(flyHash,   fly);
        anim.SetBool(walkHash,  walk);
        anim.SetBool(scaryHash, scary);

        // When we newly enter lunge range, play jumpscare once
        if (lunging && !wasLunging)
        {
            SoundManager.Instance?.PlaySFX("jumpscare");
        }

        wasLunging = lunging;
    }

    // ------------ NPC pool helper ------------

    private void RefreshNpcPool()
    {
        npcPool = null;

        if (NPCManager.Instance == null)
        {
            Debug.LogWarning("Manananggal: NPCManager.Instance is null, cannot build npcPool.");
            return;
        }

        var list = NPCManager.Instance.SpawnedNpcs;
        if (list == null || list.Count == 0)
        {
            Debug.LogWarning("Manananggal: NPCManager.SpawnedNpcs is empty.");
            return;
        }

        npcPool = new NPC[list.Count];
        for (int i = 0; i < list.Count; i++)
        {
            npcPool[i] = list[i];
        }
    }

    // ------------ Escape flash logic ------------

    private void HandleFlashNpcLifetime()
    {
        if (activeFlashNpc == null) return;
        flashTimer -= Time.deltaTime;
        if (flashTimer <= 0f)
        {
            // Flash finished → put NPC back and show monster again
            EndFlash(showMonster: true);
        }
    }

    private void StartEscapeFlash()
    {
        flashStarted = true;

        // Blue flash when escape sequence starts
        FlashManager.Instance?.FlashEscape();

        // Make sure we have the runtime NPC pool when we actually need it
        if (npcPool == null || npcPool.Length == 0)
        {
            RefreshNpcPool();
        }

        // Choose a random NPC from the 10 spawned by NPCManager
        if (npcPool != null && npcPool.Length > 0)
        {
            NPC npc = npcPool[Random.Range(0, npcPool.Length)];
            if (npc != null)
            {
                activeFlashNpc = npc;
                flashOriginalPosition = npc.transform.position;
                flashOriginalRotation = npc.transform.rotation;

                // Move this NPC to where the monster is
                npc.transform.SetPositionAndRotation(transform.position, transform.rotation);

                // Good vs bad from runtime type
                flashNpcIsGood = (activeFlashNpc.Type == NPC.NPCType.good);

                flashTimer = npcFlashDuration;
            }
        }
        else
        {
            Debug.LogWarning("Manananggal: npcPool still empty on touch. Ensure NPCManager has spawned NPCs.");
        }

        // Open the escape choice UI
        if (EscapeChoiceUI.Instance != null)
        {
            EscapeChoiceUI.Instance.Open(this, flashNpcIsGood);
        }

        Debug.Log("Manananggal escape flash started.");
    }

    private void SetVisualsActive(bool active)
    {
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = active;
        }
    }

    /// <summary>
    /// Ends the NPC flash: restore NPC transform and optionally re-show monster.
    /// Safe to call multiple times.
    /// </summary>
    private void EndFlash(bool showMonster)
    {
        if (activeFlashNpc != null)
        {
            activeFlashNpc.transform.SetPositionAndRotation(flashOriginalPosition, flashOriginalRotation);
            activeFlashNpc = null;
        }

        if (showMonster)
        {
            SetVisualsActive(true);
        }
    }

    /// <summary>
    /// Called when the player consumes a drug item. Immediately despawns the
    /// Manananggal and resets its spawn timer, without triggering escape logic.
    /// </summary>
    public void DespawnFromDrug()
    {
        // End any active NPC flash without re-showing the monster.
        EndFlash(showMonster: false);

        inEscapeSequence = false;
        isActive = false;

        // Hide visuals and move the monster far below the map so it can't be touched.
        SetVisualsActive(false);
        transform.position = new Vector3(0f, -1000f, 0f);

        // Reset spawn timer so it can spawn again later based on the player's state.
        spawnTimer = spawnCheckInterval;
    }

    public void animDebug()
    {
        if (player == null) return;

        float debugRadius = 15f;
        Vector3 dir = Random.insideUnitSphere;
        dir.y = 0f;
        dir.Normalize();

        Vector3 pos = player.transform.position + dir * debugRadius;
        pos.y = groundY + flyHeight;

        transform.position = pos;
        isActive = true;
        inEscapeSequence = false;

        SetVisualsActive(true);

        // Play spawn SFX for debug spawn as well
        SoundManager.Instance?.PlaySFX("monster_spawn");
        Debug.Log("Manananggal DEBUG spawn.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (inEscapeSequence) return;

        Player p = other.GetComponent<Player>();
        if (p == null) return;
        SoundManager.Instance?.PlaySFX("screech");

        // Smoothly rotate player to face the monster
        StartCoroutine(SmoothFacePlayerTowardsMonster(p));

        // Stop chasing and enter escape sequence
        isActive = false;
        inEscapeSequence = true;

        // Reset state and start the in-your-face timer; the monster will hide and flash will start after this delay in Update
        flashStarted = false;
        inYourFaceTimer = inYourFaceDuration;

        Debug.Log("Manananggal touched the player – escape sequence primed.");
    }

    /// <summary>
    /// Smoothly rotates the player around Y to face the monster over a short duration.
    /// Camera (as child of player) will follow this rotation.
    /// </summary>
    private IEnumerator SmoothFacePlayerTowardsMonster(Player p)
    {
        if (p == null) yield break;

        Transform playerTransform = p.transform;

        // Direction to monster, ignoring vertical difference
        Vector3 toMonster = transform.position - playerTransform.position;
        toMonster.y = 0f;
        if (toMonster.sqrMagnitude < 0.0001f)
            yield break;

        Quaternion startRot = playerTransform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(toMonster.normalized, Vector3.up);

        float duration = 0.3f;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            playerTransform.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            yield return null;
        }

        playerTransform.rotation = targetRot;
    }

    /// <summary>
    /// Called by EscapeChoiceUI after the player has made a decision.
    /// Monster DESPAWNS (hidden + moved away) but is not destroyed.
    /// </summary>
    public void OnEscapeChoiceResolved()
    {
        // Ensure NPC is restored even if the player chose before flash timer finished
        EndFlash(showMonster: false);

        inEscapeSequence = false;
        isActive = false;

        // Hide monster and move it far away so it can't be touched again
        SetVisualsActive(false);
        transform.position = new Vector3(0f, -1000f, 0f);

        // Reset spawn timer so it can spawn again later based on high %
        spawnTimer = spawnCheckInterval;
    }
}
