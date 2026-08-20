using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ResolutionAdapter: Tự động điều chỉnh tầm nhìn của Camera và Canvas Scaler
/// để game hiển thị hoàn hảo trên mọi kích thước màn hình dọc (9:16, 19.5:9, 20:9, 3:4, etc.)
/// Gắn script này vào Main Camera trong Scene.
/// </summary>
[RequireComponent(typeof(Camera))]
[ExecuteAlways]
public class ResolutionAdapter : MonoBehaviour
{
    [Header("Target Design Resolution")]
    [Tooltip("Chiều rộng thiết kế chuẩn (mặc định 1080)")]
    public float targetWidth = 1080f;

    [Tooltip("Chiều cao thiết kế chuẩn (mặc định 1920)")]
    public float targetHeight = 1920f;

    [Header("Camera Settings")]
    [Tooltip("Kích thước Orthographic chuẩn trong Scene lúc thiết kế")]
    public float targetOrthographicSize = 36f;

    [Header("Optional Canvas Scaler (Để trống nếu tự chỉnh tay)")]
    public CanvasScaler canvasScaler;

    private Camera cam;
    private float lastScreenWidth = -1;
    private float lastScreenHeight = -1;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        if (cam != null && !cam.orthographic)
        {
            cam.orthographic = true;
        }

        // Tự động tìm CanvasScaler nếu chưa gán
        if (canvasScaler == null)
        {
            canvasScaler = FindObjectOfType<CanvasScaler>();
        }

        UpdateResolution();
    }

    private void Update()
    {
        // Kiểm tra nếu kích thước màn hình thay đổi (ví dụ người dùng resize trình duyệt hoặc xoay màn hình)
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateResolution();
        }
    }

    public void UpdateResolution()
    {
        if (cam == null) cam = GetComponent<Camera>();
        if (cam == null || Screen.width <= 0 || Screen.height <= 0) return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float targetAspect = targetWidth / targetHeight; // 1080 / 1920 = 0.5625
        float currentAspect = (float)Screen.width / (float)Screen.height;

        // 1. Điều chỉnh Camera Orthographic Size
        if (cam.orthographic)
        {
            if (currentAspect < targetAspect)
            {
                // Màn hình hẹp/dài hơn chuẩn (ví dụ iPhone 19.5:9, Samsung 20:9)
                // Cần tăng orthographicSize để chiều ngang bàn cờ KHÔNG bị cắt 2 bên
                cam.orthographicSize = targetOrthographicSize * (targetAspect / currentAspect);
            }
            else
            {
                // Màn hình rộng hơn chuẩn (ví dụ iPad 3:4, PC 16:9)
                cam.orthographicSize = targetOrthographicSize;
            }
        }

        // 2. Điều chỉnh Canvas Scaler (Match Width Or Height)
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(targetWidth, targetHeight);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            if (currentAspect < targetAspect)
            {
                // Màn hình hẹp -> Match = 0 (Fit hoàn toàn theo chiều rộng, không bị tràn lề)
                canvasScaler.matchWidthOrHeight = 0f;
            }
            else
            {
                // Màn hình rộng -> Match = 1 (Fit theo chiều cao)
                canvasScaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
}
