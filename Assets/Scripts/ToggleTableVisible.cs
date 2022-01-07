using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleTableVisible : MonoBehaviour
{
    public void ToggleVisible()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
