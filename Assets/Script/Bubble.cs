using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public bool isAlwaysDisplay = false;
    public float displayDuration = 3.0f;

    [SerializeField] private GameObject bubbleObject; // GameObject chứa Sprite mẹ
    [SerializeField] private float iconSpacing = 0.5f; // Khoảng cách giữa các icon

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

        ClearExistingChildren();

        if (bubbleObject != null)
        {
            SpriteRenderer parentSR = bubbleObject.GetComponent<SpriteRenderer>();
            int order = parentSR != null ? parentSR.sortingOrder + 1 : 1;
            int layerID = parentSR != null ? parentSR.sortingLayerID : 0;

            int count = fourSprites.Count;
            float spacing = (GridManager1.Instance != null && GridManager1.Instance.cellSize > 0) ? GridManager1.Instance.cellSize : iconSpacing;

            for (int i = 0; i < count; i++)
            {
                if (fourSprites[i] == null) continue;

                GameObject childGO = new GameObject($"FlavorIcon_{i}");
                childGO.transform.SetParent(bubbleObject.transform, false);

                // Công thức tính vị trí cân đối 2 bên cách nhau đúng bằng kích thước 1 khối (cellSize)
                float offsetX = (i - (count - 1) / 2.0f) * spacing;
                childGO.transform.localPosition = new Vector3(offsetX, 0f, 0f);

                SpriteRenderer childSR = childGO.AddComponent<SpriteRenderer>();
                childSR.sprite = fourSprites[i];
                childSR.sortingLayerID = layerID;
                childSR.sortingOrder = order;
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
        yield return new WaitForSeconds(displayDuration);
        bubbleObject.SetActive(false);
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