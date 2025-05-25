using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


namespace cc.Interaction
{
    [System.Serializable]
    public struct RadioContent
    {
        public string station;
        [TextArea]
        public string text;
       
    }
    public class RadioInteraction : MonoBehaviour, IInteractable
    {
        [Header("Setup")]
        public string currentStation;
        [Space]
        public List<RadioContent> Content = new List<RadioContent>();
        [Header("Components")]
        [SerializeField]
        TypewriterText Typewriter;

        public GameObject GetTargetObject()
        {
            return gameObject;
        }

        public void OnInteract()
        {
            var rc = Content.FirstOrDefault(x => x.station.Equals(currentStation));
            if (rc.text != null)
                Typewriter.SetNewText(rc.text);
            else
                Typewriter.SetNewText(".....");
        }

     
    }

}
