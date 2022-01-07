using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectInfoScript : MonoBehaviour
{

    public void toggleInfoVisible()
    {
        this.gameObject.SetActive(!this.gameObject.activeInHierarchy);
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
