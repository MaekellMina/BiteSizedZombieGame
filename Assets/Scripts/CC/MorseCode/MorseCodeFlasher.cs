using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace cc.IntrusiveThots
{
    public class MorseCodeFlasher : MonoBehaviour
    {
        public Light2D lightSource;
        public AudioSource Beeper;
        public AudioClip lightClick;

        public IEnumerator PlayMorseVisual(string morse, float unitDuration)
        {
            if (lightClick) Beeper.clip = lightClick;

            foreach (char symbol in morse)
            {
                switch (symbol)
                {
                    case '.':
                        yield return Blink(1);
                        break;
                    case '-':
                        yield return Blink(3);
                        break;
                    case ' ':
                        yield return new WaitForSeconds(unitDuration * 2);
                        break;
                    case '/':
                        yield return new WaitForSeconds(unitDuration * 6);
                        break;
                }
            }
        }

        private IEnumerator Blink(int units)
        {
            if (lightClick) Beeper.Play();
            lightSource.enabled = true;
            yield return new WaitForSeconds(units * 0.2f);
            lightSource.enabled = false;
            yield return new WaitForSeconds(0.2f);
        }
    }

}
