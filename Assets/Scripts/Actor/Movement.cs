using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A compainon class to Actor that is incharge of:
/// 1) Moving the actor
///     a) Each movement is only the move between one node on myTiles to the next. 
///     b) Actual pathfinding is done by the PathManager objects not this script.
/// 2) Keeping track of the actors location in terms of grids and tiles.
/// 3) Making dealing with animations as they pertain to walking.
/// </summary>
public class Movement : MonoBehaviour {
    /// <summary>
    /// The actor associated with this script. All Movement scrips require and actor ascript and visa versa.
    /// </summary>
    public Actor MyActor { get; private set; }
    /// <summary>
    /// Will match the x and y value in the animator
    /// </summary>
    private float animationX, animationY;
    /// <summary>
    /// The tiles object this actor is on
    /// </summary>
    public Tiles MyTiles { get; set; }
    /// <summary>
    /// If true, this actor is currently walking between two nodes
    /// </summary>
    public bool Walking { get; private set; }
    /// <summary>
    /// Indicates the actor is currently in process of moving between cells. 
    /// This differs from Walking in that a walking actor is currently following a path, but may not be currently in the process of moving.
    /// </summary>
    public bool InStep { get; private set; }
    /// <summary>
    /// The array index in MyTiles.tiles that the actor is on. If walking TileCoord.x/Y will be the tile you are moving toward, and StartTileCoord.x/Y will be the tile you started from.
    /// If not moving TileCoord.x/TileCoord.y will be your exact position in the array and StartTileCoord.x/Y will be 0.
    /// </summary>
    public Coord TileCoord { get; private set; }
    public Coord StartTileCoord { get; private set; }
    public HashSet<MovingThroughItem> MyMovingThrough { get; private set; }
    /// <summary>
    /// myRigidboy, myCollider, and myAnimator are all referances to required gameObjects attached to all actors.
    /// </summary>
    private Rigidbody2D myRigidbody;
    private BoxCollider2D myCollider;
    private Animator myAnimator;
    /// <summary>
    /// The system doesn't allow changing the moveTime while moving, so if the _actor's speed changes while moving, the new moveTime is stroed here until the current move is complete
    /// </summary>
    private float nextMoveTime;
    private float _moveTime;
    /// <summary>
    /// The number of seconds it takes to move 1 node. This is determined based on the speed in actor using the formula in the method SeepToMoveTime
    /// </summary>
    private float MoveTime {
        get { return _moveTime; }
        set {
            _moveTime = value;
            inverseMoveTime = 1f / _moveTime;
        }
    }
    /// <summary>
    /// The inverse of move time, stored like this to make the calulation of move time to distance multiplication rather than division.
    /// </summary>
    private float inverseMoveTime;          //Used to make movement more efficient.
    /// <summary>
    /// If this is set to true, the current move action will be cancelled after the next frame.
    /// </summary>
    private bool cancelMove;

    /// <summary>
    /// Stores the caridnal directions in an enum to make them easier to use
    /// </summary>
    public enum Directions { North, Northeast, East, Southeast, South, Southwest, West, Northwest }
    /// <summary>
    /// Converts Directions into a float that can be multiplied by the size of a node to get the vector of travel
    /// </summary>
    public static Dictionary<Directions, Vector3> DirectionToVector = new Dictionary<Directions, Vector3>(){
        { Directions.North, new Vector3( 0f, 1f, 0f ) },
        { Directions.Northeast, new Vector3( 1f, 1f, 0f ) },
        { Directions.East, new Vector3( 1f, 0f, 0f ) },
        { Directions.Southeast, new Vector3( 1f, -1f, 0f ) },
        { Directions.South, new Vector3( 0f, -1f, 0f )},
        { Directions.Southwest, new Vector3( -1f, -1f, 0f ) },
        { Directions.West, new Vector3( -1f, 0f, 0f ) },
        { Directions.Northwest, new Vector3( -1f, 1f, 0f ) }
    };

    /// <summary>
    /// Converts a direction into an int that represent the direction that can be stored in the MovingThrough array to represent an actor moving through the node
    /// </summary>
    public static Dictionary<Directions, int> MovingThroughDirectionInt = new Dictionary<Directions, int>()
    {
        { Directions.North, 1 },
        { Directions.Northeast, 3 },
        { Directions.East, 5 },
        { Directions.Southeast, 7 },
        { Directions.South, 9 },
        { Directions.Southwest, 11 },
        { Directions.West, 13 },
        { Directions.Northwest, 15 }
    };

    public static Dictionary<int, Directions> MovingThroughIntDirection = new Dictionary<int, Directions>()
    {
        { 1, Directions.North },
        { 3, Directions.Northeast },
        { 5, Directions.East },
        { 7, Directions.Southeast },
        { 9, Directions.South },
        { 11, Directions.Southwest },
        { 13, Directions.West },
        { 15, Directions.Northwest }
    };

    /// <summary>
    /// Speed is an abstract property of an actor. This function translates that property into moveTime which is the number of seconds it takes the actor to move 1 tile
    /// </summary>
    public static float SpeedToMoveTime(float _speed)
    {
        return Mathf.Clamp(0.25f * (GameManager.instance.defaultSpeed / _speed), GameManager.instance.maxMovementSpeed, GameManager.instance.minMovementSpeed);
    }

    // Use this for initialization
    void Start() {
        //Get Components
        if (MyActor == null)
            MyActor = GetComponent<Actor>();
        if (myAnimator == null)
            myAnimator = GetComponent<Animator>();
        if (myRigidbody == null)
            myRigidbody = GetComponent<Rigidbody2D>();
        if (myCollider == null)
            myCollider = GetComponent<BoxCollider2D>();

        SetMoveTime();
        MyMovingThrough = new HashSet<MovingThroughItem>();

        StartCoroutine(WaitForActor());
        //Sets default to facing south and not walking
        SetWalking(false);
    }

    /// <summary>
    /// Since PlaceActor can't be run until this code is registed in the actor, this function simply waits until after the actor is ready to call PlaceActor
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForActor()
    {
        while (MyActor.myMovement == null)
            yield return null;
        PlaceActor();
        MyTiles.TilesToConsole();
        MyActor.MoveReady();
    }

    /// <summary>
    /// Sets the moveTime based on the actor's speed right now regardless of if this actor is currently moving
    /// </summary>
    private void SetMoveTimeNow()
    {
        MoveTime = SpeedToMoveTime(MyActor.currentSpeed);
        nextMoveTime = 0f;
    }

    /// <summary>
    /// Public function to set moveTime based on the actor's current speed. 
    /// If actor is currently moving then this will take place after the actor finishes the current move.
    /// </summary>
    public void SetMoveTime()
    {
        if (!Walking)
            SetMoveTimeNow();
        nextMoveTime = SpeedToMoveTime(MyActor.currentSpeed);
    }

    /// <summary>
    /// Sets the moveTime using nextMove time.
    /// </summary>
    private void SetNextMoveTime()
    {
        if (nextMoveTime != 0f)
            MoveTime = nextMoveTime;
        nextMoveTime = 0f;
    }

    public void PlaceActor(Coord _p)
    {
        TileCell aTile = MyTiles.GetTile(_p);
        PlaceActor(aTile.WorldPosition + GameManager.instance.TileOffset);
    }

    /// <summary>
    /// Anytime an actors position is change manually (outside of this class) it should always be with the method to ensure that actor know where it is.
    /// </summary>
    /// <param name="_position">New transform.position you wish to move the actor to</param>
    public bool PlaceActor(Vector3 _position)
    {
        PathManager pm = GameManager.instance.pathManager;

        //Stored so we can move the actor back to its original position if the new position is not passable
        Vector3 oldPosition = transform.position;
        transform.position = _position;
        //If not walking then the two variable below will be used to stop the actor. It is done in this order to precent having to call start/stop moving on an actor if PlaceActor fails
        Tiles oldTiles = MyTiles;
        Coord oldCoord = TileCoord;
        if (!PlaceActor())
        {
            transform.position = oldPosition;
            return false;
        }
        //IF walking get them walking again from this position.
        if (Walking) {
            CoordLinked aPath = null;
            if (pm.paths.ContainsKey(MyActor.ID))
                aPath = pm.paths[MyActor.ID];
            if (aPath != null && aPath.GetFoot() != null)
                pm.MoveActorTo(MyActor, aPath.GetFoot());
        }
        //If not walking remove their blocking on the old position, and start blocking again at the new position
        else
        {
            pm.ActorMoving(oldCoord, oldTiles, MyActor);
            pm.ActorStopped(MyActor);
        }   

        return true;
    }

    /// <summary>
    /// Used to place the actor on a tiles object so that the actor can keep track of its position.
    /// </summary>
    public bool PlaceActor()
    {
        PathManager aPathManager = GameManager.instance.pathManager;
        Coord p;
        Vector3 idealPosition;

        Tiles aTiles = aPathManager.WorldPositionToTile(transform.position, out p, out idealPosition);
        if (aTiles != null)
        {
            //Unblock the current tile
            aPathManager.ActorMoving(MyActor);
            //Register the new position
            MyTiles = aTiles;
            TileCoord = p.Copy();
            //Move the actor then block the new tile
            transform.position = idealPosition;
            if (!Walking)
                aPathManager.ActorStopped(MyActor);
            if (GameManager.instance.ShowDebug) Debug.Log("Actor, " + MyActor.ID + ", has been place at: " + TileCoord);
            return true;
        }
        return false;
    }
    /// <summary>
    /// Cancels the current move after the next frame. Move done actions will still be performed, except move true up.
    /// </summary>
    public void CancelMove()
    {
        cancelMove = true;
    }

    /// <summary>
    /// Moves an actor one node in the passed direction. This move will not slow towards the end of the move.
    /// </summary>
    /// <param name="_direction">The direction you wish to move this actor in.</param>
    public void MoveOne(Coord _destination)
    {
        MoveOne(_destination, false);
    }

    public static Directions CoordsToDirection(Coord _start, Coord _end)
    {
        Coord diff = _end - _start;
        if (diff.X > 0 && diff.Y < 0)
            return Directions.Northeast;
        else if (diff.X < 0f && diff.Y < 0)
            return Directions.Northwest;
        else if (diff.X > 0 && diff.Y > 0)
            return Directions.Southeast;
        else if (diff.X < 0 && diff.Y > 0)
            return Directions.Southwest;
        else if (diff.X > 0)
            return Directions.East;
        else if (diff.X < 0)
            return Directions.West;
        else if (diff.Y > 0)
            return Directions.South;
        else if (diff.Y < 0)
            return Directions.North;
        return Directions.South;
    }

    /// <summary>
    /// Moves and actor one node in the passed direction. It also handles:
    /// 1) tracking tilesX/Y and startTilesX/Y
    /// 2) Setting speed, and slowing the character down if they are moving into difficult terain
    /// 3) Sets moving through
    /// 4) Changes the animators facing directin as needed
    /// </summary>
    /// <param name="_direction">The direction you wish to move this actor in.</param>
    /// <param name="_endSlow">If true the actor will slow towards the end of the move.</param>
    public void MoveOne(Coord _destination, bool _endSlow)
    {
        if (GameManager.instance.pathManager.pathCalcCount.ContainsKey(MyActor.ID))
            GameManager.instance.pathManager.pathCalcCount[MyActor.ID] = 0;

        _destination = _destination.Copy();
        //Turn off cancelMove
        cancelMove = false;
        //Transform the current position to the desired end position based on the passed x and y, and the size of the tile
        Vector3 end = MyTiles.tiles[_destination.X, _destination.Y].WorldPosition + GameManager.instance.TileOffset;

        //Find the direction of travel
        Directions direction = CoordsToDirection(TileCoord, _destination);

        //Capture the current position as the starting point of the move
        StartTileCoord = TileCoord.Copy();

        //Stores the position in MyTiles.nodes that this actor is moving to
        TileCoord = _destination;

        //Sets the current moveTime to make sure its acurate before the move.
        SetMoveTimeNow();
        //If entering difficult terain double the moveTime so the actor moves twice as slowly.
        if (MyTiles.tiles[TileCoord.X, TileCoord.Y].Type == TileCell.TileType.Difficult)
            MoveTime *= 2;

        MyTiles.SetMovingThrough(CalculateMovingThrough(StartTileCoord, TileCoord));

        //Start actually moving, either at a constant speed or progressivly more slowly
        if (_endSlow)
            StartCoroutine(SmoothMovement(end));
        else
            StartCoroutine(FastMovement(end));
        //Set the animation direction
        SetFacing(direction);
    }

    public HashSet<MovingThroughItem> CalculateMovingThrough(Coord _start, Coord _end)
    {
        return CalculateMovingThrough(CoordsToDirection(_start, _end), _start, _end);
    }

    /// <summary>
    /// Takes a direction of travel and the two adjacent Coords that represent the start and end of a movement, and returns the set of MovingThroughItems the actor would ocupy to make the move.
    /// Any node that will not be ocupied by this actor at the end of the move uses the base moving through, any node that will be occupied at the end of the move will base moving through +1.
    /// This function will not actually set MovingThrough on a tile, or the movements MyMovingThrough.
    /// This method was writen under the asumption that the two coords are not more than 1 step appart, if they are more than one step appart this method is unlikely to return the desired result.
    /// </summary>
    /// <param name="_direction">Direction of travel represented by the two coords</param>
    /// <param name="_start">Where the actor would start</param>
    /// <param name="_end">Where the actor would end</param>
    /// <returns>A set of MovingThroughItems that represent where the actors would be.</returns>
    public HashSet<MovingThroughItem> CalculateMovingThrough(Directions _direction, Coord _start, Coord _end)
    {
        //Any location where the actor will not be after the move should be mt. Any location they will occupy after the move should be mt + 1
        HashSet<MovingThroughItem> output = new HashSet<MovingThroughItem>();
        int mt = MovingThroughDirectionInt[_direction];
        output.Add(new MovingThroughItem(_start, MyActor, mt));
        output.Add(new MovingThroughItem(_end, MyActor, mt + 1));
        if (Coord.AreDiagonal(_start, _end))
        {
            output.Add(new MovingThroughItem(new Coord(_start.X, _end.Y), MyActor, mt));
            output.Add(new MovingThroughItem(new Coord(_end.X, _start.Y), MyActor, mt));
        }
        return output;
    }

    /// <summary>
    /// When actors start a move they reserve nodes to prevent two actors trying to move into the same node.
    /// This function removes this actors reservations in MyTiles.movingThrough.
    /// </summary>
    public void RemoveMovingThrough()
    {
        if(MyMovingThrough != null)
            foreach (MovingThroughItem aItem in MyMovingThrough)
                MyTiles.RemoveMovingThrough(aItem.P);
        MyMovingThrough.Clear();
    }

    /// <summary>
    /// Moves an actor to the passed end point at a consitent speed based on moveTime
    /// </summary>
    /// <param name="_end">The vector3 you wish to move the actor to.</param>
    /// <returns></returns>
    protected IEnumerator FastMovement(Vector3 _end)
    {
        //Indicate the actor is walking
        SetWalking(true);
        InStep = true;

        //Convert find the true distance being traveled, convert that into "tiles" by dividing by timeSize and
        //then divide the whole thing by 1 to get the inverse so we can multiply rath than divide.
        float inverseDistanceRatio = 1 / (Vector3.Distance(transform.position, _end) / GameManager.instance.tileSize);
        //Creates a move vector, that when added to the actors current position will move them to end
        Vector3 moveVector = _end - transform.position;
        //Multiply the fraction of a second between the last frame and this one (Time.deltaTime), by 
        //the inverse of how long it should take an actor to cross a tile in seconds (inverseMoveTime)
        //Divied by the actual distance moved, expressed in terms of tiles (inverseDistanceRatio)
        Vector3 newPostion = transform.position + ( moveVector  * ( inverseMoveTime * Time.deltaTime * inverseDistanceRatio) );

        //While not cancelMove and the neither the X or Y position has moved passed the end point
        while (!cancelMove
            && (Mathf.Abs(moveVector.x) > .001 || Mathf.Abs(moveVector.y) > .001 )
            && (Mathf.Abs(moveVector.x) < .001
            || (moveVector.x > 0 && newPostion.x < _end.x)
            || (moveVector.x < 0 && newPostion.x > _end.x))
            && (Mathf.Abs(moveVector.y) < .001
            || (moveVector.y > 0 && newPostion.y < _end.y)
            || (moveVector.y < 0 && newPostion.y > _end.y)))
        {
            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            myRigidbody.MovePosition(newPostion);

            //Multiply the fraction of a second between the last frame and this one (Time.deltaTime), by 
            //the inverse of how long it should take an actor to cross a tile in seconds (inverseMoveTime)
            newPostion = transform.position + (moveVector * (inverseMoveTime * Time.deltaTime * inverseDistanceRatio));

            //Return until next frame and then contiue loop
            yield return null;
        }
        //If the move wasn't cancelled move the actor the exsact postion because floats
        if (!cancelMove)
            transform.position = _end;
        else
            SetWalking(false);
        //Reset moving through
        RemoveMovingThrough();
        InStep = false;
        //Call pathmanger to get the next step
        GameManager.instance.pathManager.AdvancePath(MyActor.ID);
    }

    /// <summary>
    /// This move type is designed to progresively slow down towards the end of the move cycle and thus is called as the last step in the path.
    /// </summary>
    /// <param name="_end">The Vector3 you wish to move to.</param>
    /// <returns></returns>
    protected IEnumerator SmoothMovement(Vector3 _end)
    {
        //Indicate the actor is walking
        SetWalking(true);
        InStep = true;
        //Convert find the true distance being traveled, convert that into "tiles" by dividing by timeSize and
        //then divide the whole thing by 1 to get the inverse so we can multiply rath than divide.
        float inverseDistanceRatio = 1 / (Vector3.Distance(transform.position, _end) / GameManager.instance.tileSize);
        //Calculate the remaining distance to move based on the square magnitude of the difference between current position and end parameter. 
        //Square magnitude is used instead of magnitude because it's computationally cheaper.
        float sqrRemainingDistance = (transform.position - _end).sqrMagnitude;
        
        //While that distance is greater than a very small amount
        while (!cancelMove && sqrRemainingDistance > .0001f)
        {
            //Find a new position proportionally closer to the end, based on the moveTime
            //Vector3 newPostion = Vector3.MoveTowards(myRigidbody.position, end, inverseMoveTime * Time.deltaTime);
            Vector3 newPostion = Vector3.MoveTowards(myRigidbody.position, _end, inverseMoveTime * Time.deltaTime * inverseDistanceRatio);

            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            myRigidbody.MovePosition(newPostion);

            //Recalculate the remaining distance after moving.
            sqrRemainingDistance = (transform.position - _end).sqrMagnitude;

            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }
        //If the move wasn't cancelled move the actor the exsact postion because floats
        if (!cancelMove)
            transform.position = _end;
        //Reset moving through
        RemoveMovingThrough();
        //Call pathmanger, this should trigger closing the path
        GameManager.instance.pathManager.AdvancePath(MyActor.ID);
        //Stop walking animation
        SetWalking(false);
        InStep = false;
        //Since we don't expect to go back to MoveOne, set MoveTime here
        SetNextMoveTime();
    }

    /// <summary>
    /// If true is passed actors walk animation will start, if false it will end
    /// </summary>
    /// <param name="_walking">If true is passed actors walk animation will start, if false it will end</param>
    public void SetWalking( bool _walking)
    {
        myAnimator.SetBool("Walking", _walking);
        Walking = _walking;
    }

    /// <summary>
    /// Used to determine which direction the actor is facing based on the passed _direction. If they are moving diagonally we choose wichever of the two direction we aren't currently facing.
    /// </summary>
    /// <param name="_direction">Direction to turn the actor in</param>
    public void SetFacing( Directions _direction)
    {
        float x = DirectionToVector[_direction].x, y = DirectionToVector[_direction].y;

        if(Mathf.Abs(x) > .01f && Mathf.Abs(y) > .01f)
        {
            if (Mathf.Abs(animationX) > .01f)
                x = 0;
            else
                y = 0;
        }

        animationX = x;
        animationY = y;
        myAnimator.SetFloat("X", animationX);
        myAnimator.SetFloat("Y", animationY);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
