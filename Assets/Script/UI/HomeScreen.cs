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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio("bgm");
        }

        in4Banner.SetActive(false);

        playBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayAudio("next");
            Time.timeScale = 1f;
            GameManager1.selectedLevelIndex = PlayerDataManager.GetHighestUnlockedLevel();
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
