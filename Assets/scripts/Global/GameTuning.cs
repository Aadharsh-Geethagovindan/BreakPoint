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
    public static GameTuning I { get; set; }

    [Header("Assign your GameTuningConfig asset here")]
    public GameTuningConfig config;

    [Header("If ON, we deep-copy defaults at runtiso Play Mode edits don't modify the asset")]
    [SerializeField] bool runtimeOnly = false;

    // Everyone reads/writes this at runtime
    public GameTuningData data { get;  set; }

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (config == null)
        {
            Debug.LogError("GameTuning: Please assign a GameTuningConfig asset.");
            config = ScriptableObject.CreateInstance<GameTuningConfig>();
        }

        // Deep copy the defaults into a runtime instance (so edits don't change the asset)
        if (runtimeOnly)
        {
            data = JsonUtility.FromJson<GameTuningData>(
                JsonUtility.ToJson(config.defaults)); // deep clone via JSON
        }
        else
        {
            // Direct reference: edits WILL persist to the asset in editor
            data = config.defaults;
        }
    }

    #if UNITY_EDITOR
    // Call this if you want to persist current runtime values back to the asset intentionally
    public void SaveRuntimeToAsset()
    {
        if (config == null || data == null) return;
        config.defaults = JsonUtility.FromJson<GameTuningData>(
            JsonUtility.ToJson(data)); // copy back
        UnityEditor.EditorUtility.SetDirty(config);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log("GameTuning: Saved runtime values back to asset defaults.");
    }
#endif
}