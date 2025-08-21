using UnityEngine;

[System.Serializable]
public class GameTuningData
{
    public float hpScale = 1.0f;            // default 1.0
    public int burndownSteps = 2;

    public int burndownStartRound = 1;

    public bool flipToMax = true;
    
    // Six phase percentages
    public float[] burndownPercentSchedule = new float[6] { 0.05f, 0.10f, 0.20f, 0.20f, 0.30f, 0.40f }; 
}

public class GameTuning : MonoBehaviour
{
    public static GameTuning I { get; private set; }  
    public GameTuningData data = new GameTuningData(); 

    void Awake()                                       
    {                                                  
        if (I != null && I != this) { Destroy(gameObject); return; }  
        I = this;                                       
        DontDestroyOnLoad(gameObject);                  
    }                                                  
}
