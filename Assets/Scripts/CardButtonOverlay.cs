using UnityEngine;
using UnityEngine.InputSystem; // Add this for new Input System
using System.Collections.Generic;

public class CardButtonOverlay : MonoBehaviour
{
    public PlayingCard parentCard;
    private Camera mainCamera;

    // Multi-card selection system
    private static List<PlayingCard> selectedCards = new List<PlayingCard>();
    private static int maxSelectedCards = 5;

    [Header("Hover Effect")]
    public float hoverScaleMultiplier = 1.05f;
    public float hoverAnimationSpeed = 8f;
    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isHovering = false;

    void Start()
    {
        // Find the parent card if not assigned
        if (parentCard == null)
        {
            parentCard = GetComponentInParent<PlayingCard>();
        }

        // Subscribe to card selection event
        if (parentCard != null)
        {
            parentCard.OnCardSelected += HandleCardSelection;
        }

        // Cache the camera reference
        mainCamera = Camera.main;

        // Match the overlay size to the card sprite
        AdjustOverlaySize();

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (parentCard != null)
        {
            parentCard.OnCardSelected -= HandleCardSelection;
        }
    }

    private void AdjustOverlaySize()
    {
        if (parentCard != null && parentCard.GetComponent<SpriteRenderer>() != null)
        {
            SpriteRenderer cardRenderer = parentCard.GetComponent<SpriteRenderer>();
            SpriteRenderer overlayRenderer = GetComponent<SpriteRenderer>();
            BoxCollider2D overlayCollider = GetComponent<BoxCollider2D>();

            // If this overlay has a sprite renderer, match it to the card size
            if (overlayRenderer != null)
            {
                overlayRenderer.size = cardRenderer.sprite.bounds.size;
            }

            // If this overlay has a box collider, match it to the card size
            if (overlayCollider != null)
            {
                overlayCollider.size = cardRenderer.sprite.bounds.size;
            }
        }
    }

    void Update()
    {
        // Handle hover animation
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * hoverAnimationSpeed);

        // Check for hover using the new Input System
        CheckHover();

        // Check for click using the new Input System
        CheckClick();
    }

    private void CheckHover()
    {
        // Make sure we have a mouse device
        if (Mouse.current == null)
            return;

        // Get mouse position from the new Input System
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));

        // Check if mouse is over this object
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            bool isPointOverCollider = collider.OverlapPoint(worldPosition);

            // Only trigger hover if state changes
            if (isPointOverCollider && !isHovering)
            {
                OnHoverEnter();
            }
            else if (!isPointOverCollider && isHovering)
            {
                OnHoverExit();
            }
        }
    }

    private void CheckClick()
    {
        // Make sure we have a mouse device
        if (Mouse.current == null)
            return;

        // Check if left mouse button was pressed this frame
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Get mouse position from the new Input System
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, -mainCamera.transform.position.z));

            // Check if mouse is over this object
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(worldPosition))
            {
                OnClick();
            }
        }
    }

    private void OnHoverEnter()
    {
        isHovering = true;
        targetScale = originalScale * hoverScaleMultiplier;
    }

    private void OnHoverExit()
    {
        isHovering = false;
        targetScale = originalScale;
    }

    private void OnClick()
    {
        if (parentCard != null)
        {
            // Call the select card method which will toggle selection
            parentCard.SelectCard();
        }
    }

    // Handle card selection with max card limit
    private void HandleCardSelection(PlayingCard card)
    {
        // Toggle behavior
        if (card.isSelected)
        {
            // Card is now selected

            // Check if we need to enforce the max card limit
            if (selectedCards.Count >= maxSelectedCards && !selectedCards.Contains(card))
            {
                // We're at the limit and trying to add a new card
                // Deselect the oldest card
                if (selectedCards.Count > 0)
                {
                    PlayingCard oldestCard = selectedCards[0];
                    oldestCard.SetSelectionState(false);
                    selectedCards.Remove(oldestCard);
                    Debug.Log($"Max cards reached. Deselected oldest card: {oldestCard.rank} of {oldestCard.suit}");
                }
            }

            // Add this card to the selected list if not already there
            if (!selectedCards.Contains(card))
            {
                selectedCards.Add(card);
                Debug.Log($"Card selected: {card.rank} of {card.suit}. Total selected: {selectedCards.Count}/{maxSelectedCards}");
            }
        }
        else
        {
            // Card is now deselected

            // Remove from the selected list
            if (selectedCards.Contains(card))
            {
                selectedCards.Remove(card);
                Debug.Log($"Card deselected: {card.rank} of {card.suit}. Total selected: {selectedCards.Count}/{maxSelectedCards}");
            }
        }
    }

    // Static method to get currently selected cards
    public static List<PlayingCard> GetSelectedCards()
    {
        return selectedCards;
    }

    // Static method to set the maximum number of cards that can be selected
    public static void SetMaxSelectedCards(int max)
    {
        maxSelectedCards = Mathf.Max(1, max); // Ensure at least 1
        Debug.Log($"Maximum selectable cards set to: {maxSelectedCards}");
    }

    // Static method to clear all selections
    public static void ClearAllSelections()
    {
        foreach (var card in selectedCards)
        {
            card.SetSelectionState(false);
        }
        selectedCards.Clear();
        Debug.Log("All card selections cleared");
    }
}