using UnityEngine;

[CreateAssetMenu(menuName = "Jokers/Flat Multiplier Joker")]
public class FlatMultiplierJoker : JokerEffect
{
    public int flatBonus = 4;

    public override int GetMultiplier(PlayingCard[] playedCards)
    {
        return flatBonus;
    }
}
