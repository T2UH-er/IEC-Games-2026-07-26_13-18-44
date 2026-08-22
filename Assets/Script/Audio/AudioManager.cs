using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Database")]
    [SerializeField] private AudioDatabase audioDatabase;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Global Volume Modifier")]
    [Range(0f, 1f)] public float masterVolume = 1f; // Biến duy nhất dùng để điều chỉnh âm lượng toàn hệ thống

    private string configPath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (bgmSource == null) bgmSource = gameObject.AddComponent<AudioSource>();
            if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

            bgmSource.loop = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        configPath = Path.Combine(Application.streamingAssetsPath, "AudioConfig.json");
        LoadAudioConfig();
    }

    private const string KEY_MASTER_VOLUME = "MasterVolume";

    // Đọc hệ số âm lượng từ PlayerPrefs hoặc file .json ngoài
    private void LoadAudioConfig()
    {
        if (PlayerPrefs.HasKey(KEY_MASTER_VOLUME))
        {
            masterVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(KEY_MASTER_VOLUME, 1f));
        }
        else if (File.Exists(configPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                AudioConfigData config = JsonConvert.DeserializeObject<AudioConfigData>(jsonContent);

                if (config != null)
                {
                    masterVolume = Mathf.Clamp01(config.volume);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi đọc file AudioConfig.json: {e.Message}");
            }
        }

        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume;
        }
    }

    #region Play Methods

    // Overload 1: Không có tham số đi kèm -> Phát theo âm lượng mặc định trong ScriptableObject
    public void PlayAudio(string audioName)
    {
        AudioData data = audioDatabase != null ? audioDatabase.GetAudio(audioName) : null;
        if (data == null)
        {
            Debug.LogWarning($"Không tìm thấy audio: {audioName}");
            return;
        }

        PlayAudioInternal(data, data.defaultVolume);
    }

    // Overload 2: Có tham số đi kèm -> Phát theo âm lượng của tham số truyền vào
    public void PlayAudio(string audioName, float customVolume)
    {
        AudioData data = audioDatabase != null ? audioDatabase.GetAudio(audioName) : null;
        if (data == null)
        {
            Debug.LogWarning($"Không tìm thấy audio: {audioName}");
            return;
        }

        PlayAudioInternal(data, customVolume);
    }

    // Tính toán âm lượng thực tế = [Âm lượng chỉ định/mặc định] x masterVolume
    private void PlayAudioInternal(AudioData data, float inputVolume)
    {
        if (data == null || data.clip == null)
        {
            Debug.LogWarning($"[AudioManager] AudioClip cho '{data?.audioName}' bị null hoặc chưa được gán!");
            return;
        }

        float finalVolume = inputVolume * masterVolume;

        if (data.audioType == AudioType.BGM)
        {
            if (bgmSource.clip == data.clip && bgmSource.isPlaying)
            {
                bgmSource.volume = finalVolume;
                return;
            }

            bgmSource.clip = data.clip;
            bgmSource.volume = finalVolume;
            bgmSource.Play();
        }
        else if (data.audioType == AudioType.SFX)
        {
            sfxSource.PlayOneShot(data.clip, finalVolume);
        }
    }

    #endregion

    #region Stop Methods

    // Dừng phát âm thanh theo tên
    public void StopAudio(string audioName)
    {
        AudioData data = audioDatabase != null ? audioDatabase.GetAudio(audioName) : null;
        if (data == null) return;

        if (data.audioType == AudioType.BGM && bgmSource.clip == data.clip)
        {
            bgmSource.Stop();
        }
        else if (data.audioType == AudioType.SFX)
        {
            sfxSource.Stop();
        }
    }

    #endregion

    #region Master Volume

    // Hàm cập nhật và lưu giá trị masterVolume
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value); // Đảm bảo giá trị nằm trong khoảng [0, 1]
        if (bgmSource != null)
        {
            bgmSource.volume = masterVolume;
        }
        SaveAudioConfig();
    }

    // Hàm ghi dữ liệu xuống PlayerPrefs và file .json
    public void SaveAudioConfig()
    {
        // Luôn lưu vào PlayerPrefs (Hỗ trợ 100% WebGL, Android, iOS, PC)
        PlayerPrefs.SetFloat(KEY_MASTER_VOLUME, masterVolume);
        PlayerPrefs.Save();

#if !UNITY_WEBGL
        try
        {
            AudioConfigData config = new AudioConfigData { volume = masterVolume };
            string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Tạo thư mục nếu chưa tồn tại
            string directory = Path.GetDirectoryName(configPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(configPath, jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi lưu file AudioConfig.json: {e.Message}");
        }
#endif
    }

    #endregion
}