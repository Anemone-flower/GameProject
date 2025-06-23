using System.Collections;
using UnityEngine;
using TMPro;

public class DialogueTyper : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public float typingSpeed = 0.15f;

    private Coroutine typingCoroutine;
    private string currentText = "";
    private bool isTyping = false;

    public void StartTyping(string fullText)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        currentText = fullText;
        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    IEnumerator TypeText(string textToType)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in textToType.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
    }

    // 선택 기능: 타이핑 중일 때 F 키 다시 누르면 전체 텍스트 즉시 출력
    public void SkipTyping()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            dialogueText.text = currentText;
            isTyping = false;
        }
    }

    public bool IsTyping()
    {
        return isTyping;
    }
}
