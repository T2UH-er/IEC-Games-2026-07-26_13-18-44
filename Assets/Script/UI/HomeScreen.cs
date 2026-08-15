using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class HomeScreen : MonoBehaviour
{
    [SerializeField] Button playBtn;
    [SerializeField] Button levelBtn;
    [SerializeField] Button quitBtn;
    void Start()
    {
        playBtn.onClick.AddListener(() =>
        {

        });

        levelBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("LevelList");
        });

        quitBtn.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }
}
