using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Deck deck;
    public GameObject gameBoard;
    public Transform deckPosition;
    public float cardOffset = 0.05f;

    [Header("Card Settings")]
    public float cardScale = 0.7f;  // Control the scale of cards
    public Vector3 deckCornerPosition = new Vector3(12.0f, -7.0f, 0f);

    [Header("Card Selection")]
    public GameObject cardButtonOverlayPrefab;

    [Header("UI Elements")]
    public Text selectedValueText;
    public Text handValueText;

    [Header("Level Settings")]
    public int currentLevel = 1;
    public int scoreToBeat = 50;
    public int currentScore = 0;
    public Text levelText;
    public Text scoreToBeatText;
    public Text currentScoreText;
    public Button playHandButton;

    public Player player;

    public Transform deckArea;

    private List<PlayingCard> cardsInPlay = new List<PlayingCard>();
    private List<PlayingCard> deckStack = new List<PlayingCard>(); // Keep track of visual deck stack

    void Start()
    {
        if (deck == null)
        {
            Debug.LogError("Deck reference is missing!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        InitializeGame();

        player.DisplayCards();

        UpdateUIValues();
        UpdateLevelUI();

        if (playHandButton != null)
        {
            playHandButton.onClick.AddListener(PlayHand);
            playHandButton.interactable = false; // Disabled until cards are selected
        }
        else
        {
            Debug.LogWarning("Play Hand Button reference is missing!");
        }
    }

    private void UpdateUIValues()
    {
        // Update total hand value if we have that UI element
        if (handValueText != null)
        {
            int handValue = player.GetHandValue();
            handValueText.text = $"Hand Value: {handValue}";
        }

        // Update selected card value
        if (selectedValueText != null)
        {
            // For single selection mode
            if (!player.allowMultipleSelection)
            {
                PlayingCard selectedCard = player.GetSelectedCard();
                if (selectedCard != null)
                {
                    int cardValue = GetCardValue(selectedCard);
                    selectedValueText.text = $"Selected: {cardValue}";
                }
                else
                {
                    selectedValueText.text = "Selected: None";
                }
            }
            // For multiple selection mode
            else
            {
                int selectedValue = player.GetSelectedCardsValue();
                List<PlayingCard> selectedCards = player.GetSelectedCards();
                selectedValueText.text = $"Selected: {selectedValue} ({selectedCards.Count} cards)";
            }
        }

        // Update play button state based on selection
        UpdatePlayButtonState();
    }

    // New method to update play button state
    private void UpdatePlayButtonState()
    {
        if (playHandButton != null)
        {
            // Enable button only if cards are selected
            if (!player.allowMultipleSelection)
            {
                // Single card selection mode
                playHandButton.interactable = player.GetSelectedCard() != null;
            }
            else
            {
                // Multiple card selection mode
                playHandButton.interactable = player.GetSelectedCards().Count > 0;
            }
        }
    }

    public void InitializeGame()
    {
        // Clear any existing cards
        ClearTable();

        // Initialize the deck
        deck.InitializeDeck();

        // Create a visual representation of the deck on the table
        CreateDeckVisual();
    }

    private void ClearTable()
    {
        // Deactivate all cards currently in play
        foreach (PlayingCard card in cardsInPlay)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        cardsInPlay.Clear();

        // Clear deck stack
        foreach (PlayingCard card in deckStack)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        deckStack.Clear();
    }

    private void CreateDeckVisual()
    {
        // Use the deckArea Transform if available, otherwise use a default position
        Vector3 spawnPosition;
        if (deckArea != null)
        {
            spawnPosition = deckArea.position;
        }
        else
        {
            Debug.LogWarning("DeckArea reference is not set! Using a default position.");
            spawnPosition = new Vector3(8.0f, -5.0f, 0f); // Fallback position
        }

        // Create a small stack of cards to represent the deck (just a few for visual effect)
        int visualDeckSize = Mathf.Min(5, deck.GetCardCount()); // Show max 5 cards in visual stack

        for (int i = 0; i < visualDeckSize; i++)
        {
            // Use the deck to create a proper card
            PlayingCard card = deck.CreateDeckVisualCard();

            if (card != null)
            {
                // Position with slight offset for stack effect
                card.transform.position = new Vector3(
                    spawnPosition.x + (i * 0.02f),
                    spawnPosition.y + (i * 0.02f),
                    spawnPosition.z - (i * 0.001f)); // Small offsets for visual stacking

                // Apply scale to control size
                card.transform.localScale = new Vector3(cardScale, cardScale, cardScale);

                // Ensure card stays showing back
                card.ShowBack();
                deckStack.Add(card);
            }
        }
    }

    public void PlayHand()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is missing in PlayHand!");
            return;
        }

        // Store selected cards value before clearing
        int scoreToAdd = 0;

        if (!player.allowMultipleSelection)
        {
            // Single card selection mode
            PlayingCard selectedCard = player.GetSelectedCard();
            if (selectedCard != null)
            {
                scoreToAdd = GetCardValue(selectedCard);
            }
        }
        else
        {
            // Multiple card selection mode
            scoreToAdd = player.GetSelectedCardsValue();
        }

        // Add score only if there were selected cards
        if (scoreToAdd > 0)
        {
            currentScore += scoreToAdd;

            // Clear everything
            DiscardSelectedCards();

            // Complete reset of Player's selection state
            player.ResetSelectionState();

            // Update UI
            UpdateLevelUI();
            CheckLevelCompletion();

            // Draw new hand only after everything is reset
            player.DisplayCards();
            UpdateUIValues();

            // Disable play button until new selection is made
            if (playHandButton != null)
            {
                playHandButton.interactable = false;
            }
        }
    }

    private void DrawNewHand()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is null in DrawNewHand!");
            return;
        }

        // Make sure any selection state is completely reset before drawing a new hand
        if (player.GetSelectedCard() != null)
        {
            Debug.Log("Resetting selection state before drawing new hand");
            player.ResetSelectionState();
        }

        player.DisplayCards();

        // Update UI to reflect new hand
        UpdateUIValues();

        // Null check before accessing playHandButton
        if (playHandButton != null)
        {
            playHandButton.interactable = false;
        }
    }

    private void CheckLevelCompletion()
    {
        if (currentScore >= scoreToBeat)
        {
            // Level completed!
            Debug.Log($"Level {currentLevel} completed!");
        }
    }

    private void UpdateLevelUI()
    {
        if (levelText != null) levelText.text = $"Level: {currentLevel}";
        if (scoreToBeatText != null) scoreToBeatText.text = $"Target: {scoreToBeat}";
        if (currentScoreText != null) currentScoreText.text = $"Score: {currentScore}";
    }

    public PlayingCard DrawCardToPosition(Vector3 position)
    {
        if (deck == null)
        {
            Debug.LogError("Deck reference is null in DrawCardToPosition!");
            return null;
        }

        PlayingCard card = deck.DrawCard();
        if (card != null)
        {
            card.gameObject.SetActive(true);

            // Apply the cardScale to the drawn card
            card.transform.localScale = new Vector3(cardScale, cardScale, cardScale);

            // Assign the button overlay prefab
            card.buttonOverlayPrefab = cardButtonOverlayPrefab;

            card.transform.position = position;
            card.ShowFront(); // Show the front of the card
            cardsInPlay.Add(card);

            // Update visual deck stack if cards are getting low
            UpdateDeckVisual();
        }
        return card;
    }

    // Update the visual appearance of the deck stack
    private void UpdateDeckVisual()
    {
        // Hide the deck visual completely if the deck is empty
        if (deck != null && deck.GetCardCount() <= 0 && deckStack.Count > 0)
        {
            foreach (PlayingCard card in deckStack)
            {
                if (card != null && card.gameObject != null)
                {
                    Destroy(card.gameObject);
                }
            }
            deckStack.Clear();
        }
    }

    // Handle card values like in Balatro (needed for gameplay)
    public int GetCardValue(PlayingCard card)
    {
        if (card == null)
        {
            Debug.LogError("Attempting to get value of null card!");
            return 0;
        }

        // Assign rankValue based on the card's rank
        int value = card.rank switch
        {
            "2" => 2,
            "3" => 3,
            "4" => 4,
            "5" => 5,
            "6" => 6,
            "7" => 7,
            "8" => 8,
            "9" => 9,
            "10" => 10,
            "Jack" => 10,
            "Queen" => 10,
            "King" => 10,
            "Ace" => 11,
            _ => 0
        };

        return value;
    }

    // Called when a card is selected
    public void OnCardSelected(PlayingCard selectedCard)
    {
        if (selectedCard == null)
        {
            Debug.LogError("OnCardSelected called with null card!");
            return;
        }

        Debug.Log($"GameManager received selection: {selectedCard.rank} of {selectedCard.suit}");
        Debug.Log($"Card value: {GetCardValue(selectedCard)}, isSelected: {selectedCard.isSelected}");

        // Update UI to show current values
        UpdateUIValues();
    }

    // For multiple card selection mode
    public void OnMultipleCardsSelected(List<PlayingCard> selectedCards)
    {
        Debug.Log($"GameManager received multiple card selection. Count: {selectedCards.Count}");

        // Calculate total value of selected cards
        int totalValue = 0;
        foreach (PlayingCard card in selectedCards)
        {
            totalValue += GetCardValue(card);
        }

        Debug.Log($"Total value of selected cards: {totalValue}");

        // Update UI to show current values
        UpdateUIValues();
    }

    private void DiscardSelectedCards()
    {
        if (player == null)
        {
            Debug.LogError("Player reference is null in DiscardSelectedCards!");
            return;
        }

        // Get a copy of the list to avoid modification issues during iteration
        List<PlayingCard> cardsToDiscard = new List<PlayingCard>(player.GetSelectedCards());

        Debug.Log($"Discarding {cardsToDiscard.Count} selected cards");

        foreach (PlayingCard card in cardsToDiscard)
        {
            if (card != null)
            {
                // Make sure card is visually deselected first
                card.SetSelectionState(false);

                // Remove from play tracking
                cardsInPlay.Remove(card);

                // Destroy the GameObject
                Destroy(card.gameObject);
            }
        }
    }
}