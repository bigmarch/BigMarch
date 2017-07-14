using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
	    
	}

    void OnGUI()
    {
        if(GUILayout.Button("Record"))
        {
            MicroPhoneInput.getInstance().StartRecord();
        }
        if(GUILayout.Button("Play"))
        {
            MicroPhoneInput.getInstance().PlayRecord();
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
