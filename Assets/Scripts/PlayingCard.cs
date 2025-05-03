using UnityEngine;

public class PlayingCard : MonoBehaviour
{
    public Sprite frontSprite;
    public Sprite backSprite;
    public string rank;
    public string suit;
    public int rankValue;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetCardData(Sprite front, Sprite back, string rank, string suit)
    {
        frontSprite = front;
        backSprite = back;
        this.rank = rank;
        this.suit = suit;

        spriteRenderer.sprite = backSprite; // Start showing the back
    }

    public void ShowFront()
    {
        spriteRenderer.sprite = frontSprite;
    }

    public void ShowBack()
    {
        spriteRenderer.sprite = backSprite;
    }
}