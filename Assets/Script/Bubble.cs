using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bubble — Quản lý bong bóng thoại hiển thị yêu cầu món ăn / vị.
/// Sử dụng Prefab "source and number" (hoặc sinh động) để hiển thị các cụm [Vị + Số] dàn đều theo hàng ngang.
/// </summary>
public class Bubble : MonoBehaviour
{
    public bool isAlwaysDisplay = false;
    public float displayDuration = 3.0f;

    [Header("Prefab Cụm Vị & Số")]
    [Tooltip("Kéo Prefab 'source and number' vào đây. Script sẽ tự thay đổi Sprite Vị và Sprite Số")]
    public GameObject flavorNumberPrefab;

    [SerializeField] private GameObject bubbleObject; // GameObject chứa Sprite mẹ (Khung thoại)
    [SerializeField] private float padding = 0.15f;   // Lề thụt vào bên trong

    [Header("Icon Settings")]
    [Tooltip("Khoảng cách giữa Icon Vị và Icon Số (khi không dùng prefab)")]
    public float flavorNumberGap = 0.28f;

    [Tooltip("Kích thước tổng thể của các cụm vị")]
    public float globalScale = 1.0f;

    [Header("Content Alignment (Căn chỉnh vùng chứa Icon)")]
    [Tooltip("Độ lệch tâm của vùng chứa icon (ví dụ: X > 0 để dịch sang phải tránh Avatar)")]
    public Vector2 contentOffset = Vector2.zero;

    [Tooltip("Tỷ lệ chiều rộng vùng chứa icon (1.0 = toàn bộ khung, 0.55 = nửa bên phải)")]
    [Range(0.1f, 1.0f)]
    public float contentWidthRatio = 1.0f;

    public List<Sprite> fourSprites = new List<Sprite>();
    private Coroutine displayCoroutine;

    /// <summary>
    /// Khởi tạo và dàn đều các cụm vị [Icon Vị + Số] THEO HÀNG NGANG.
    /// Nhận danh sách các Sprite: [Vị 1, Số 1, Vị 2, Số 2, ...].
    /// </summary>
    public void SetupBubble(List<Sprite> sprites)
    {
        ClearExistingChildren();

        if (sprites == null || sprites.Count == 0 || sprites.Count % 2 != 0)
        {
            if (bubbleObject != null && !isAlwaysDisplay) bubbleObject.SetActive(false);
            return;
        }

        this.fourSprites = sprites;

        if (bubbleObject != null)
        {
            SpriteRenderer parentSR = bubbleObject.GetComponent<SpriteRenderer>();
            if (parentSR == null || parentSR.sprite == null) return;

            // Đảm bảo sortingOrder của Bubble đủ cao để hiển thị trên cùng
            if (parentSR.sortingOrder < 35) parentSR.sortingOrder = 35;
            int order = parentSR.sortingOrder + 1;
            int layerID = parentSR.sortingLayerID;

            // 1. Lấy kích thước và tâm thực tế của Sprite khung thoại (cộng thêm contentOffset)
            Bounds parentBounds = parentSR.sprite.bounds;
            Vector3 baseCenter = parentBounds.center + new Vector3(contentOffset.x, contentOffset.y, 0f);

            // 2. Vùng sử dụng bên trong khung (theo contentWidthRatio)
            float totalW = parentBounds.size.x * contentWidthRatio;
            float padX = Mathf.Max(padding, totalW * 0.1f);
            float padY = Mathf.Max(padding, parentBounds.size.y * 0.2f);

            float innerWidth = Mathf.Max(0.1f, totalW - (padX * 2f));
            float innerHeight = Mathf.Max(0.1f, parentBounds.size.y - (padY * 2f));

            // Số cụm vị cần hiển thị trên hàng ngang (1 vị = 2 sprite, 2 vị = 4 sprite)
            int pairCount = fourSprites.Count / 2;
            float stepWidth = innerWidth / pairCount;

            for (int k = 0; k < pairCount; k++)
            {
                Sprite flavorSprite = fourSprites[k * 2];     // Icon Vị (Ớt, Muối, v.v.)
                Sprite numberSprite = fourSprites[k * 2 + 1]; // Icon Số (1, 2, 3, v.v.)

                // Tọa độ X tâm của cụm vị thứ k (dàn đều theo vùng content)
                float pairCenterX = baseCenter.x + (k - (pairCount - 1) / 2.0f) * stepWidth;
                float pairCenterY = baseCenter.y;

                // ─── CÁCH 1: DÙNG PREFAB "source and number" ───
                if (flavorNumberPrefab != null)
                {
                    GameObject pairObj = Instantiate(flavorNumberPrefab, bubbleObject.transform);

                    // Tìm các SpriteRenderer bên trong Prefab
                    Transform sourceT = pairObj.transform.Find("source");
                    Transform numberT = pairObj.transform.Find("number");

                    SpriteRenderer sourceSR = sourceT != null ? sourceT.GetComponent<SpriteRenderer>() : null;
                    SpriteRenderer numberSR = numberT != null ? numberT.GetComponent<SpriteRenderer>() : null;

                    // Fallback nếu tên GameObject trong prefab khác
                    if (sourceSR == null || numberSR == null)
                    {
                        SpriteRenderer[] allSR = pairObj.GetComponentsInChildren<SpriteRenderer>();
                        if (allSR.Length >= 2)
                        {
                            sourceSR = allSR[0];
                            numberSR = allSR[1];
                        }
                    }

                    // Đổi Sprite vị và số
                    if (sourceSR != null)
                    {
                        sourceSR.sprite = flavorSprite;
                        sourceSR.sortingLayerID = layerID;
                        sourceSR.sortingOrder = order;
                    }

                    if (numberSR != null)
                    {
                        numberSR.sprite = numberSprite;
                        numberSR.sortingLayerID = layerID;
                        numberSR.sortingOrder = order + 1;
                    }

                    // Tự động scale cụm prefab vừa vặn với kích thước khung thoại
                    float fitScale;
                    if (pairCount == 1)
                    {
                        fitScale = Mathf.Min(innerWidth * 0.65f / 3.8f, innerHeight / 3.8f) * globalScale;
                    }
                    else
                    {
                        fitScale = Mathf.Min(stepWidth / 3.8f, innerHeight / 3.6f) * globalScale;
                    }
                    pairObj.transform.localScale = new Vector3(fitScale, fitScale, 1f);

                    // Bù trừ offset nội bộ của Prefab "source and number" để căn giữa hoàn hảo tại (pairCenterX, pairCenterY)
                    Vector3 sourceLocalOffset = sourceT != null ? sourceT.localPosition : Vector3.zero;
                    Vector3 comboVisualCenter = sourceLocalOffset + new Vector3(0.35f, 0.55f, 0f);
                    pairObj.transform.localPosition = new Vector3(pairCenterX, pairCenterY, 0f) - comboVisualCenter * fitScale;
                }
                // ─── CÁCH 2: FALLBACK SINH TỰ ĐỘNG NẾU CHƯA GÁN PREFAB ───
                else
                {
                    float maxIconDim = Mathf.Min(stepWidth * 0.40f, innerHeight * 0.70f) * globalScale;
                    float halfGap = Mathf.Min(flavorNumberGap, stepWidth * 0.22f);

                    // Tạo Icon Vị
                    if (flavorSprite != null)
                    {
                        GameObject flavorGO = new GameObject($"Flavor_{k}");
                        flavorGO.transform.SetParent(bubbleObject.transform, false);
                        flavorGO.transform.localPosition = new Vector3(pairCenterX - halfGap, pairCenterY, 0f);

                        SpriteRenderer sr = flavorGO.AddComponent<SpriteRenderer>();
                        sr.sprite = flavorSprite;
                        sr.sortingLayerID = layerID;
                        sr.sortingOrder = order;

                        Bounds b = flavorSprite.bounds;
                        if (b.size.x > 0 && b.size.y > 0)
                        {
                            float maxB = Mathf.Max(b.size.x, b.size.y);
                            float fit = maxIconDim / maxB;
                            flavorGO.transform.localScale = new Vector3(fit, fit, 1f);
                        }
                    }

                    // Tạo Icon Số
                    if (numberSprite != null)
                    {
                        GameObject numberGO = new GameObject($"Number_{k}");
                        numberGO.transform.SetParent(bubbleObject.transform, false);
                        numberGO.transform.localPosition = new Vector3(pairCenterX + halfGap, pairCenterY, 0f);

                        SpriteRenderer sr = numberGO.AddComponent<SpriteRenderer>();
                        sr.sprite = numberSprite;
                        sr.sortingLayerID = layerID;
                        sr.sortingOrder = order;

                        Bounds b = numberSprite.bounds;
                        if (b.size.x > 0 && b.size.y > 0)
                        {
                            float maxB = Mathf.Max(b.size.x, b.size.y);
                            float fit = maxIconDim / maxB;
                            numberGO.transform.localScale = new Vector3(fit, fit, 1f);
                        }
                    }
                }
            }

            bubbleObject.SetActive(true);
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
