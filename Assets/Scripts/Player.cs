using UnityEngine;
using System.Collections.Generic;
using Gtec.UnityInterface;

public class Player : MonoBehaviour
{
    [Header("References")]
    public GameManager gameManager;
    public Transform handArea;  // The area where cards are displayed

    [Header("Hand Settings")]
    public int handSize = 5;  // Fixed hand size
    public float cardSpacing = 2.2f;  // Increased horizontal spacing between cards

    private List<PlayingCard> cardsInHand = new List<PlayingCard>();

    [Header("Visual Settings")]
    public Vector3 handPosition = new Vector3(-2.0f, -3.5f, 0f);

    public Sprite darkSprite;
    public Sprite flashSprite;

    private ERPFlashController2D bciManager;

    void Start()
    {
        bciManager = GameObject.FindFirstObjectByType<ERPFlashController2D>();
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
            // Add to our hand list
            cardsInHand.Add(card);

            ERPFlashObject2D objBci = bciManager.TrainingObject;

            objBci.ClassId = cardsInHand.IndexOf(card) + 1;
            objBci.GameObject = card.gameObject;
            objBci.DarkSprite = darkSprite;
            objBci.FlashSprite = flashSprite;
            objBci.Rotate = false;

            bciManager.ApplicationObjects.Add(objBci);

            // Always show the card face up
            card.ShowFront();

            // Parent the card to the handArea if available
            if (handArea != null)
            {
                card.transform.SetParent(handArea, true);
            }
        }
    }

    // Calculate the position for a card in the hand
    // Adjust calculation of card positions to spread them out more
    private Vector3 CalculateCardPosition(int cardIndex)
    {
        Vector3 basePosition = handArea != null ? handArea.position : handPosition;

        // Layout the cards in a row with specified spacing
        // Now with cards spreading from right to left (hence the negative cardIndex)
        float xOffset = cardIndex * cardSpacing - ((handSize - 1) * cardSpacing);

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
                Destroy(card.gameObject);
            }
        }

        cardsInHand.Clear();
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
}