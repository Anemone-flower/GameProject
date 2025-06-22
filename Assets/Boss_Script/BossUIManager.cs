using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossUIManager : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    public Slider hpSlider;
    public Image hpFillImage;
    public RectTransform shieldRect;
    public TMP_Text bossNameText;

    [Header("실드 바 설정")]
    public float maxShieldWidth = 200f;
    public float shieldLerpSpeed = 10f;

    private int maxHP;
    private float targetShieldWidth = 0f;

    void Update()
    {
        if (shieldRect != null)
        {
            float currentWidth = shieldRect.sizeDelta.x;
            float newWidth = Mathf.Lerp(currentWidth, targetShieldWidth, Time.deltaTime * shieldLerpSpeed);
            shieldRect.sizeDelta = new Vector2(newWidth, shieldRect.sizeDelta.y);
        }
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    public void SetupUI(int maxHealth, string bossName = "")
    {
        maxHP = maxHealth;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = maxHealth;
        }

        if (hpFillImage != null)
        {
            hpFillImage.enabled = true;
        }

        if (shieldRect != null)
        {
            shieldRect.sizeDelta = new Vector2(0f, shieldRect.sizeDelta.y);
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        targetShieldWidth = 0f;
    }

    /// <summary>
    /// 체력 및 실드 UI 갱신
    /// </summary>
    public void UpdateHP(int currentHealth, float shieldAmount = 0f)
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (hpFillImage != null)
        {
            hpFillImage.enabled = currentHealth > 0;
        }

        if (shieldRect != null)
        {
            float shieldRatio = Mathf.Clamp01(shieldAmount / maxHP);
            targetShieldWidth = maxShieldWidth * shieldRatio;
        }
    }

    /// <summary>
    /// UI 비활성화
    /// </summary>
    public void HideUI()
    {
        if (hpSlider != null)
            hpSlider.gameObject.SetActive(false);

        if (shieldRect != null)
            shieldRect.gameObject.SetActive(false);

        if (bossNameText != null)
            bossNameText.gameObject.SetActive(false);
    }
}
