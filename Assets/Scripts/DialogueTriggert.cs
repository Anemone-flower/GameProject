using System.Collections;
using UnityEngine;
using TMPro;
public class DialogueTrigger : MonoBehaviour
{
    public GameObject dialogueUI;
    public DialogueTyper typer;
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (!dialogueUI.activeSelf)
            {
                dialogueUI.SetActive(true);
                typer.StartTyping("그래, 날 막으러 온 것인가..?");
                Time.timeScale = 0f;
            }
            else if (typer.IsTyping())
            {
                typer.SkipTyping(); // 타이핑 중이면 즉시 출력
            }
        }

        if (dialogueUI.activeSelf && Input.GetKeyDown(KeyCode.Escape))
        {
            dialogueUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            dialogueUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}
