using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBlockInfo1 : MonoBehaviour
{
    public BlockShapeData shapeData;
    public Vector2Int originCell;
    public ItemData1[] itemPerCell;
    public GameObject cellIconPrefab;
    public float rotationAngle = 0f;

    private Coroutine flashCoroutine;

    /// <summary>
    /// Hiệu ứng nhấp nháy màu đỏ khi không thể xoay khối trên Grid
    /// </summary>
    public void FlashRed(float duration = 0.35f)
    {
        if (GridManager1.Instance == null) return;
        List<PlacedBlockInfo1> blockCells = GridManager1.Instance.GetAllCellsOfBlock(originCell, shapeData);
        foreach (var cellInfo in blockCells)
        {
            if (cellInfo != null)
                cellInfo.StartFlashIndividual(duration);
        }
    }

    public void StartFlashIndividual(float duration)
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRedRoutine(duration));
    }

    private IEnumerator FlashRedRoutine(float duration)
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        if (srs == null || srs.Length == 0) yield break;

        Color[] originalColors = new Color[srs.Length];
        for (int i = 0; i < srs.Length; i++)
            originalColors[i] = srs[i].color;

        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        Color flashColor = new Color(1f, 0.25f, 0.25f, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            // Rung nhẹ (Shake)
            float shakeOffset = Mathf.Sin(elapsed * 40f) * 0.08f * (1f - t);
            transform.localPosition = originalPos + new Vector3(shakeOffset, 0f, 0f);

            // Nháy đỏ rồi về bình thường
            Color currentColor = Color.Lerp(flashColor, Color.white, t);
            for (int i = 0; i < srs.Length; i++)
            {
                if (srs[i] != null)
                    srs[i].color = currentColor;
            }

            yield return null;
        }

        transform.localPosition = originalPos;
        for (int i = 0; i < srs.Length; i++)
        {
            if (srs[i] != null)
                srs[i].color = originalColors[i];
        }

        flashCoroutine = null;
    }
}
