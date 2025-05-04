using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform handArea;  // The area where cards are displayed

    [Header("Hand Settings")]
    public int handSize = 5;  // Fixed hand size
    public float cardSpacing = 2.2f;  // Horizontal spacing between cards

    private List<PlayingCard> cardsInHand = new List<PlayingCard>();
    private PlayingCard currentlySelectedCard = null;

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
    }

    // This will be called by the GameManager when it's ready
    public void DisplayCards()
    {
        // Clear any existing cards
        ClearHand();
        currentlySelectedCard = null;

        // Display exactly 5 cards
        for (int i = 0; i < handSize; i++)
        {
            AddCardToHand(i);
        }
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

            // Always show the card face up
            card.ShowFront();

            // Register for selection events
            card.OnCardSelected += HandleCardSelected;
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

            // Example: Notify GameManager with all selected cards
            if (selectedCards.Count > 0)
            {
                gameManager.OnMultipleCardsSelected(selectedCards);
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
        foreach (PlayingCard card in cardsInHand)
        {
            if (card != null && card.gameObject != null)
            {
                // Unregister event before destroying
                card.OnCardSelected -= HandleCardSelected;
                Destroy(card.gameObject);
            }
        }

        cardsInHand.Clear();
        currentlySelectedCard = null;
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

    // Get the currently selected card
    public PlayingCard GetSelectedCard()
    {
        return currentlySelectedCard;
    }
}