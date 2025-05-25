using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using cc.Interaction.Interface;


namespace cc.Interaction
{
    public class ValveSwitch : MonoBehaviour, IInteractable
    {
        [Header("Valve State")]
        [SerializeField] private bool isOn = false;

        [Header("Events")]
        public UnityEvent onValveOn;
        public UnityEvent onValveOff;
        public UnityEvent e_OnInteractFeedback;
        public float feedbackDelay = 0.2f;

        public
        bool isInteracting = false;
        Coroutine InteractionFeedbackRoutine;
        public void ToggleSwitch()
        {
            SetValve(!isOn);
        }

        // Call this if you want to explicitly set the valve
        public void SetValve(bool turnOn)
        {
            if (isOn == turnOn) return;

            isOn = turnOn;
            if (isOn)
                onValveOn?.Invoke();
            else
                onValveOff?.Invoke();
        }

        public void OnInteract()
        {
            if (isInteracting) return;

            Debug.Log("INTERACT");
            isInteracting = true;
            if (InteractionFeedbackRoutine != null)
                StopCoroutine(InteractionFeedbackRoutine);

            InteractionFeedbackRoutine = StartCoroutine(FeedBackBefore());
        }

        IEnumerator FeedBackBefore()
        {
            e_OnInteractFeedback?.Invoke();
            yield return new WaitForSeconds(feedbackDelay);
            ToggleSwitch();
            isInteracting = false;
        }
        public GameObject GetTargetObject()
        {
            return this.gameObject;
        }

        public bool IsOn => isOn;

        public void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                InteractionManager.instance?.Register(this);
            }
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                isInteracting = false;
                InteractionManager.instance?.Unregister(this);
            }
        }
    }

}

