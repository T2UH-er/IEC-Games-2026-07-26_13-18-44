using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public bool isAlwaysDisplay = false;
    public float displayDuration = 3.0f;

    [SerializeField] private GameObject bubbleObject; // GameObject chứa Sprite mẹ
    [SerializeField] private float padding = 0.1f;    // Lề thụt vào bên trong Sprite mẹ

    [Header("Icon Settings")]
    [Tooltip("Kích thước cạnh cố định (đơn vị Unity Unit) áp dụng cho tất cả Sprite con")]
    [SerializeField] private float globalScale = 1.0f;

    [SerializeField] private float iconSpacing = 0.01f;

    public List<Sprite> fourSprites = new List<Sprite>();
    private Coroutine displayCoroutine;

    /// <summary>
    /// Hàm khởi tạo dữ liệu Sprite và vẽ lưới. Nhận từ 1 đến 4 Sprite.
    /// </summary>
    public void SetupBubble(List<Sprite> sprites)
    {
        if (sprites == null || sprites.Count == 0 || sprites.Count > 4) return;
        if (sprites.Count % 2 != 0) return;

        this.fourSprites = sprites;
        ClearExistingChildren();

        if (bubbleObject != null)
        {
            SpriteRenderer parentSR = bubbleObject.GetComponent<SpriteRenderer>();
            if (parentSR == null || parentSR.sprite == null) return;

            int order = parentSR.sortingOrder + 1;
            int layerID = parentSR.sortingLayerID;

            // 1. LẤY KÍCH THƯỚC THỰC TẾ VÀ TÂM THỰC TẾ CỦA SPRITE MẸ (Bỏ qua ảnh hưởng của Pivot)
            Bounds parentBounds = parentSR.sprite.bounds;
            Vector3 parentCenter = parentBounds.center; // Tâm chuẩn của khung chữ nhật

            // Trừ bớt padding để icon không bị dính sát lề
            float innerWidth = parentBounds.size.x - (padding * 2f);
            float innerHeight = parentBounds.size.y - (padding * 2f);

            int count = fourSprites.Count;
            int cols = 2;
            int rows = count > 2 ? 2 : 1;

            // Tính khoảng cách tự động dựa trên kích thước khung mẹ (Chia thành 2 cột, 2 hàng)
            float stepX = innerWidth / cols;
            float stepY = innerHeight / rows;

            // 2. Tìm Sprite có kích thước lớn nhất để Scale đồng bộ
            Vector2 maxSpriteSize = Vector2.zero;
            foreach (var sp in fourSprites)
            {
                if (sp != null)
                {
                    if (sp.bounds.size.x > maxSpriteSize.x) maxSpriteSize.x = sp.bounds.size.x;
                    if (sp.bounds.size.y > maxSpriteSize.y) maxSpriteSize.y = sp.bounds.size.y;
                }
            }

            for (int i = 0; i < count; i++)
            {
                if (fourSprites[i] == null) continue;

                GameObject childGO = new GameObject($"FlavorIcon_{i}");
                childGO.transform.SetParent(bubbleObject.transform, false);

                int col = i % cols;
                int row = i / cols;

                // Tính vị trí chuẩn căn từ parentCenter (Bất kể Pivot của Sprite mẹ đặt ở đâu)
                float offsetX = (col - (cols - 1) / 2.0f) * (innerWidth / 2f);
                float offsetY = ((rows - 1) / 2.0f - row) * (innerHeight / 2f);

                // Gán vị trí cộng thêm parentCenter
                childGO.transform.localPosition = parentCenter + new Vector3(offsetX, offsetY, 0f);

                SpriteRenderer childSR = childGO.AddComponent<SpriteRenderer>();
                childSR.sprite = fourSprites[i];
                childSR.sortingLayerID = layerID;
                childSR.sortingOrder = order;

                // 3. SCALE VỪA KHÍT TỪNG Ô (Không lo bị đè hay tràn lề)
                Bounds currentBounds = fourSprites[i].bounds;
                if (currentBounds.size.x > 0 && currentBounds.size.y > 0)
                {
                    // Giới hạn icon tối đa chỉ chiếm 80% kích thước của ô cờ nhỏ (stepX, stepY)
                    float maxAllowedW = stepX * 0.8f;
                    float maxAllowedH = stepY * 0.8f;

                    float scaleX = maxAllowedW / currentBounds.size.x;
                    float scaleY = maxAllowedH / currentBounds.size.y;

                    float fitScale = Mathf.Min(scaleX, scaleY) * globalScale;
                    childGO.transform.localScale = new Vector3(fitScale, fitScale, 1f);
                }
            }

            bubbleObject.SetActive(isAlwaysDisplay);
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
        Debug.Log("activate!");
        yield return new WaitForSeconds(displayDuration);
        bubbleObject.SetActive(false);
        Debug.Log("deactivate!");
        displayCoroutine = null;
    }

    private void ClearExistingChildren()
    {
        if (bubbleObject == null) return;
        for (int i = bubbleObject.transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = bubbleObject.transform.GetChild(i).gameObject;
            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }
    }
}