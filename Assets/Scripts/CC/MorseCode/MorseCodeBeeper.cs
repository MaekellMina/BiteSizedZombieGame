using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace cc.IntrusiveThots
{
    public class MorseCodeBeeper : MonoBehaviour
    {      

        [Header("Setup")]
        [SerializeField] AudioClip Dit;
        [SerializeField] AudioClip Dah;        
        [Space]
        [SerializeField] AudioSource Beeper;        
        [SerializeField]
        [ReadOnly]
        AudioClip[] beepAudioList;

        Coroutine beepRoutine;
       
        private void OnEnable()
        {
            
        }

        private void OnValidate()
        {
            if (Dit && Dah && beepAudioList.Length<2)
            {
                beepAudioList = new[] { Dit, Dah };
            }           
        }
      
        public void Refresh()
        {
            if (Dit && Dah)
            {                
                beepAudioList = new[] { Dit, Dah };
            }
          
        }
        
        [Button("TestBeep", EButtonEnableMode.Playmode)]

        public void Beep()
        {
            var morseParser = GetComponent<MorseCodeParser>();
            if (beepRoutine != null)
                StopCoroutine(beepRoutine);
            beepRoutine = StartCoroutine(PlayBeep(morseParser.GetPattern));
        }

        public void Beep(List<int> currentPatternKey)
        {
            if (beepRoutine != null)
                StopCoroutine(beepRoutine);
            beepRoutine = StartCoroutine(PlayBeep(currentPatternKey));
        }

        IEnumerator PlayBeep(List<int> currentPatternKey)
        {
            if (currentPatternKey.Count < 1)
            {
                Debug.Log("No Pattern");
                yield break;
            }
            if (!Beeper)
            {
                Debug.Log("No audiosource");
                yield break;
            }

            float unitDuration = 0.1f; // 1 unit = 0.1 seconds = 100ms
            float letterGap = 3 * unitDuration;
            float wordGap = 7 * unitDuration;

            WaitForSeconds letterWait = new WaitForSeconds(letterGap);
            WaitForSeconds wordWait = new WaitForSeconds(wordGap);
            WaitUntil beepNotPlayWait = new WaitUntil(() => !Beeper.isPlaying);

            for (int i = 0; i < currentPatternKey.Count; i++)
            {
                var beepInt = currentPatternKey[i];
                if (beepInt < 0)
                {
                    Debug.Log("pause");
                    yield return (beepInt < -1) ? wordWait : letterWait;
                }
                else
                {
                    Debug.Log("beep");
                    Beeper.clip = beepAudioList[beepInt];
                    Beeper.Play();
                    yield return beepNotPlayWait;
                }

            }

        }
        [Button("STAHP", EButtonEnableMode.Playmode)]
        public void Stop()
        {
            if (beepRoutine != null)
                StopCoroutine(beepRoutine);
           
        }
    }
}
