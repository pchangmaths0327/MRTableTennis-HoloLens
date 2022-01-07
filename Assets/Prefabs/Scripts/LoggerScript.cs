using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class LoggerScript : MonoBehaviour
{
    public ControlScript textContent;
    // Start is called before the first frame update
    void Start()
    {
    }

    public void log(string text)
    {
        textContent.LogText(text);
    }

    public void toggleVisible()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
