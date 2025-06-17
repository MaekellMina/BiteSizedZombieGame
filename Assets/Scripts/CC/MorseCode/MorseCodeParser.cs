using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace cc.IntrusiveThots
{
    public class MorseCodeParser : MonoBehaviour
    {
        [System.Serializable]
        public class MorseSet
        {
            public char character;
            public string code;
            public int[] pattern;

            public MorseSet(char character, string code, int[] pattern)
            {
                this.character = character;
                this.code = code;
                this.pattern = pattern;
            }
        }

        [SerializeField]
        [TextArea]
        string message;

        [SerializeField]
        [TextArea]
        string morseTranslation;
        [Space]
        [SerializeField]
        List<int> currentPatternKey = new List<int>();
        [Header("Misc :>")]
        [SerializeField]
        [TextArea]
        string morseToTranslate;
        [SerializeField]
        [TextArea]
        string morseToTranslateResult;

        [Space]
        [SerializeField] TextAsset MorseChart;

        [Space]
        public List<MorseSet> Key = new List<MorseSet>();

        private void OnValidate()
        {
            if (message != null && Key.Count > 0)
            {
                Convert();
            }
        }

        public void Convert()
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            string upperMessage = message.ToUpper();
            StringBuilder morseBuilder = new StringBuilder();
            List<int> patternHolder = new();

            for (int i = 0; i < upperMessage.Length; i++)
            {
                char currentChar = upperMessage[i];

                var set = Key.FirstOrDefault(s => s.character == currentChar);
                if (set == null)
                    continue;

                morseBuilder.Append(set.code).Append(' ');
                patternHolder.AddRange(set.pattern);

                // Check if we need to add an inter-letter gap
                bool isLastChar = (i + 1 >= upperMessage.Length);
                char nextChar = isLastChar ? '\0' : upperMessage[i + 1];

                if (currentChar != ' ' && nextChar != ' ' && !isLastChar)
                {
                    patternHolder.Add(-1); // Inter-letter gap
                }
            }

            morseTranslation = morseBuilder.ToString().TrimEnd();
            currentPatternKey = patternHolder;
        }

        [Button("ConvertFromMorse", EButtonEnableMode.Editor)]
        public void ConvertFromMorse()
        {
            string cleaned = morseToTranslate.Replace(" / ", " / ").Replace("  ", " ").Trim();
            string[] parts = cleaned.Split(' ');
            for (int i = 0; i < parts.Length; i++)
            {
                if (parts[i] == "/")
                    parts[i] = "/";
            }

            var currentString = "";

            // List<int> PatternHolder = new();
            for (int i = 0; i < parts.Length; i++)
            {
                var set = Key.FirstOrDefault(set => set.code == parts[i]);
                if (set != null)
                {
                    currentString += set.character;
                }
                else
                {
                    currentString += " ";

                }

            }
            morseToTranslateResult = currentString;
        }

        [Button("ParseChart", EButtonEnableMode.Editor)]
        public void ParseChart()
        {
            if (MorseChart == null)
            {
                Debug.LogError("No Morse code text file assigned.");
                return;
            }
            Key = new List<MorseSet>();
            var space = new MorseSet(' ', "/", new int[1] { -2 });
            Key.Add(space);

            string[] lines = MorseChart.text.Split('\n');
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] parts = line.Trim().Split('|');
                if (parts.Length == 2)
                {
                    char character = parts[0].TrimEnd()[0];
                    string code = parts[1];
                    var pattern = new List<int>();
                    for (int i = 0; i < code.Length; i++)
                    {
                        if (code[i] == '.')
                        {
                            pattern.Add(0);
                        }
                        else if (code[i] == '-')
                        {
                            pattern.Add(1);
                        }

                    }
                    var morseSet = new MorseSet(character, code, pattern.ToArray());
                    Key.Add(morseSet);
                }
            }

        }


        public string GetMorse() => morseTranslation;
        public List<int> GetPattern => currentPatternKey;
   
    
    }

}
