using UnityEngine;
using UnityEngine.UI;

public class ButtonGridGenerator : MonoBehaviour
{
    [Header("Cài đặt Component cha")]
    [Tooltip("Kéo object Content của Scroll View vào đây")]
    public RectTransform contentParent;

    [Header("Cài đặt UI Nút")]
    public Sprite buttonSprite;
    public Font buttonFont;
    public float buttonEdgeLength = 100f; // Độ dài cạnh của nút hình vuông
    public int fontSize = 24;            // Cỡ chữ trong nút

    [Header("Cài đặt số lượng & Bố cục")]
    public int numberOfButtons = 30;
    public int buttonsPerRow = 5;        // Cố định 5 nút 1 hàng
    public float paddingLeftRight = 20f; // Lề trái và lề phải của Content

    void Start()
    {
        GenerateButtons();
    }

    void GenerateButtons()
    {
        // 1. Tự động thêm Content Size Fitter nếu chưa có để Content tự co giãn chiều cao khi cuộn
        ContentSizeFitter sizeFitter = contentParent.GetComponent<ContentSizeFitter>();
        if (sizeFitter == null)
        {
            sizeFitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
        }
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 2. Lấy hoặc thêm tự động Grid Layout Group cho object cha
        GridLayoutGroup grid = contentParent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = contentParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Cố định 5 cột
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = buttonsPerRow;

        // Đặt kích thước ô vuông cho nút
        grid.cellSize = new Vector2(buttonEdgeLength, buttonEdgeLength);

        // Đặt Padding lề trái/phải
        grid.padding.left = Mathf.RoundToInt(paddingLeftRight);
        grid.padding.right = Mathf.RoundToInt(paddingLeftRight);

        // 3. TÍNH TOÁN DÀN ĐỀU THEO CHIỀU NGANG:
        Canvas.ForceUpdateCanvases();
        float totalWidth = contentParent.rect.width;

        // Tổng chiều rộng khả dụng dành cho khoảng cách (Spacing) giữa các nút
        float availableWidthForSpacing = totalWidth - (paddingLeftRight * 2) - (buttonEdgeLength * buttonsPerRow);

        // Khoảng cách giữa 5 nút sẽ có 4 khoảng trống (buttonsPerRow - 1)
        float horizontalSpacing = 0f;
        if (buttonsPerRow > 1)
        {
            horizontalSpacing = availableWidthForSpacing / (buttonsPerRow - 1);
        }

        // Nếu màn hình quá nhỏ làm spacing bị âm, ép về 0
        if (horizontalSpacing < 0) horizontalSpacing = 0;

        // Gán khoảng cách ngang (x) và khoảng cách dọc (y - mặc định 20f)
        grid.spacing = new Vector2(horizontalSpacing, 20f);

        // 4. Vòng lặp tạo 30 nút
        for (int i = 1; i <= numberOfButtons; i++)
        {
            CreateSingleButton(i);
        }
    }

    void CreateSingleButton(int index)
    {
        // --- TẠO OBJECT NÚT ---
        GameObject buttonObj = new GameObject("Button_" + index);
        buttonObj.transform.SetParent(contentParent, false);

        // Thêm các component cấu thành UI
        buttonObj.AddComponent<RectTransform>();
        buttonObj.AddComponent<CanvasRenderer>();

        // Cấu hình Image (Sprite)
        Image btnImage = buttonObj.AddComponent<Image>();
        if (buttonSprite != null)
        {
            btnImage.sprite = buttonSprite;
        }

        // Cấu hình Button component
        Button btn = buttonObj.AddComponent<Button>();

        // --- TẠO OBJECT TEXT CON ---
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);

        // Cấu hình RectTransform cho Text full viền nút
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        textObj.AddComponent<CanvasRenderer>();

        // Cấu hình Text
        Text btnText = textObj.AddComponent<Text>();
        btnText.text = index.ToString();
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.black;

        // Cấu hình cỡ chữ
        btnText.fontSize = fontSize;
        btnText.resizeTextForBestFit = true; // Tự thu nhỏ chữ nếu kích thước nút quá nhỏ không chứa vừa
        btnText.resizeTextMinSize = 10;
        btnText.resizeTextMaxSize = fontSize;

        // Gắn font chữ
        if (buttonFont != null)
        {
            btnText.font = buttonFont;
        }
        else
        {
            btnText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
    }
}