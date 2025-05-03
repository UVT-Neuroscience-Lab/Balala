using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite cardBack;
    public GameObject gameBoard; // Reference to the green square
    private Vector2 boardSize = new Vector2(13f, 4f); // Width x Height in card count

    void Start()
    {
        Debug.Log("GameManager Start() running...");
        Sprite[] cardFaces = Resources.LoadAll<Sprite>("card_faces");

        // Find the bottom-right corner of the screen in world space
        Vector3 bottomRightCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, -1));  // 10 is Z to get it in front of the camera
        bottomRightCorner.z = 0;  // Ensure Z is 0 for proper 2D stacking

        // Stack the cards in the bottom-right corner
        for (int i = 0; i < cardFaces.Length; i++)
        {
            // Position each card slightly above the previous one
            float stackOffset = i * 0.1f; // Stack offset (adjust as needed for spacing)

            // Calculate the position for each card in the stack
            Vector3 cardPos = bottomRightCorner + new Vector3(-stackOffset, stackOffset, 0);

            // Instantiate the card
            GameObject newCard = Instantiate(cardPrefab, cardPos, Quaternion.identity);

            // Set the card face
            Card cardScript = newCard.GetComponent<Card>();
            cardScript.frontSprite = cardFaces[i];
            cardScript.backSprite = cardBack;
            cardScript.ShowBack();

            // Parent the card to the gameBoard (optional)
            newCard.transform.SetParent(gameBoard.transform);
        }
    }
}