using UnityEngine;
using System.Collections.Generic;
using Gtec.UnityInterface;

public class Deck : MonoBehaviour
{
    public GameObject cardPrefab;
    private Sprite cardBackSprite; // Will be loaded from Resources
    private List<PlayingCard> deck = new List<PlayingCard>(); // List of all cards in the deck
    void Awake()
    {
        // Load the card back sprite
        cardBackSprite = Resources.Load<Sprite>("Deck Backs5");
        if (cardBackSprite == null)
        {
            Debug.LogError("Failed to load card back sprite from 'Resources/Deck Backs5'. Check the path.");
        }
    }

    // Initialize the deck with cards
    public void InitializeDeck()
    {
        // Check if card back sprite was loaded
        if (cardBackSprite == null)
        {
            // Try loading it again if it failed in Awake
            cardBackSprite = Resources.Load<Sprite>("Deck Backs5");
            if (cardBackSprite == null)
            {
                Debug.LogError("Card back sprite still not loaded. Cards may not display properly.");
            }
        }

        // Clear any existing cards
        foreach (PlayingCard card in deck)
        {
            if (card != null && card.gameObject != null)
            {
                Destroy(card.gameObject);
            }
        }
        deck.Clear();

        // Use exact capitalization to match your file naming convention
        string[] suits = { "Hearts", "Clubs", "Diamonds", "Spades" };
        string[] ranks = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "Jack", "Queen", "King", "Ace" };

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                GameObject cardObject = Instantiate(cardPrefab, transform);  // Create the card prefab
                PlayingCard card = cardObject.GetComponent<PlayingCard>(); // Get the PlayingCard component


                if (card == null)
                {
                    Debug.LogError("PlayingCard component not found on card prefab!");
                    continue;
                }

                // Load the front texture using the path you specified
                Sprite frontSprite = Resources.Load<Sprite>($"card_faces/{rank}_of_{suit}");

                if (frontSprite == null)
                {
                    Debug.LogError($"Could not load sprite for {rank} of {suit} at path 'Resources/card_faces/{rank}_of_{suit}'. Check your Resources folder path.");
                    continue;
                }

                // Set card data
                card.SetCardData(frontSprite, cardBackSprite, rank, suit);

                deck.Add(card);  // Add the card to the deck

                cardObject.SetActive(false);  // Set the card inactive initially
            }
        }

        ShuffleDeck();  // Shuffle the deck after initializing it
    }

    // Shuffle the deck
    public void ShuffleDeck()
    {
        for (int i = 0; i < deck.Count; i++)
        {
            PlayingCard temp = deck[i];
            int randomIndex = Random.Range(i, deck.Count);
            deck[i] = deck[randomIndex];
            deck[randomIndex] = temp;
        }
    }

    // Draw a card from the deck (removes it from the deck and returns it)
    public PlayingCard DrawCard()
    {
        if (deck.Count > 0)
        {
            PlayingCard card = deck[0];
            deck.RemoveAt(0);  // Remove the card from the deck
            card.gameObject.SetActive(true);  // Make sure the card is active
            return card;
        }
        else
        {
            Debug.LogWarning("Deck is empty!");
            return null;  // Return null if no cards are left in the deck
        }
    }

    // Create a visual card for display purposes without removing from the deck
    public PlayingCard CreateDeckVisualCard()
    {
        GameObject cardObject = Instantiate(cardPrefab, transform);
        PlayingCard visualCard = cardObject.GetComponent<PlayingCard>();

        if (visualCard != null)
        {
            // Load sprites needed for a visual card
            Sprite backSprite = Resources.Load<Sprite>("Deck Backs5");
            if (backSprite == null)
            {
                Debug.LogError("Could not load back sprite for deck visual");
                Destroy(cardObject);
                return null;
            }

            // Use a sample front sprite (doesn't matter which one as we'll show the back)
            Sprite frontSprite = Resources.Load<Sprite>("card_faces/2_of_Clubs");
            if (frontSprite == null)
            {
                Debug.LogError("Could not load sample front sprite for deck visual");
                frontSprite = backSprite; // Use back as fallback
            }

            // Configure the card
            visualCard.SetCardData(frontSprite, backSprite, "", "");
            cardObject.SetActive(true);
            return visualCard;
        }

        return null;
    }

    // Draw all cards from the deck (useful for shuffling the deck into place in the GameManager)
    public List<PlayingCard> DrawAllCards()
    {
        List<PlayingCard> allCards = new List<PlayingCard>(deck);
        deck.Clear();
        return allCards;
    }

    // Get the current number of cards in the deck
    public int GetCardCount()
    {
        return deck.Count;
    }

    // Add a card back to the deck (at the bottom)
    public void ReturnCardToDeck(PlayingCard card)
    {
        if (card != null)
        {
            deck.Add(card);
            card.ShowBack();
            card.gameObject.SetActive(false);
        }
    }
}