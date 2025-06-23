using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossSceneIntro : MonoBehaviour
{
    [Header("UI 참조")]
    public Image blackBackground;               // 검은 배경
    public TMP_Text monologueText;              // 대사 출력 텍스트
    public GameObject bossUIGroup;              // 체력바 + 이름 UI 그룹
    public TMP_Text bossNameText;               // 보스 이름

    [Header("연출 텍스트")]
    [TextArea(2, 5)] public string[] monologueLines;
    public string bossName = "아카츠키";

    [Header("타이핑 설정")]
    public float typingSpeed = 0.05f;
    public float monologueDelay = 1.2f;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // 게임 일시정지
        Time.timeScale = 0f;

        monologueText.text = "";
        bossNameText.text = "";
        blackBackground.color = new Color(0, 0, 0, 1);
        bossUIGroup.SetActive(false);

        // 보스 대사 출력
        foreach (string line in monologueLines)
        {
            yield return StartCoroutine(TypeTextRealtime(monologueText, line));
            yield return new WaitForSecondsRealtime(monologueDelay);
            monologueText.text = "";
        }

        // 페이드 인
        yield return StartCoroutine(FadeOutBlackRealtime());

        // 보스 UI 등장
        bossUIGroup.SetActive(true);

        // 이름 출력
        yield return StartCoroutine(TypeTextRealtime(bossNameText, bossName));

        // 게임 재개
        Time.timeScale = 1f;
    }

    private IEnumerator TypeTextRealtime(TMP_Text textUI, string text)
    {
        textUI.text = "";
        foreach (char c in text)
        {
            textUI.text += c;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }
    }

    private IEnumerator FadeOutBlackRealtime()
    {
        float duration = 1.5f;
        float timer = 0f;
        Color color = blackBackground.color;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / duration);
            blackBackground.color = color;
            yield return null;
        }

        blackBackground.gameObject.SetActive(false);
    }
}
