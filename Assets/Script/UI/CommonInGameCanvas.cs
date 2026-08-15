using UnityEngine;
using UnityEngine.UI;

public class CommonInGameCanvas : MonoBehaviour
{
    [SerializeField] private Button pauseBtn;
    [SerializeField] private GameObject pauseMenu;

    private void Start()
    {
        pauseBtn.onClick.AddListener(() =>
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        });
    }
}
