using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public float HighScore;

    public void ResetToDefaults()
    {
        HighScore = 0;
    }
}
