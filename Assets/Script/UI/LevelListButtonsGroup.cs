using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelListButtonsGroup : MonoBehaviour
{
    [SerializeField] Button homeBtn;
    [SerializeField] Button audioBtn;

    [SerializeField] GameObject audioBanner;
    void Start()
    {
        homeBtn.onClick.AddListener(
            () => {
                SceneManager.LoadScene("HomeScreen");
        });

        audioBtn.onClick.AddListener(
            () => {
                audioBanner.SetActive(true);
        });
    }
}
