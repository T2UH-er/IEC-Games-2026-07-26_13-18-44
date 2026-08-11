using UnityEngine;
using System.Collections.Generic;

public static class SpriteGridUtility
{
    /// <summary>
    /// Tạo các Sprite con xếp dạng lưới 2x2 (tối đa 4) và căn chính giữa Sprite mẹ.
    /// Nếu danh sách ít hơn 4 Sprite, các vị trí còn lại sẽ bỏ trống.
    /// </summary>
    public static void InstantiateGridInSprite(GameObject parent, List<Sprite> childSprites, float padding = 0f)
    {
        // 1. Kiểm tra đầu vào
        if (parent == null)
        {
            Debug.LogError("[SpriteGridUtility] GameObject mẹ không tồn tại!");
            return;
        }

        SpriteRenderer parentSR = parent.GetComponent<SpriteRenderer>();
        if (parentSR == null || parentSR.sprite == null)
        {
            Debug.LogError("[SpriteGridUtility] GameObject mẹ phải có SpriteRenderer và được gán Sprite!");
            return;
        }

        // Cho phép danh sách từ 1 đến 4 Sprite
        if (childSprites == null || childSprites.Count == 0 || childSprites.Count > 4)
        {
            Debug.LogWarning("[SpriteGridUtility] Danh sách Sprite phải có từ 1 đến 4 phần tử!");
            return;
        }

        // 2. Lấy kích thước thực tế theo Local Space của Sprite mẹ
        Bounds localBounds = parentSR.sprite.bounds;
        float parentWidth = localBounds.size.x - (padding * 2);
        float parentHeight = localBounds.size.y - (padding * 2);

        Vector3 parentLocalCenter = localBounds.center;

        // Tọa độ tương đối cho 4 ô lưới 2x2
        // [0]: Trên-Trái, [1]: Trên-Phải, [2]: Dưới-Trái, [3]: Dưới-Phải
        Vector3[] gridOffsets = new Vector3[4]
        {
            new Vector3(-parentWidth / 4f,  parentHeight / 4f, 0f),
            new Vector3( parentWidth / 4f,  parentHeight / 4f, 0f),
            new Vector3(-parentWidth / 4f, -parentHeight / 4f, 0f),
            new Vector3( parentWidth / 4f, -parentHeight / 4f, 0f)
        };

        float slotWidth = parentWidth / 2f;
        float slotHeight = parentHeight / 2f;

        // 3. Khởi tạo Sprite con theo số lượng thực tế có trong List (tối đa 4)
        for (int i = 0; i < childSprites.Count; i++)
        {
            // Bỏ qua nếu phần tử trong List bị null
            if (childSprites[i] == null) continue;

            // Tạo GameObject con
            GameObject childGO = new GameObject($"ChildSprite_{i}");
            childGO.transform.SetParent(parent.transform, false);

            // Đặt vị trí Local chuẩn theo ô trong lưới
            childGO.transform.localPosition = parentLocalCenter + gridOffsets[i];
            childGO.transform.localRotation = Quaternion.identity;
            childGO.transform.localScale = Vector3.one;

            // Thêm SpriteRenderer
            SpriteRenderer childSR = childGO.AddComponent<SpriteRenderer>();
            childSR.sprite = childSprites[i];

            // Đảm bảo hiển thị phía trên Sprite mẹ
            childSR.sortingLayerID = parentSR.sortingLayerID;
            childSR.sortingOrder = parentSR.sortingOrder + 1;

            // 4. Scale Sprite con vừa vặn với ô lưới
            Bounds childBounds = childSprites[i].bounds;
            if (childBounds.size.x > 0 && childBounds.size.y > 0)
            {
                float scaleX = slotWidth / childBounds.size.x;
                float scaleY = slotHeight / childBounds.size.y;

                float finalScale = Mathf.Min(scaleX, scaleY);
                childGO.transform.localScale = new Vector3(finalScale, finalScale, 1f);
            }
        }
    }
}