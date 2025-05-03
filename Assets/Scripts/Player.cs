using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem; // For the new Input System
using UnityEngine.EventSystems;

public class Player : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform handArea;  // The area where cards are displayed

    [Header("Hand Settings")]
    public int handSize = 8;  // Fixed hand size
    public float cardSpacing = 2.2f;  // Horizontal spacing between cards
    private List<PlayingCard> cardsInHand = new List<PlayingCard>();

    [Header("Selection Settings")]
    public int maxSelectedCards = 5;  // Maximum number of cards that can be selected
    public Color selectionHighlightColor = new Color(1f, 0.8f, 0.2f, 1f);  // Yellow-ish highlight color
    public float selectionYOffset = 0.5f;  // How much selected cards move up
    private List<PlayingCard> selectedCards = new List<PlayingCard>();

    [Header("Debug Settings")]
    public bool debugMode = true;  // Enable debug logging

    [Header("Visual Settings")]
    public Vector3 handPosition = new Vector3(-2.0f, -3.5f, 0f);

    // Array to store selection handlers for keyboard input support
    private CardSelectionHandler[] cardHandlers;

    // Input variables for the new Input System
    private Keyboard keyboard;

    void Awake()
    {
        // Get the keyboard for the new Input System
        keyboard = Keyboard.current;
    }

    void Start()
    {
        // Validate we have all necessary references
        if (gameManager == null)
        {
            Debug.LogError("Player is missing GameManager reference!");
            return;
        }

        // Display cards - moved here for automatic startup
        DisplayCards();
    }

    void Update()
    {
        // Skip input handling if keyboard isn't available
        if (keyboard == null)
        {
            keyboard = Keyboard.current;
            if (keyboard == null) return;
        }

        // Handle numeric key input for card selection (1-8)
        for (int i = 0; i < handSize; i++)
        {
            // Check if the corresponding number key was just pressed
            Key numKey = Key.Digit1 + i;
            if (i < 9 && keyboard[numKey].wasPressedThisFrame)
            {
                // Convert key press (1-8) to array index (0-7)
                int cardIndex = i;
                if (cardIndex < cardsInHand.Count && cardHandlers != null && cardIndex < cardHandlers.Length)
                {
                    if (cardHandlers[cardIndex] != null)
                    {
                        cardHandlers[cardIndex].SelectByKeyboard();
                        if (debugMode) Debug.Log($"Key {i + 1} pressed, selecting card {i}");
                    }
                }
            }
        }

        // Press 'C' to clear all selections
        if (keyboard[Key.C].wasPressedThisFrame)
        {
            ClearCardSelections();
            if (debugMode) Debug.Log("Cleared all card selections");
        }
    }

    // This will be called by the GameManager when it's ready
    public void DisplayCards()
    {
        if (debugMode) Debug.Log("Displaying cards in hand");

        // Clear any existing cards
        ClearHand();

        // Initialize handlers array
        cardHandlers = new CardSelectionHandler[handSize];

        // Display cards
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
            if (debugMode) Debug.Log($"Adding card to hand: {card.rank} of {card.suit}");

            // Add to our hand list
            cardsInHand.Add(card);

            // Always show the card face up
            card.ShowFront();

            // Parent the card to the handArea if available
            if (handArea != null)
            {
                card.transform.SetParent(handArea, true);
            }

            // Add card selection component with proper initialization
            CardSelectionHandler selectionHandler = card.gameObject.AddComponent<CardSelectionHandler>();
            selectionHandler.Initialize(this, card, cardIndex);
            selectionHandler.debugMode = this.debugMode;

            // Store the handler for keyboard access
            cardHandlers[cardIndex] = selectionHandler;
        }
        else
        {
            Debug.LogError($"Failed to draw card at index {cardIndex}");
        }
    }

    // Calculate the position for a card in the hand
    private Vector3 CalculateCardPosition(int cardIndex)
    {
        Vector3 basePosition = handArea != null ? handArea.position : handPosition;

        // Layout the cards in a row with specified spacing
        float xOffset = cardIndex * cardSpacing - ((handSize - 1) * cardSpacing / 2.0f);

        return new Vector3(
            basePosition.x + xOffset,
            basePosition.y,
            basePosition.z - (cardIndex * 0.01f)  // Small z offset to prevent z-fighting
        );
    }

    // Remove all cards from hand
    public void ClearHand()
    {
        if (debugMode) Debug.Log("Clearing hand");

        // Clear selections first
        ClearCardSelections();

        foreach (PlayingCard card in cardsInHand)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }

        cardsInHand.Clear();
        cardHandlers = null;
    }

    // Toggles card selection status
    public void ToggleCardSelection(PlayingCard card)
    {
        if (card == null) return;

        if (selectedCards.Contains(card))
        {
            // Deselect the card
            if (debugMode) Debug.Log($"Deselecting card: {card.rank} of {card.suit}");

            selectedCards.Remove(card);

            // Reset position
            Vector3 cardPos = card.transform.position;
            card.transform.position = new Vector3(cardPos.x, cardPos.y - selectionYOffset, cardPos.z);

            // Reset appearance using direct sprite renderer manipulation
            SpriteRenderer renderer = card.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = Color.white; // Reset to default color
            }

            // Reset scale
            card.transform.localScale = Vector3.one;
        }
        else if (selectedCards.Count < maxSelectedCards)
        {
            // Select the card
            if (debugMode) Debug.Log($"Selecting card: {card.rank} of {card.suit}");

            selectedCards.Add(card);

            // Adjust position and appearance to show selection
            Vector3 cardPos = card.transform.position;
            card.transform.position = new Vector3(cardPos.x, cardPos.y + selectionYOffset, cardPos.z);

            // Use direct color manipulation
            SpriteRenderer renderer = card.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.color = selectionHighlightColor;
            }

            // Add scale animation if desired
            card.transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }
        else
        {
            // Maximum selections reached - provide feedback
            Debug.LogWarning($"Maximum of {maxSelectedCards} cards can be selected");
            // You could add a UI notification or sound effect here
        }
    }

    // Clear all card selections
    public void ClearCardSelections()
    {
        if (debugMode && selectedCards.Count > 0)
            Debug.Log("Clearing all card selections");

        foreach (PlayingCard card in selectedCards)
        {
            if (card != null && card.gameObject != null)
            {
                // Reset position
                Vector3 cardPos = card.transform.position;
                card.transform.position = new Vector3(cardPos.x, cardPos.y - selectionYOffset, cardPos.z);

                // Reset appearance using direct sprite renderer manipulation
                SpriteRenderer renderer = card.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.white; // Reset to default color
                }

                // Reset scale
                card.transform.localScale = Vector3.one;
            }
        }

        selectedCards.Clear();
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

    // Get the list of selected cards
    public List<PlayingCard> GetSelectedCards()
    {
        return new List<PlayingCard>(selectedCards);
    }

    // Get number of selected cards
    public int GetSelectedCardCount()
    {
        return selectedCards.Count;
    }

    // Check if a card is selected
    public bool IsCardSelected(PlayingCard card)
    {
        return selectedCards.Contains(card);
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

    // Get the total value of selected cards
    public int GetSelectedCardsValue()
    {
        int totalValue = 0;
        foreach (PlayingCard card in selectedCards)
        {
            totalValue += gameManager.GetCardValue(card);
        }
        return totalValue;
    }

    // Called by UI buttons if you add them
    public void OnConfirmSelection()
    {
        if (debugMode)
        {
            Debug.Log($"Selection confirmed with {selectedCards.Count} cards");
            foreach (PlayingCard card in selectedCards)
            {
                Debug.Log($"Selected: {card.rank} of {card.suit}");
            }
        }

        // TODO: Add your code here to process the selected cards
        // For example, play the selected cards, submit them for scoring, etc.
    }
}