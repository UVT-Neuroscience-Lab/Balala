using UnityEngine;

public abstract class JokerEffect : ScriptableObject
{
    public string jokerName;
    public Sprite jokerArt;

    public abstract int GetMultiplier(PlayingCard[] playedCards);
}
