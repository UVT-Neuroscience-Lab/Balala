using UnityEngine;

public class PlayingCard : MonoBehaviour
{
    public Sprite frontSprite;
    public Sprite backSprite;
    public string rank;
    public string suit;
    public int rankValue;

    private SpriteRenderer spriteRenderer;
    private bool isFaceUp = false;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        // Store the original color when the card is created
        originalColor = spriteRenderer.color;
    }

    public void SetCardData(Sprite front, Sprite back, string rank, string suit)
    {
        frontSprite = front;
        backSprite = back;
        this.rank = rank;
        this.suit = suit;

        // Assign rankValue based on the card's rank
        rankValue = rank switch
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

        spriteRenderer.sprite = backSprite; // Start showing the back
        isFaceUp = false;
    }

    public void ShowFront()
    {
        spriteRenderer.sprite = frontSprite;
        isFaceUp = true;
    }

    public void ShowBack()
    {
        spriteRenderer.sprite = backSprite;
        isFaceUp = false;
    }

    public void FlipCard()
    {
        if (isFaceUp)
        {
            ShowBack();
        }
        else
        {
            ShowFront();
        }
    }

    // Balatro-style card animation
    public void AnimateCardDraw(Vector3 targetPosition, float duration = 0.5f)
    {
        StartCoroutine(MoveCardCoroutine(targetPosition, duration));
    }

    private System.Collections.IEnumerator MoveCardCoroutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
    }

    // ---- SELECTION METHODS ----

    // Method to highlight the card when selected
    public void HighlightCard(Color highlightColor)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }

        // Apply highlight color
        spriteRenderer.color = highlightColor;
    }

    // Method to reset card appearance when deselected
    public void ResetCardAppearance()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null) return;
        }

        // Restore original color
        spriteRenderer.color = originalColor;
    }

    // Optional: animate selection with a slight scale or rotation effect
    public void AnimateSelection(bool isSelected)
    {
        if (isSelected)
        {
            // You could add a small "pulse" or other animation here
            transform.localScale = new Vector3(1.05f, 1.05f, 1.05f);
        }
        else
        {
            // Reset to normal scale
            transform.localScale = Vector3.one;
        }
    }
}