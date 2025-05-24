using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorTeleport : MonoBehaviour,IInteractable
{
    public Sprite closedSprite;
    public Sprite openSprite;
    public bool isOpen = false;
    public Transform teleportTarget;          // Where the player gets teleported
    public bool autoTeleport = false;         // If true, teleport on enter without key press

    private SpriteRenderer spriteRenderer;
    private bool playerInRange = false;
    [SerializeField]
    private Transform player;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateDoorVisual();
    }

    void Update()
    {
       
    }

    void Interact()
    {
        InteractionManager.instance.Unregister(this);
        Debug.Log($"TP {gameObject.name} {teleportTarget.transform.gameObject.name}");
        ToggleDoor();
        TeleportPlayer();
    }

    void ToggleDoor()
    {
        isOpen = !isOpen;
        UpdateDoorVisual();

        // Optional: disable collider when door is open
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = !isOpen;
    }

    void TeleportPlayer()
    {
        player = InteractionManager.instance.playerTransform;
        if (player != null && teleportTarget != null)
        {
            player.transform.position = teleportTarget.position;
        }
    }

    void UpdateDoorVisual()
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }


   
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;           
            InteractionManager.instance.Register(this);
            if (autoTeleport)
                Interact();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
           
            InteractionManager.instance.Unregister(this);
        }
    }

    public void OnInteract()
    {
        if (playerInRange && !autoTeleport)
        {
            Interact();
        }
    }

    public GameObject GetTargetObject()
    {
        return this.gameObject;
    }
}
