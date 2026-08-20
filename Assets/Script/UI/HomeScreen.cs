using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Button levelBtn;
    [SerializeField] Button audioBtn;
    [SerializeField] Button infoBtn;

    [SerializeField] GameObject in4Banner;
    [SerializeField] GameObject audioBanner;
    void Start()
    {
        in4Banner.SetActive(false);

        playBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayAudio("next");
            Time.timeScale = 1f;

            int completedLevel = PlayerPrefs.GetInt("CompletedLevel", 0);
            int nextLevel = completedLevel + 1;
            GameManager1.selectedLevelIndex = nextLevel;

            SceneManager.LoadScene("test");
        });

        levelBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayAudio("next");
            SceneManager.LoadScene("LevelList");
        });

        infoBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayAudio("next");
            in4Banner.SetActive(true);
        });

        audioBtn.onClick.AddListener(() => {
            AudioManager.Instance.PlayAudio("next");
            audioBanner.SetActive(true);

            Button[] buttons = FindObjectsOfType<Button>();
            foreach (Button button in buttons)
            {
                if (button != audioBanner.GetComponentInChildren<Button>()) button.interactable = false;
            }

        });
    }
}
