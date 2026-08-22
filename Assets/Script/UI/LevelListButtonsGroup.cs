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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio("bgm");
        }

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

        int highestUnlocked = PlayerDataManager.GetHighestUnlockedLevel();

        // Gán sự kiện onClick và trạng thái mở khóa cho các nút level được tạo ra
        Button[] allButtons = FindObjectsOfType<Button>();
        foreach (Button btn in allButtons)
        {
            if (btn.name.StartsWith("Button_"))
            {
                if (int.TryParse(btn.name.Substring(7), out int index))
                {
                    bool isUnlocked = index <= highestUnlocked;

                    btn.interactable = isUnlocked;

                    Image btnImg = btn.GetComponent<Image>();
                    if (btnImg != null && !isUnlocked)
                    {
                        btnImg.color = new Color(0.6f, 0.6f, 0.6f, 0.6f);
                    }

                    if (isUnlocked)
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
}
