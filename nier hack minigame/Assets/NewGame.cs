using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGame : MonoBehaviour
{
    public bool notResetButton = true;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //check if user pressed R once
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reset();
        }
    }
    
    public void Reset()
    {
        GameManager.Instance.NewGame();
    }
}
