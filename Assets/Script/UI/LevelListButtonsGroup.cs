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
                AudioManager.Instance.PlayAudio("back");
                SceneManager.LoadScene("HomeScene");
        });

        audioBtn.onClick.AddListener(
            () => {
                AudioManager.Instance.PlayAudio("next");
                audioBanner.SetActive(true);

                Button[] buttons = FindObjectsOfType<Button>();

                foreach (Button button in buttons) { 
                    if ( button != audioBanner.GetComponentInChildren<Button>()) button.interactable = false;
                }
        });
    }
}
