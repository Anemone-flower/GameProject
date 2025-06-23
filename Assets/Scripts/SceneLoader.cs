using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 씬 이름으로 이동

    public GameObject dialoguePanel;

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f; // 만약 대화 중 시간 멈췄다면 재개
    }
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // 씬 번호로 이동
    public void LoadSceneByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    // 게임 종료
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
