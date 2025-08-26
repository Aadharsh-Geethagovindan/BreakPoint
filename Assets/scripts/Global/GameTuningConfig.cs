using UnityEngine;


[CreateAssetMenu(menuName = "BP/Game Tuning Config", fileName = "GameTuningConfig")]
public class GameTuningConfig : ScriptableObject
{
    public GameTuningData defaults = new GameTuningData();
}