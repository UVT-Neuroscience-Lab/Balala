using UnityEngine;
using UnityEngine.EventSystems;

// This component handles card selection with multiple input methods
public class CardSelectionHandler : MonoBehaviour, IPointerClickHandler
{
    private Player playerRef;
    private PlayingCard cardRef;

    // Keep track of this card's index in the hand
    private int cardIndex = -1;

    // Debug flag
    public bool debugMode = true;

    public void Initialize(Player player, PlayingCard card, int index)
    {
        playerRef = player;
        cardRef = card;
        cardIndex = index;

        // Make sure we have a collider for mouse input
        EnsureCollider();

        if (debugMode) Debug.Log($"Card Selection Handler initialized for card {cardRef.rank} of {cardRef.suit}");
    }

    private void EnsureCollider()
    {
        // Make sure we have a proper sized collider
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            if (debugMode) Debug.Log("Added BoxCollider2D to card");
        }

        // Size the collider to match the card sprite
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        if (renderer != null && renderer.sprite != null)
        {
            // Make sure collider is in local space and proper size
            collider.size = renderer.sprite.bounds.size;

            // Make sure the collider is big enough to be easily clicked
            if (collider.size.x < 1.0f) collider.size = new Vector2(1.0f, collider.size.y);
            if (collider.size.y < 1.5f) collider.size = new Vector2(collider.size.x, 1.5f);
        }
        else
        {
            // Default size if no sprite is available
            collider.size = new Vector2(2f, 3f);
        }

        // Make sure box collider is set to trigger
        collider.isTrigger = true;
    }

    // Handle mouse/touch click on the card using the EventSystem interface
    public void OnPointerClick(PointerEventData eventData)
    {
        if (debugMode) Debug.Log($"Card clicked via PointerClick: {cardRef.rank} of {cardRef.suit}");
        ToggleSelection();
    }

    // Select via keyboard (called by Player class)
    public void SelectByKeyboard()
    {
        if (debugMode) Debug.Log($"Card selection triggered via keyboard: {cardRef.rank} of {cardRef.suit}");
        ToggleSelection();
    }

    private void ToggleSelection()
    {
        if (playerRef != null && cardRef != null)
        {
            playerRef.ToggleCardSelection(cardRef);
        }
        else
        {
            Debug.LogError("Card Selection Handler missing references!");
        }
    }

#if UNITY_EDITOR
    // Add visual debugging in the editor
    private void OnDrawGizmos()
    {
        // Draw a highlight around the card if it's in the editor
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, col.size);
        }
    }
#endif
}