using UnityEngine;
using UnityEngine.UI;                
using UnityEngine.EventSystems;      

public class TuningOverlayToggle : MonoBehaviour
{
    [SerializeField] GameObject overlayRoot;   
    [SerializeField] CanvasGroup overlayCg;   
    [SerializeField] float fadeTime = 0.08f;   

    bool isOpen = false;                       
    Coroutine fadeCo;                          

    void Awake()                                
    {                                           
        if (overlayRoot != null) overlayRoot.SetActive(false); 
        if (overlayCg != null) overlayCg.alpha = 0f;           
    }                                           

    public void Toggle()                       
    {                                           
        if (isOpen) Close(); else Open();       
    }                                           

    public void Open()                          
    {                                           
        if (overlayRoot == null) return;        
        overlayRoot.SetActive(true);            
        isOpen = true;                          
        Fade(1f);                               
    }                                           

    public void Close()                         
    {                                           
        if (overlayRoot == null) return;        
        isOpen = false;                         
        Fade(0f, () => overlayRoot.SetActive(false)); 
    }                                           

    void Update()                               
    {                                           
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) Close(); 
    }                                           

    void Fade(float target, System.Action after = null) 
    {                                                   
        if (overlayCg == null) { after?.Invoke(); return; }      
        if (fadeCo != null) StopCoroutine(fadeCo);               
        fadeCo = StartCoroutine(FadeCo(target, after));          
    }                                                   

    System.Collections.IEnumerator FadeCo(float target, System.Action after) 
    {                                                   
        float t = 0f, start = overlayCg.alpha;          
        while (t < fadeTime) {                          
            t += Time.unscaledDeltaTime;                
            overlayCg.alpha = Mathf.Lerp(start, target, t / fadeTime); 
            yield return null;                          
        }                                               
        overlayCg.alpha = target;                       
        after?.Invoke();                                
    }                                                   
}
