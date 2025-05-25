using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using cc.Interaction.Interface;

namespace cc.Interaction
{

    public class DoorTeleport : MonoBehaviour, IInteractable
    {
        public Sprite closedSprite;
        public Sprite openSprite;
        public bool isOpen = false;
        public Transform teleportTarget;
        public bool autoTeleport = false;

        private SpriteRenderer spriteRenderer;
        private bool playerInRange = false;
        [SerializeField]
        private Transform player;

        public bool isSinglePlayer = false;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            UpdateDoorVisual();
        }


        void Interact()
        {
            InteractionManager.instance?.Unregister(this);
            Debug.Log($"TP {gameObject.name} {teleportTarget.transform.gameObject.name}");
            ToggleDoor();
            TeleportPlayer();
        }

        void ToggleDoor()
        {
            isOpen = !isOpen;
            UpdateDoorVisual();

        }

        void TeleportPlayer()
        {
            if (isSinglePlayer)
                player = InteractionManager.instance?.playerTransform;

            if (player != null && teleportTarget != null)
            {
                var target = player;
                target.transform.position = teleportTarget.position;
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
                player = other.transform;
                playerInRange = true;


                if (isSinglePlayer)
                    InteractionManager.instance?.Register(this);

                if (autoTeleport)
                    Interact();
            }
        }

        void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                player = null;

                if (isSinglePlayer)
                    InteractionManager.instance?.Unregister(this);
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
}