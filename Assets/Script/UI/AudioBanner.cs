using UnityEngine;
using UnityEngine.UI;
public class AudioBanner : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] Button closeBtn;

    void Start()
    {
        if (volumeSlider != null)
        {
            // 1. Gán giá trị masterVolume hiện tại của AudioManager cho Scrollbar khi khởi động
            if (AudioManager.Instance != null)
            {
                volumeSlider.value = AudioManager.Instance.masterVolume;
            }

            // 2. Đăng ký sự kiện khi Scrollbar thay đổi giá trị (OnValueChanged)
            volumeSlider.onValueChanged.AddListener(OnVolumeScrollbarChanged);
        }
        else
        {
            Debug.LogWarning("Chưa gán Scrollbar vào AudioSettingsUI!");
        }


        volumeSlider.onValueChanged.AddListener(OnVolumeScrollbarChanged);

        closeBtn.onClick.AddListener(() =>
        {
            AudioManager.Instance.PlayAudio("back");
            gameObject.SetActive(false);

            Button[] buttons = FindObjectsOfType<Button>();
            foreach (Button b in buttons) {
                b.interactable = true;
            }
        });
    }

    // Hàm Callback khi Scrollbar thay đổi giá trị
    private void OnVolumeScrollbarChanged(float value)
    {
        if (AudioManager.Instance != null)
        {
            // Cập nhật masterVolume và tự động lưu vào file JSON
            AudioManager.Instance.SetMasterVolume(value);
        }
    }
}
