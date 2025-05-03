using UnityEngine;

public class Level
{
    public int LevelNumber { get; private set; }
    public int ScoreToBeat { get; private set; }

    public Level(int number, int score)
    {
        LevelNumber = number;
        ScoreToBeat = score;
    }

    public void AdvanceLevel()
    {
        LevelNumber++;
        ScoreToBeat += 10;
    }
}
