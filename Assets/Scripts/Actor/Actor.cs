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

    public static bool operator ==( Actor _one, Actor _two)
    {
        if (object.ReferenceEquals(_one, null) && object.ReferenceEquals(_two, null))
            return true;
        if (object.ReferenceEquals(_one, null) || object.ReferenceEquals(_two, null))
            return false;
        return _one.ID == _two.ID;
    }

    public static bool operator !=(Actor _one, Actor _two)
    {
        return !(_one == _two);
    }

    public override bool Equals(object obj)
    {
        var actor = obj as Actor;
        return actor != null &&
               base.Equals(obj) &&
               ID == actor.ID;
    }

    public override int GetHashCode()
    {
        var hashCode = 1458105184;
        hashCode = hashCode * -1521134295 + base.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ID);
        return hashCode;
    }
}
