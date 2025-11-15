using UnityEngine;

public class TileVisual : MonoBehaviour
{
    private Color originalColor;
    private Renderer tileRenderer;
    public Tile tileData; 

    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            originalColor = tileRenderer.material.color;
        }
    }

    public void Highlight(Color color)
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = color;
        }
    }

    public void ResetColor()
    {
        if (tileRenderer != null)
        {
            tileRenderer.material.color = originalColor;
        }
    }
}