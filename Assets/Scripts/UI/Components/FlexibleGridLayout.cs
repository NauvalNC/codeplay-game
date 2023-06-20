using UnityEngine;
using UnityEngine.UI;

public class FlexibleGridLayout : LayoutGroup
{
    public enum FitType
    {
        Uniform,
        Width,
        Height,
        FixedRows,
        FixedColumns
    }

    [Header("Layout Properties")]
    public int rows;
    public int columns;
    public Vector2 spacing;
    private Vector2 cellSize;

    [Header("Fitting Properties")]
    public FitType fitType;
    public bool fitX;
    public bool fitY;

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fitType == FitType.Width || fitType == FitType.Height || fitType == FitType.Uniform)
        {
            fitX = true;
            fitY = true;

            float tSqrt = Mathf.Sqrt(transform.childCount);
            rows = Mathf.CeilToInt(tSqrt);
            columns = Mathf.CeilToInt(tSqrt);
        }

        if (fitType == FitType.Width || fitType == FitType.FixedColumns)
        {
            rows = Mathf.CeilToInt(transform.childCount / (float)columns);
        }
        else if (fitType == FitType.Height || fitType == FitType.FixedRows)
        {
            columns = Mathf.CeilToInt(transform.childCount / (float)rows);
        }

        float tParentWidth = rectTransform.rect.width;
        float tParentHeight = rectTransform.rect.height;

        float tCellWidth = (tParentWidth / (float)columns) - ((spacing.x / (float)columns) * 2) - (padding.left / (float)columns) - (padding.right / (float)columns);
        float tCellHeight = (tParentHeight / (float)rows) - ((spacing.y / (float)rows) * 2) - (padding.top / (float)rows) - (padding.bottom / (float)rows);

        cellSize.x = fitX ? tCellWidth : cellSize.x;
        cellSize.y = fitY ? tCellHeight : cellSize.y;

        int tTargetColumn = 0;
        int tTargetRow = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            tTargetRow = i / columns;
            tTargetColumn = i % columns;

            RectTransform tItem = rectChildren[i];
            float xPos = (cellSize.x * tTargetColumn) + (spacing.x * tTargetColumn) + padding.left;
            float yPos = (cellSize.y * tTargetRow) + (spacing.y * tTargetRow) + padding.top;

            SetChildAlongAxis(tItem, 0, xPos, cellSize.x);
            SetChildAlongAxis(tItem, 1, yPos, cellSize.y);
        }
    }

    public override void CalculateLayoutInputVertical()
    {

    }

    public override void SetLayoutHorizontal()
    {

    }

    public override void SetLayoutVertical()
    {

    }
}
