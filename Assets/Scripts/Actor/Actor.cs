using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Actor : MonoBehaviour {
    public string ID;
    public Movement myMovement;
    public float speed = 1f;    //Default speed is one.
    private float _currentSpeed;
    public float currentSpeed {
        get { return _currentSpeed;  }
        set
        {
            _currentSpeed = value;
            if(myMovement != null)
                myMovement.SetMoveTime();
        }
    }

    // Use this for initialization
    void Start () {
        //If ID is not already set, set one
        if (ID == null || ID.Length < 3)
            ID = Guid.NewGuid().ToString();
        Debug.Log("New Player Actor : " + ID);

        CalCurrentSpeed();

        if (myMovement == null)
            myMovement = GetComponent<Movement>();

        GameManager.instance.AddActor(this);
    }

    //Using the base of speed, calcuated the current speed of the actor based on equipment
    public void CalCurrentSpeed()
    {
        currentSpeed = speed;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
