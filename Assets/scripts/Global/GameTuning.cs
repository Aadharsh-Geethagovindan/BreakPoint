using UnityEngine;

[System.Serializable]
public class GameTuningData
{
    public float hpScale = 1.0f;            // default 1.0
    public int burndownSteps = 3;

    public int burndownStartRound = 5;

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
        ApplyRuntimeDefaults();
    }  

    private void ApplyRuntimeDefaults() 
    {
        // Force these values every time unless overridden by UI
        if (data.burndownStartRound <= 0)
            data.burndownStartRound = 12;

        if (data.burndownSteps <= 0)
            data.burndownSteps = 5;

        if (data.burndownPercentSchedule == null || data.burndownPercentSchedule.Length != 6)
            data.burndownPercentSchedule = new float[6] { 0.05f, 0.10f, 0.20f, 0.20f, 0.30f, 0.40f };

        
        data.flipToMax = true;
    }                                                
}
