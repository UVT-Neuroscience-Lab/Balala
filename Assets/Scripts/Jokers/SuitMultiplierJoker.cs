using UnityEngine;

[CreateAssetMenu(menuName = "Jokers/Suit Multiplier Joker")]
public class SuitMultiplierJoker : JokerEffect
{
    public string targetSuit; // "Hearts", "Diamonds", etc.
    public int bonusPerCard = 3;

    public override int GetMultiplier(PlayingCard[] playedCards)
    {
        int count = 0;
        foreach (var card in playedCards)
        {
            if (card.suit == targetSuit)
                count++;
        }
        return count * bonusPerCard;
    }
}
