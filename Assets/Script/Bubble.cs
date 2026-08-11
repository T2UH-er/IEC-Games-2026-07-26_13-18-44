using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public bool isAlwaysDisplay = true;

    [SerializeField] private GameObject bubbleObject; // GameObject chứa Sprite mẹ
    [SerializeField] private float padding = 0.1f; // Lề thụt vào bên trong Sprite mẹ (tùy chọn)

    public List<Sprite> fourSprites = new List<Sprite>();
    private Coroutine displayCoroutine;

    /// <summary>
    /// Hàm khởi tạo dữ liệu Sprite và vẽ lưới. Nhận từ 1 đến 4 Sprite.
    /// </summary>
    public void SetupBubble(List<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0 || sprites.Count > 4)
        {
            Debug.LogWarning("[Bubble] Cần truyền từ 1 đến 4 Sprite!");
            return;
        }

        this.fourSprites = sprites;

        // Xóa các sprite con cũ (nếu có) trước khi tạo mới
        ClearExistingChildren();

        // Tiến hành tạo lưới với số lượng Sprite thực tế
        SpriteGridUtility.InstantiateGridInSprite(bubbleObject, fourSprites, padding);

        // Cấu hình trạng thái ẩn/hiện
        if (isAlwaysDisplay)
        {
            bubbleObject.SetActive(true);
        }
        else
        {
            bubbleObject.SetActive(false);
        }
    }

    public void ShowBubble()
    {
        if (isAlwaysDisplay) return;

        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        displayCoroutine = StartCoroutine(DisplayTheBubbleRoutine());
    }

    private IEnumerator DisplayTheBubbleRoutine()
    {
        bubbleObject.SetActive(true);
        yield return new WaitForSeconds(3.0f);
        bubbleObject.SetActive(false);
        displayCoroutine = null;
    }

    private void ClearExistingChildren()
    {
        foreach (Transform child in bubbleObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
}