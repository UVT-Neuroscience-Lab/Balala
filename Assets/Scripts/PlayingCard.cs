using UnityEngine;
using System.Collections;

public class PlayingCard : MonoBehaviour
{
    public Sprite frontSprite;
    public Sprite backSprite;
    public string rank;
    public string suit;
    public int rankValue;

    [Header("Selection")]
    public GameObject buttonOverlayPrefab;
    private GameObject buttonInstance;
    public bool isSelected = false;
    public System.Action<PlayingCard> OnCardSelected;

    [Header("Animation")]
    public float selectionMoveAmount = 0.3f;
    public float animationSpeed = 5f;
    private Vector3 basePosition; // ✅ CHANGED: no longer set in Awake
    private Vector3 targetPosition;
    private bool isAnimating = false;

    private SpriteRenderer spriteRenderer;
    private bool isFaceUp = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }

    void Start()
    {
        CreateButtonOverlay();
    }

    void Update()
    {
        if (isAnimating)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * animationSpeed);
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.01f)
            {
                transform.localPosition = targetPosition;
                isAnimating = false;
            }
        }
    }

    private void CreateButtonOverlay()
    {
        if (buttonOverlayPrefab != null)
        {
            buttonInstance = Instantiate(buttonOverlayPrefab, transform);
            buttonInstance.transform.localPosition = Vector3.zero;
            buttonInstance.transform.localScale = Vector3.one;
        }
        else
        {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1f, 1.4f);
            collider.isTrigger = true;

            CardClickHandler clickHandler = gameObject.AddComponent<CardClickHandler>();
            clickHandler.parentCard = this;
        }
    }

    public void SelectCard()
    {
        isSelected = !isSelected;
        targetPosition = basePosition + (isSelected ? Vector3.up * selectionMoveAmount : Vector3.zero);
        isAnimating = true;
        OnCardSelected?.Invoke(this);
    }

    public void SetSelectionState(bool selected)
    {
        if (isSelected != selected)
        {
            isSelected = selected;
            targetPosition = basePosition + (isSelected ? Vector3.up * selectionMoveAmount : Vector3.zero);
            isAnimating = true;
        }
    }

    public void InitializeBasePosition() // ✅ NEW: call this AFTER layout
    {
        basePosition = transform.localPosition;
        targetPosition = basePosition;
    }

    public void DestroyButton()
    {
        if (buttonInstance != null)
            Destroy(buttonInstance);
    }

    public void SetCardData(Sprite front, Sprite back, string rank, string suit)
    {
        frontSprite = front;
        backSprite = back;
        this.rank = rank;
        this.suit = suit;

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

        spriteRenderer.sprite = backSprite;
        isFaceUp = false;
    }

    public void ShowFront() { spriteRenderer.sprite = frontSprite; isFaceUp = true; }
    public void ShowBack() { spriteRenderer.sprite = backSprite; isFaceUp = false; }

    public void FlipCard()
    {
        if (isFaceUp) ShowBack();
        else ShowFront();
    }
}

public class CardClickHandler : MonoBehaviour
{
    public PlayingCard parentCard;

    private void OnMouseDown()
    {
        if (parentCard != null)
            parentCard.SelectCard();
    }
}