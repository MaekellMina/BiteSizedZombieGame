using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace cc.Interaction.SO
{

    [RequireComponent(typeof(BoxCollider2D))]
    public class InteractableListenerObject : MonoBehaviour
    {
        public List<InteractionSignalSO> interactionSignals = new List<InteractionSignalSO>();

        [Space]
        [SerializeField]
        private bool UseAttachedInteractable = true;

        [Space]
        //[HideIf("UseAttachedInteractable")]
        public UnityEvent<GameObject> OnInteract = new UnityEvent<GameObject>();

        [Header("DEBUG")]
        [SerializeField]
        [ReadOnly]
        MonoBehaviour AttachedInteractable;

        private void Awake()
        {
            if (UseAttachedInteractable)
            {            
                UseAttachedInteractable = (AttachedInteractable != null);
            }
        }

        private void OnEnable()
        {           

            if (interactionSignals.Count > 0)
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    interactionSignals[i].RegisterListener(HandleInteraction);

                }

        }

        private void OnDisable()
        {          

            if (interactionSignals.Count > 0)
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    interactionSignals[i].UnregisterListener(HandleInteraction);

                }
        }

        private void HandleInteraction(GameObject Interactable)
        {
            if (Interactable != this.gameObject) return;
                OnInteract?.Invoke(this.gameObject);

        } 

     
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    if (interactionSignals[i].IsMyPlayer(other.gameObject))
                        interactionSignals[i].SetCurrentInteractable(this.gameObject);
                }
            }
           
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Player")
            {
                for (int i = 0; i < interactionSignals.Count; i++)
                {
                    if (interactionSignals[i].IsMyPlayer(other.gameObject))
                        interactionSignals[i].ClearCurrentInteractable(this.gameObject);

                }
            }

        }

        void TriggerInteractable(GameObject g)
        {
            var att = AttachedInteractable as IInteractable;
            att?.OnInteract();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (UseAttachedInteractable)
            {
                var components = GetComponents<MonoBehaviour>();
                foreach (var comp in components)
                {
                    if (comp is IInteractable interactable)
                    {
                        AttachedInteractable = comp;
                        break;
                    }
                }

                if (AttachedInteractable == null) return;

                if (!EventContainsMethod(OnInteract, this, nameof(TriggerInteractable)))
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(OnInteract, TriggerInteractable);
                    EditorUtility.SetDirty(this);
                }
            }


        }

        private bool EventContainsMethod(UnityEvent<GameObject> evt, Object target, string methodName)
        {
            int count = evt.GetPersistentEventCount();
            for (int i = 0; i < count; i++)
            {
                if (evt.GetPersistentTarget(i) == target &&
                    evt.GetPersistentMethodName(i) == methodName)
                {
                    return true;
                }
            }
            return false;
        }
#endif

       
    }

}
