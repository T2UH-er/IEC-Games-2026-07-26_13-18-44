using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Button levelBtn;
    [SerializeField] Button infoBtn;

    [SerializeField] GameObject in4Banner;
    void Start()
    {
        in4Banner.SetActive(false);

        playBtn.onClick.AddListener(() =>
        {

        });

        levelBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LevelList");
        });

        infoBtn.onClick.AddListener(() =>
        {
            in4Banner.SetActive(true);
        });
    }
}
