using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardSelection : MonoBehaviour
{
    [Header("Selection Settings")]
    public float selectedScale = 1.1f;
    public Color selectedTint = Color.yellow;
    public float selectionLerpSpeed = 10f;

    private Player player;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isSelected = false;

    void Awake()
    {
        player = FindAnyObjectByType<Player>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Smooth animation for selection/deselection
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            isSelected ? originalScale * selectedScale : originalScale,
            Time.deltaTime * selectionLerpSpeed
        );

        spriteRenderer.color = Color.Lerp(
            spriteRenderer.color,
            isSelected ? selectedTint : originalColor,
            Time.deltaTime * selectionLerpSpeed
        );
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
    }

    public bool IsSelected()
    {
        return isSelected;
    }
}