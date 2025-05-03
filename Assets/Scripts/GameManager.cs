using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Deck deck;
    public GameObject gameBoard;
    public Transform deckPosition;
    public float cardOffset = 0.05f;

    [Header("Card Settings")]
    public float cardScale = 0.7f;  // Control the scale of cards
    // Move the deck extremely far to the right and bottom corner
    public Vector3 deckCornerPosition = new Vector3(12.0f, -7.0f, 0f); 

    // Add reference to the Player
    public Player player;

    public Transform deckArea;

    private List<PlayingCard> cardsInPlay = new List<PlayingCard>();
    private List<PlayingCard> deckStack = new List<PlayingCard>(); // Keep track of visual deck stack

    void Start()
    {
        // First make sure all references are set
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

        // Initialize the game
        InitializeGame();

        // Now tell the player to display cards AFTER initialization
        player.DisplayCards();
    }

    public void InitializeGame()
    {
        // Clear any existing cards
        ClearTable();

        // Initialize the deck
        deck.InitializeDeck();

        // Create a visual representation of the deck on the table
        // WITHOUT removing cards from the actual deck
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

    public PlayingCard DrawCardToPosition(Vector3 position)
    {
        PlayingCard card = deck.DrawCard();
        if (card != null)
        {
            card.gameObject.SetActive(true);

            // Apply the cardScale to the drawn card
            card.transform.localScale = new Vector3(cardScale, cardScale, cardScale);

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
        if (deck.GetCardCount() <= 0 && deckStack.Count > 0)
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
}