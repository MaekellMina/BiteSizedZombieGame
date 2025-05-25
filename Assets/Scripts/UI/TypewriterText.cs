using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]

public class TypewriterText : MonoBehaviour
{
    [TextArea]
    public string fullText;

    public float typeDelay = 0.05f;
    public float punctuationDelay = 0.25f;
    public bool playeOnEnable = true;
    [SerializeField]
    private TextMeshProUGUI textComponent;
    private Coroutine typingCoroutine;

    void Awake()
    {
        textComponent.maxVisibleCharacters = 0;
    }

    void Start()
    {
        if (playeOnEnable)
            StartTyping();
    }

    public void StartTyping()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText());
    }

    public IEnumerator TypeText()
    {
        textComponent.maxVisibleCharacters = 0;
        textComponent.text = fullText;
        textComponent.ForceMeshUpdate(); // ensures character count is updated
        int totalChars = textComponent.textInfo.characterCount;
        int visibleCount = 0;

        while (visibleCount < totalChars)
        {
            visibleCount++;
            textComponent.maxVisibleCharacters = visibleCount;

            char currentChar = fullText[Mathf.Clamp(visibleCount - 1, 0, fullText.Length - 1)];

            // Check for punctuation
            if (currentChar == '.' || currentChar == ',' || currentChar == '!' || currentChar == '?')
            {
                // Check for ellipsis "..."
                if (currentChar == '.' && visibleCount >= 3 &&
                    fullText[visibleCount - 2] == '.' && fullText[visibleCount - 3] == '.')
                {
                    yield return new WaitForSeconds(punctuationDelay + 0.1f);
                }
                else
                {
                    yield return new WaitForSeconds(punctuationDelay);
                }
            }
            else
            {
                yield return new WaitForSeconds(typeDelay);
            }
        }
    }

    public void SetNewText(string newText, bool autoStart = true)
    {
        fullText = newText;
        textComponent.maxVisibleCharacters = 0;
        if (autoStart)
            StartTyping();
    }

    public void Skip()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);
        textComponent.maxVisibleCharacters = textComponent.textInfo.characterCount;
    }

    private void OnValidate()
    {
        if(textComponent==null)
         textComponent = GetComponent<TextMeshProUGUI>();
        textComponent.text = fullText;
        
    }

    public void UpdateText()
    {       
        textComponent.text = fullText;
        textComponent.ForceMeshUpdate();

    }
}
