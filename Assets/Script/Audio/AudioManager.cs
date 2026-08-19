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

    // Đọc hệ số âm lượng mặc định từ file .json ngoài
    private void LoadAudioConfig()
    {
        if (File.Exists(configPath))
        {
            try
            {
                string jsonContent = File.ReadAllText(configPath);
                AudioConfigData config = JsonConvert.DeserializeObject<AudioConfigData>(jsonContent);

                if (config != null)
                {
                    masterVolume = config.volume;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Lỗi khi đọc file AudioConfig.json: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning("Không tìm thấy file AudioConfig.json tại StreamingAssets. Dùng giá trị mặc định.");
        }
    }

    #region Play Methods

    // Overload 1: Không có tham số đi kèm -> Phát theo âm lượng mặc định trong ScriptableObject
    public void PlayAudio(string audioName)
    {
        AudioData data = audioDatabase.GetAudio(audioName);
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
        AudioData data = audioDatabase.GetAudio(audioName);
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
        float finalVolume = inputVolume * masterVolume;

        if (data.audioType == AudioType.BGM)
        {
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
        AudioData data = audioDatabase.GetAudio(audioName);
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
        SaveAudioConfig();
    }

    // Hàm ghi dữ liệu xuống file .json
    public void SaveAudioConfig()
    {
        try
        {
            AudioConfigData config = new AudioConfigData { volume = masterVolume };
            string jsonContent = JsonConvert.SerializeObject(config, Formatting.Indented);

            // Tạo thư mục nếu chưa tồn tại
            string directory = Path.GetDirectoryName(configPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(configPath, jsonContent);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Lỗi khi lưu file AudioConfig.json: {e.Message}");
        }
    }

    #endregion
}