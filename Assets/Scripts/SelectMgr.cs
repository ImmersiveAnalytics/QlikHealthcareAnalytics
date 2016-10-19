using UnityEngine;
using System.Collections;

public class SelectMgr : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // Handle Tap Gestures
    void OnSelect()
    {
        gameObject.transform.position = new Vector3(1f, 1f, 1f);
    }
}
