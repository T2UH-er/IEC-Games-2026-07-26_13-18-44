using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] Toggle audioToggle;
    [SerializeField] Button resumeBtn;
    [SerializeField] Button homeBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioToggle.onValueChanged.AddListener((input) =>
        {
            if (audioToggle.isOn)
            {
                // bật âm thanh
            }
            else { 
                // tắt âm thanh
            }
        });

        resumeBtn.onClick.AddListener(() =>
        {
            gameObject.SetActive(false);
        });

        homeBtn.onClick.AddListener(() => {
            SceneManager.LoadScene("HomeScreen");
        });
    }
}
