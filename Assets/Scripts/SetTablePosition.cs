using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SetTablePosition : MonoBehaviour
{
    public GameObject oldTable;
    public GameObject table;
    public GameObject scoreboard;
    public TextMeshPro readyToPlayText;
    private float fadeTime = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        table.transform.localScale = oldTable.transform.localScale;
        table.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
        this.gameObject.transform.localPosition = oldTable.transform.localPosition;
        this.gameObject.transform.localRotation = oldTable.transform.localRotation;

        if (table.transform.localScale.x > table.transform.localScale.y)
        {
            this.gameObject.transform.Rotate(0, 0, 90);
            table.transform.localScale = new Vector3(table.transform.localScale.y, table.transform.localScale.x, 1);
        }

        startFadeOutAndDeactivate();
    }


    IEnumerator FadeOut()
    {
        for (float f = 0.95f; f > 0; f -= 0.05f)
        {
            readyToPlayText.alpha = f;
            yield return new WaitForSeconds(0.05f * fadeTime);
        }
        readyToPlayText.alpha = 0;
        //this.gameObject.SetActive(false);
    }

    public void startFadeOutAndDeactivate()
    {
        StartCoroutine("FadeOut");
    }

    public void flipTable()
    {
        this.gameObject.transform.Rotate(0.0f, 0.0f, 180.0f);
    }
}
