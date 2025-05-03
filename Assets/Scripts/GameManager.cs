using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Sprite cardBack;
    public GameObject gameBoard; // Reference to the green square

    void Start()
    {
        Debug.Log("GameManager Start() running...");

        // Load all the sprites from the "card_faces" folder within Resources
        Sprite[] cardFaces = Resources.LoadAll<Sprite>("card_faces");

        if (cardFaces.Length == 0)
        {
            Debug.LogError("No card faces loaded from Resources/card_faces!");
            return;
        }

        // Get the world position of the bottom-right screen corner
        Vector3 bottomRightCorner = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, -Camera.main.transform.position.z));
        bottomRightCorner.z = 0;

        // Loop through each sprite (card face)
        for (int i = 0; i < cardFaces.Length; i++)
        {
            Sprite face = cardFaces[i];
            string[] nameParts = face.name.Split("_of_");

            if (nameParts.Length != 2)
            {
                Debug.LogWarning($"Invalid sprite name format: {face.name}");
                continue;
            }

            // Extract rank and suit from the name
            string rank = nameParts[0];
            string suit = nameParts[1];

            // Get the rank value (for Blackjack-style games, etc.)
            int rankValue = GetRankValue(rank);

            // Stack offset to position cards in a stack (adjust as needed)
            float stackOffset = i * 0.1f;
            Vector3 cardPos = bottomRightCorner + new Vector3(-stackOffset, stackOffset, 0);

            // Instantiate the card and get its PlayingCard script
            GameObject newCard = Instantiate(cardPrefab, cardPos, Quaternion.identity);
            PlayingCard cardScript = newCard.GetComponent<PlayingCard>();

            if (cardScript == null)
            {
                Debug.LogError("PlayingCard script not found on prefab!");
                continue;
            }

            // Set the card data using the card face and back, as well as the rank and suit
            cardScript.SetCardData(face, cardBack, rank, suit);
            cardScript.rankValue = rankValue;

            // Parent the card to the gameBoard to keep everything organized
            newCard.transform.SetParent(gameBoard.transform, true); // true keeps world position
        }
    }

    // Blackjack-style values: 2-10 = face value, J/Q/K = 10, A = 11
    private int GetRankValue(string rank)
    {
        return rank switch
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
    }
}