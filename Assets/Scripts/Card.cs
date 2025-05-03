using UnityEngine;

public class Card : MonoBehaviour
{
    public Sprite frontSprite;
    public Sprite backSprite;

    private SpriteRenderer sr;
    private bool isFaceUp = false;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void ShowFront()
    {
        sr.sprite = frontSprite;
        isFaceUp = true;
    }

    public void ShowBack()
    {
        sr.sprite = backSprite;
        isFaceUp = false;
    }

    public void Flip()
    {
        if (isFaceUp) ShowBack();
        else ShowFront();
    }
}