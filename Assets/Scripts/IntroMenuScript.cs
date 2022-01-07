using System.Collections;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Experimental.SpatialAwareness;
using UnityEngine;
using TMPro;

public class IntroMenuScript : MonoBehaviour
{
    public TextMeshPro WelcomeText;
    public TextMeshPro UnlockText;
    public GameObject NextMenu;
    public float fadeTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        DisableSceneObserver();

        WelcomeText.alpha = 0.0f;
        UnlockText.alpha = 0.0f;
        startFadeIn(0, 2.0f);
        startFadeIn(1, 3.0f);
        startConnectToPhone();
    }

    IEnumerator ConnectToPhone()
    {
        yield return new WaitForSeconds(15);
        StartCoroutine("FadeOut");
    }

    IEnumerator FadeIn(object[] parms)
    {
        int whichText = (int)parms[0];
        float waitLength = (float)parms[1];
        yield return new WaitForSeconds(waitLength);
        
        for (float f = 0.05f; f <= 1; f += 0.05f)
        {
            if(whichText == 0) // Welcome text
            {
                WelcomeText.alpha = f;
            }else if(whichText == 1) // Unlock text
            {
                UnlockText.alpha = f;
            }
            
            yield return new WaitForSeconds(0.05f * fadeTime);
        }
    }

    IEnumerator FadeOut()
    {
        for (float f = 0.95f; f > 0; f -= 0.05f)
        {
            WelcomeText.alpha = f;
            UnlockText.alpha = f;
            yield return new WaitForSeconds(0.05f * fadeTime);
        }
        this.gameObject.SetActive(false);
        NextMenu.SetActive(true);
    }

    public void startFadeIn(int whichText, float length)
    {
        object[] parms = new object[2] { whichText, length };
        StartCoroutine("FadeIn", parms);
    }

    public void startFadeOutAndDeactivate() 
    {
        StartCoroutine("FadeOut");
    }

    public void startConnectToPhone()
    {
        StartCoroutine("ConnectToPhone");

    }

    private void DisableSceneObserver()
    {
        var observer = CoreServices.GetSpatialAwarenessSystemDataProvider<IMixedRealitySceneUnderstandingObserver>();

        if (observer != null)
        {
            observer.RequestMeshData = false;
            observer.RequestPlaneData = false;
            observer.RequestOcclusionMask = false;
        }
    }

    public void AddServerIPToUnlockText(string serverIP, int serverPort)
    {
        UnlockText.text += "\n(" + serverIP + ":" + serverPort + ")";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
