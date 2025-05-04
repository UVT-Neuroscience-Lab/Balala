using UnityEngine;
using System.Collections.Generic;
using Gtec.UnityInterface;

public class Player : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform handArea;

    [Header("Hand Settings")]
    public int handSize = 5;
    public float cardSpacing = 2.2f;

    private List<PlayingCard> cardsInHand = new List<PlayingCard>();
    private PlayingCard currentlySelectedCard = null;

    [Header("BCI Training Settings")]
    public Sprite darkSprite;
    public Sprite flashSprite;
    public ERPFlashController2D bciManager;
    public GameObject bciOverlayPrefab; // Prefab for BCI training overlay

    [Header("Visual Settings")]
    public Vector3 handPosition = new Vector3(-2.0f, -3.5f, 0f);

    [Header("Selection Settings")]
    public bool allowMultipleSelection = false; // Set to true if you want to allow multiple cards to be selected

    void Start()
    {
        // Validate we have all necessary references
        if (gameManager == null)
        {
            Debug.LogError("Player is missing GameManager reference!");
            return;
        }

        if (bciOverlayPrefab == null)
        {
            Debug.LogWarning("BCI Overlay prefab is missing! Please assign a prefab with a SpriteRenderer component.");
        }
    }

    // This will be called by the GameManager when it's ready
    public void DisplayCards()
    {
        // Clear hand and reset selection state
        ClearHand();
        currentlySelectedCard = null;

        // Create new cards
        for (int i = 0; i < handSize; i++)
        {
            AddCardToHand(i);
        }

        Debug.Log($"Displayed new hand with {cardsInHand.Count} cards, no cards selected");
    }

    // Add a single card to the hand at the specified position
    private void AddCardToHand(int cardIndex)
    {
        // Calculate position for the card
        Vector3 cardPosition = CalculateCardPosition(cardIndex);

        // Draw a card from the deck
        PlayingCard card = gameManager.DrawCardToPosition(cardPosition);

        if (card != null)
        {
            // Set parent for organization
            if (handArea != null)
            {
                card.transform.SetParent(handArea, true);
            }

            // Capture base position after layout
            card.InitializeBasePosition();

            // Add to our hand list
            cardsInHand.Add(card);

            // Create BCI training overlay for this card
            CreateBCIOverlayForCard(card, cardIndex);

            // Always show the card face up
            card.ShowFront();

            // Register for selection events
            card.OnCardSelected += HandleCardSelected;
        }
    }

    // Create BCI training overlay for a card
    private void CreateBCIOverlayForCard(PlayingCard card, int cardIndex)
    {
        if (bciOverlayPrefab != null)
        {
            // Create an overlay object as a child of the card
            GameObject overlayObj = Instantiate(bciOverlayPrefab, card.transform);

            // Position the overlay in the center of the card
            overlayObj.transform.localPosition = Vector3.zero;

            // Make sure it's slightly in front of the card
            Vector3 overlayPos = overlayObj.transform.localPosition;
            overlayPos.z = -2.0f; // Smaller z-offset to ensure it's in front
            overlayObj.transform.localPosition = overlayPos;

            // Make sure the scale is appropriate (might need adjustment based on your card size)
            overlayObj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            // Make sure we have a sprite renderer component to use
            SpriteRenderer spriteRenderer = overlayObj.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = overlayObj.AddComponent<SpriteRenderer>();
            }

            // Set sprite renderer properties for visibility
            spriteRenderer.sprite = darkSprite;
            spriteRenderer.color = new Color(1f, 1f, 1f, 1f); // Full opacity
            spriteRenderer.sortingOrder = 10; // Higher than the card's sorting order

            // Debug check to make sure sprites are assigned
            if (darkSprite == null || flashSprite == null)
            {
                Debug.LogError("Dark sprite or flash sprite is null on Player component!");
            }

            // Create custom BCI object that targets the overlay instead of the card itself
            ERPFlashObject2D objBci = new ERPFlashObject2D();
            objBci.ClassId = cardIndex + 1;
            objBci.GameObject = overlayObj;
            objBci.DarkSprite = darkSprite;
            objBci.FlashSprite = flashSprite;
            objBci.Rotate = false;

            // Add to BCI manager
            if (bciManager != null && bciManager.ApplicationObjects != null)
            {
                bciManager.ApplicationObjects.Add(objBci);
                Debug.Log($"Added BCI overlay for card {cardIndex} with ClassId {objBci.ClassId}");
            }
            else
            {
                Debug.LogError("BCI Manager or its ApplicationObjects list is null!");
            }
        }
        else
        {
            Debug.LogError("BCI Overlay prefab is not assigned. Cannot create training overlay.");
        }
    }

    private void HandleCardSelected(PlayingCard selectedCard)
    {
        Debug.Log($"Selected card: {selectedCard.rank} of {selectedCard.suit}");

        if (!allowMultipleSelection)
        {
            // Single selection mode
            if (currentlySelectedCard != null && currentlySelectedCard != selectedCard)
            {
                // Deselect the previously selected card
                currentlySelectedCard.SetSelectionState(false);
            }

            // Update the currently selected card
            currentlySelectedCard = selectedCard.isSelected ? selectedCard : null;

            // If needed, trigger game actions based on selection
            if (currentlySelectedCard != null)
            {
                // Example: Notify GameManager of selection
                gameManager.OnCardSelected(currentlySelectedCard);
            }
        }
        else
        {
            // Multiple selection mode - no need to deselect others
            // You could implement logic here to track all selected cards in a list
            List<PlayingCard> selectedCards = GetSelectedCards();

            // Calculate and log the total value of selected cards
            int totalValue = GetSelectedCardsValue();
            Debug.Log($"Total value of selected cards: {totalValue}");

            // Example: Notify GameManager with all selected cards
            if (selectedCards.Count > 0)
            {
                gameManager.OnMultipleCardsSelected(selectedCards);
            }
        }
    }

    // Reset all selection state
    public void ResetSelectionState()
    {
        Debug.Log("Explicitly resetting all card selection states");

        // Reset the currently selected card reference
        currentlySelectedCard = null;

        // Deselect all cards in hand (just to be safe, though they should be cleared already)
        foreach (PlayingCard card in cardsInHand)
        {
            if (card != null && card.isSelected)
            {
                card.SetSelectionState(false);
            }
        }
    }

    // Get all currently selected cards
    public List<PlayingCard> GetSelectedCards()
    {
        List<PlayingCard> selectedCards = new List<PlayingCard>();

        foreach (PlayingCard card in cardsInHand)
        {
            if (card.isSelected)
            {
                selectedCards.Add(card);
            }
        }

        return selectedCards;
    }

    // Calculate the total value of all selected cards
    public int GetSelectedCardsValue()
    {
        int totalValue = 0;
        List<PlayingCard> selectedCards = GetSelectedCards();

        foreach (PlayingCard card in selectedCards)
        {
            totalValue += gameManager.GetCardValue(card);
        }

        return totalValue;
    }

    // Calculate the position for a card in the hand
    private Vector3 CalculateCardPosition(int cardIndex)
    {
        Vector3 basePosition = handArea != null ? handArea.position : handPosition;

        // Layout the cards in a row with specified spacing
        float xOffset = cardIndex * cardSpacing - ((handSize - 1) * cardSpacing * 0.5f);

        return new Vector3(
            basePosition.x + xOffset,
            basePosition.y,
            basePosition.z - (cardIndex * 0.01f)  // Small z offset to prevent z-fighting
        );
    }

    // Remove all cards from hand
    public void ClearHand()
    {
        Debug.Log($"Clearing hand with {cardsInHand.Count} cards");

        // First reset selection state
        currentlySelectedCard = null;

        foreach (PlayingCard card in cardsInHand)
        {
            if (card != null && card.gameObject != null)
            {
                // Make sure card is deselected first
                if (card.isSelected)
                {
                    card.SetSelectionState(false);
                }

                // Unregister event before destroying
                card.OnCardSelected -= HandleCardSelected;
                Destroy(card.gameObject);
            }
        }

        cardsInHand.Clear();

        // Also clear any BCI application objects
        if (bciManager != null && bciManager.ApplicationObjects != null)
        {
            bciManager.ApplicationObjects.Clear();
        }
    }

    // Get a card in hand by index
    public PlayingCard GetCardInHand(int index)
    {
        if (index >= 0 && index < cardsInHand.Count)
        {
            return cardsInHand[index];
        }
        return null;
    }

    // Get the list of cards in hand
    public List<PlayingCard> GetCardsInHand()
    {
        return new List<PlayingCard>(cardsInHand);
    }

    // Get the total value of cards in hand
    public int GetHandValue()
    {
        int totalValue = 0;

        foreach (PlayingCard card in cardsInHand)
        {
            totalValue += gameManager.GetCardValue(card);
        }

        return totalValue;
    }

    public PlayingCard GetSelectedCard()
    {
        return currentlySelectedCard;
    }
}