using UnityEngine;

[DefaultExecutionOrder(-1000)] // run *very* early
public class RevampGate : MonoBehaviour
{
    [SerializeField] private GameObject[] classicRoots;   // assign in Inspector
    [SerializeField] private GameObject[] revampedRoots;  // assign in Inspector

    void Awake()
    {
        bool r = GameModeService.IsRevamped;              // already added by you
        foreach (var go in classicRoots)   if (go) go.SetActive(!r);  // NEW
        foreach (var go in revampedRoots)  if (go) go.SetActive(r);   // NEW
    }
}
