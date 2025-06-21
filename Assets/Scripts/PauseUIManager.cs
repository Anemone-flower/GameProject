using UnityEngine;
using DG.Tweening;

public class PauseUIController : MonoBehaviour
{
    public GameObject backgroundOverlay;
    public RectTransform pausePanel;

    private Vector2 visiblePos;
    private Vector2 hiddenPos;

    private bool isOpen = false;

    void Awake()
    {
        visiblePos = Vector2.zero;
        hiddenPos = new Vector2(0, -Screen.height);
    }

    void Start()
    {
        pausePanel.anchoredPosition = hiddenPos;
        pausePanel.gameObject.SetActive(false);
        backgroundOverlay.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && !isOpen)
        {
            OpenPanel();
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            ClosePanel();
        }
    }

    void OpenPanel()
    {
        isOpen = true;
        Time.timeScale = 0f;

        backgroundOverlay.SetActive(true);
        pausePanel.gameObject.SetActive(true);

        // 애니메이션 실행 (시간 정지에서도 작동)
        pausePanel.anchoredPosition = hiddenPos;
        pausePanel.DOAnchorPos(visiblePos, 0.5f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    void ClosePanel()
    {
        isOpen = false;

        pausePanel.DOAnchorPos(hiddenPos, 0.5f)
            .SetEase(Ease.InCubic)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                pausePanel.gameObject.SetActive(false);
                backgroundOverlay.SetActive(false);
                Time.timeScale = 1f;
            });
    }
}
