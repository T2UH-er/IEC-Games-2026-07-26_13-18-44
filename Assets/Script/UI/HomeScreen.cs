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

            playBtn.interactable = false;
            levelBtn.interactable = false;
            audioBtn.interactable = false;
            infoBtn.interactable = false;
        });
    }
}
