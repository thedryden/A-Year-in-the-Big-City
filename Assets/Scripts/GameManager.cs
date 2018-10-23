using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    //Used to implement the singleton pattern
    public static GameManager instance = null;

    public PathManager pathManager;

    public List<Actor> Actors;
    public Dictionary<string,Actor> ActorDictionary;
    public float tileSize = 1;
    public Vector3 TileOffset { get; private set; }
    public float defaultSpeed;
    public float minMovementSpeed;
    public float maxMovementSpeed;
    /// <summary>
    /// If on debuging become verbose. If off only items that represent problems, but not nessisarily errors, will be displayed
    /// </summary>
    public bool ShowDebug;

    // Use this for initialization
    void Awake () {
        //Implements singleton pattern
        if (instance == null)
            instance = this;

        if(pathManager == null)
            pathManager = GetComponent<PathManager>();
        if (Actors == null)
            Actors = new List<Actor>();
        if (ActorDictionary == null)
            ActorDictionary = new Dictionary<string, Actor>();

        TileOffset = new Vector3( tileSize * .5f, tileSize * .5f, tileSize * .5f );
    }

    public void AddActor( Actor _actor )
    {
        //Register actors
        if (ActorDictionary.ContainsKey(_actor.ID)){
            string newID = Guid.NewGuid().ToString();
            Debug.Log("There is already an actor with the ID of: " + _actor.ID + ". ID has been changed to " + newID);
            _actor.ID = newID;
        }

        Actors.Add(_actor);
        ActorDictionary.Add(_actor.ID, _actor);
    }
	
	// Update is called once per frame
	void Update () {
        //Action to take if mouse is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pathManager.MoveActorTo("9d2c-59755b7831dc-9d2c-59755b7831dc", mousePoint);
        }
    }
}
