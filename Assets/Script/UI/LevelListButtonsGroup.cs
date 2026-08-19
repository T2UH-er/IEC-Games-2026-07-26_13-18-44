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
                SceneManager.LoadScene("HomeScence");
        });

        audioBtn.onClick.AddListener(
            () => {
                AudioManager.Instance.PlayAudio("next");
                audioBanner.SetActive(true);

                Button[] buttons = FindObjectsOfType<Button>();
                foreach (Button button in buttons)
                {
                    if (button != audioBanner.GetComponentInChildren<Button>()) button.interactable = false;
                }
            });

        // Gán sự kiện onClick cho các nút level được tạo ra
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.name.StartsWith("Button_"))
            {
                if (int.TryParse(btn.name.Substring(7), out int index))
                {
                    btn.onClick.AddListener(() =>
                    {
                        AudioManager.Instance.PlayAudio("next");
                        Time.timeScale = 1f;
                        Debug.Log($"[LevelListButtonsGroup] Chọn Level {index}");
                        GameManager1.selectedLevelIndex = index;
                        SceneManager.LoadScene("test");
                    });
                }
            }
        }
    }
}
