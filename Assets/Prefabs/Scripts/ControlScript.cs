using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlScript : MonoBehaviour
{
    public float scrollSpeed = 0.02f;
    bool upIsPressed = false;
    bool downIsPressed = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (upIsPressed)
        {
            ScrollUp();
        }
        if (downIsPressed)
        {
            ScrollDown();
        }
    }

    public void setUpIsPressed(bool val)
    {
        upIsPressed = val;
    }

    public void setDownIsPressed(bool val)
    {
        downIsPressed = val;
    }
    void ScrollUp()
    {
        this.gameObject.transform.localPosition += Vector3.up * scrollSpeed;
    }

    public void GoToTop()
    {
        this.gameObject.transform.localPosition = Vector3.zero;
    }

    void ScrollDown()
    {
        if (this.gameObject.transform.localPosition.y > scrollSpeed)
        {
            this.gameObject.transform.localPosition -= Vector3.up * scrollSpeed;
        }
    }

    public void ClearBoard()
    {
        this.gameObject.GetComponent<TextMeshPro>().text = "";
    }

    public void LogText(string text)
    {
        this.gameObject.GetComponent<TextMeshPro>().text = text + "\n" + this.gameObject.GetComponent<TextMeshPro>().text;
    }
}
