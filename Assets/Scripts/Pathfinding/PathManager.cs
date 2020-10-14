using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Creates and manages all paths for all actors.
/// </summary>
public class PathManager : MonoBehaviour {
    /// <summary>
    /// Contains all paths of all actor. Once a path is complete it will be removed, so don't always expect there to be a path
    /// </summary>
    public Dictionary<string, CoordLinked> paths;
    public Dictionary<string, int> pathCalcCount;
    /// <summary>
    /// An array containing a Tiles object for each grid in the scene
    /// </summary>
    public Tiles[] tiles;
    /// <summary>
    /// The index in tiles array for the last tiles object that used by this class
    /// </summary>
    private int activeTile;

    /// <summary>
    /// Event that is called whenever a path completes
    /// </summary>
    public delegate void PathComplete(Actor _actor);
    public event PathComplete OnPathComplete;

    /// <summary>
    /// Initalizes the class. This is done in Awake because it must always happen before Actor are started
    /// </summary>
    void Awake () {
        GetGrids();
        paths = new Dictionary<string, CoordLinked>();
        pathCalcCount = new Dictionary<string, int>();
        GameManager.instance.PathReady();
    }

    /// <summary>
    /// Finds all grids in the scene, creates a tiles object for each one, and stores that tiles object in tiles
    /// </summary>
    public void GetGrids()
    {
        Grid[] allGrids = FindObjectsOfType<Grid>();
        tiles = new Tiles[allGrids.Length];

        for (int i = 0; i < allGrids.Length; i++)
            tiles[i] = new Tiles(allGrids[i]);

        activeTile = 0;
    }

    /// <summary>
    /// Returns the tiles object at activeTile index
    /// </summary>
    /// <returns></returns>
    public Tiles ActiveTile()
    {
        return tiles[activeTile];
    }

    /// <summary>
    /// Takes a world position and searches all tiles to find the tiles object and cell that world position is (_x and _y) and also returns an idealPosition.
    /// The ideal position is a worldposition vector3 that represents the center of cell returned by _x and _y.
    /// If you pass a position that is not within any tile object this function will return null, otherwise it will return the tiles object the position is in.
    /// If you pass a position that is in a blocked cell, this function will search around that cell to find an unblocked one and return the information for that.
    /// </summary>
    /// <param name="_worldPosition">Position to check</param>
    /// <param name="_x">Out parameter, returns the row position in the tiles array</param>
    /// <param name="_y">Out parameter, returns the column position in the tiles array</param>
    /// <param name="_idealPosition">A worldposition vector3 that represents the center of cell returned by _x and _y.</param>
    /// <returns>The tiles object that contains this worldPosition</returns>
    public Tiles WorldPositionToTile(Vector3 _worldPosition, out Coord _p, out Vector3 _idealPosition)
    {
        return WorldPositionToTile(_worldPosition, out _p, out _idealPosition, false);
    }

    /// <summary>
    /// Takes a world position and searches all tiles to find the tiles object and cell that world position is (_x and _y) and also returns an idealPosition.
    /// The ideal position is a worldposition vector3 that represents the center of cell returned by _x and _y.
    /// If you pass a position that is not within any tile object this function will return null, otherwise it will return the tiles object the position is in.
    /// If ignoreBlocking is set to false and you pass a position that is in a blocking cell, this function will search around that cell to find an unblocked one and return the information for that.
    /// If ignoreBlocking is set to true this function will return a blocking cell, but if the position you pass is a placeholder cell, this function will search around that cell to find a cell that is not a placeholder and return the information for that.
    /// </summary>
    /// <param name="_worldPosition">Position to check</param>
    /// <param name="_x">Out parameter, returns the row position in the tiles array</param>
    /// <param name="_y">Out parameter, returns the column position in the tiles array</param>
    /// <param name="_idealPosition">A worldposition vector3 that represents the center of cell returned by _x and _y.</param>
    /// <returns>The tiles object that contains this worldPosition</returns>
    public Tiles WorldPositionToTile(Vector3 _worldPosition, out Coord _p, out Vector3 _idealPosition, bool _ignoreBlocking )
    {
        //Check active tile first.
        if (ActiveTile().WorldPositionToTile(_worldPosition, out _p, out _idealPosition, _ignoreBlocking))
            return ActiveTile();

        //Loop over all other tiles
        for (int i = 0; i < tiles.Length; i++)
        {
            //Don't recheck the active tilea
            if (i == activeTile)
            {
                Tiles aTiles = tiles[i];
                if (aTiles.WorldPositionToTile(_worldPosition, out _p, out _idealPosition, _ignoreBlocking))
                {
                    return aTiles;
                }
            }
        }
        //If not found
        _p = Coord.Zero();
        _idealPosition = Vector3.zero;
        return null;
    }
    /// <summary>
    /// When an actor stops moving the tile they're standing on become blocked. This function reverse that process.
    /// </summary>
    /// <param name="_id">ID of an actor</param>
    public void ActorMoving(string _id)
    {
        ActorMoving(GameManager.instance.ActorDictionary[_id]);
    }

    /// <summary>
    /// When an actor stops moving the tile they're standing on become blocked. This function reverse that process.
    /// </summary>
    /// <param name="_actor">actor who needs to no longer be blocking</param>
    public void ActorMoving(Actor _actor)
    {
        ActorMoving(_actor.myMovement.TileCoord, _actor.myMovement.MyTiles, _actor);
    }
    
    /// <summary>
     /// When an actor stops moving the tile they're standing on become blocked. This function reverse that process.
     /// </summary>
     /// <param name="_actor">actor who needs to no longer be blocking</param>
    public void ActorMoving(Coord _position, Tiles _tiles, Actor _actor)
    {
        if (_tiles == null)
            return;
        
        //Get current type of tile, revert tileOverride, if the new tile type is the same as the old tile type we're done
        TileCell.TileType startType = _tiles.GetTile(_position).Type;
        _tiles.GetTile(_position).RevertOverride();
        if (startType == _tiles.GetTile(_position).Type)
            return;

        //If the tile type is differnt update the graph
        _tiles.SetEdgesOfEdges(_position);

        //Since an non moving actor could block an entire path, all paths are recalculated.
        foreach (Actor aActor in GameManager.instance.Actors)
        {
            //Don't recalculate this actors path, and don't recalculat if the actor doens't already have a path
            if (_actor.ID != aActor.ID && paths.ContainsKey(aActor.ID))
            {
                //Get destination of the path, and recalculate
                Coord aFoot = paths[aActor.ID].GetFoot();
                if (aFoot != null)
                {
                    Vector3 end = _tiles.GetTile(aFoot).WorldPosition;
                    MoveActorTo(aActor, end);
                }
            }
        }
    }

    /// <summary>
    /// Whenever an actor stops moving, the tile they are standing on are removed from the graph.
    /// </summary>
    /// <param name="_id">ID of an actor</param>
    public void ActorStopped(string _id)
    {
        ActorStopped(GameManager.instance.ActorDictionary[_id]);
    }

    /// <summary>
    /// Whenever an actor stops moving, the tile they are standing on are removed from the graph.
    /// </summary>
    /// <param name="_id">The actor object that stopped moving</param>
    public void ActorStopped(Actor _actor)
    {
        //Get actors position
        Tiles aTiles = _actor.myMovement.MyTiles;
        if (aTiles == null)
            return;
        
        //Set they're tile as blocking and adjust the graph
        aTiles.GetTile(_actor.myMovement.TileCoord).TypeOverride = TileCell.TileType.Blocking;
        aTiles.SetEdgesOfEdges(_actor.myMovement.TileCoord);

        foreach (Actor aActor in GameManager.instance.Actors)
        {
            //If the actor has a path that's on the same tiles object that we just modified
            if (paths.ContainsKey(aActor.ID) && aTiles.Equals(aActor.myMovement.MyTiles) ){
                //Recalc path IF it is effected by the change
                ActorStoppedHelper(_actor.myMovement.TileCoord, aActor);
            }
        }
    }

    /// <summary>
    /// Loops over every node in the actors path and if it is effected by the change, recalculate it.
    /// This is a helper function and should not be called if the Actor is on a different tiles object than the tile that change or the actor does not have a path.
    /// </summary>
    /// <param name="_actor">_actor to test and possibly recalculate</param>
    private void ActorStoppedHelper(Coord _p, Actor _actor)
    {
        //Get path and start an iterator of the path
        CoordLinked aPath = paths[_actor.ID];
        Coord aCoord = aPath.StartIteration();
        //Loop over every coord in the path
        while (aCoord != null)
        {
            //Check the node itself and all adjacent nodes to see if they are effected.
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if ( ( aCoord + new Coord( x, y ) ) == _actor.myMovement.TileCoord )
                    {
                        //If affected get destination and recalc path
                        Vector3 end = _actor.myMovement.MyTiles.GetTile( aPath.GetFoot() ).WorldPosition;
                        MoveActorTo(_actor, end);
                        return;
                    }
                }
            }
            aCoord = aPath.Next();
        }
    }

    /// <summary>
    /// Advances and actor one node along the path, checking for local avodiance along the way.
    /// </summary>
    /// <param name="_id">Of the actor you wish to move</param>
    public void AdvancePath(string _id)
    {
        AdvancePath(GameManager.instance.ActorDictionary[_id]);
    }

    /// <summary>
    /// Advances and actor one node along the path, checking for local avodiance along the way.
    /// </summary>
    /// <param name="_id">Of the actor you wish to move</param>
    public void AdvancePath(Actor _actor)
    {
        Tiles tiles = _actor.myMovement.MyTiles;

        CoordLinked aPath = null;
        if(paths.ContainsKey(_actor.ID))
            aPath = paths[_actor.ID];

        //If this actor no longer has a path, stop the actor
        if (aPath == null || aPath.GetHead() == null)
        {
            paths.Remove(_actor.ID);
            ActorStopped(_actor);
            OnPathComplete(_actor);
            return;
        }

        Coord next = aPath.Pop();
        
        //If the requested "move" is to the position the actor is already at, call this againt to get the next node and then do nothing
        if (next == _actor.myMovement.TileCoord)
        {
            AdvancePath(_actor);
            return;
        }

        HashSet<MovingThroughItem> toAvoid = new HashSet<MovingThroughItem>();
        HashSet<Coord> toAvoidFirstStep = new HashSet<Coord>();
        int code = ProcessMovingThrough(_actor.myMovement, next, out toAvoid, out toAvoidFirstStep);
        if (code == 1)
        {
            ProcessCodeOne(_actor, toAvoid, toAvoidFirstStep);
            return;
        }
        else if (code == 2)
        {
            ProcessCodeTwo(_actor, toAvoid, toAvoidFirstStep);
            return;
        }
        else if (code == 3)
        {
            StartCoroutine(AdvancePathHelper(_actor));
            return;
        }

        //Determine if this is the last part of the path, if it is set moveSlow to true
        bool moveSlow = (aPath.Length == 0);

        //Move the actor
        _actor.myMovement.MoveOne(next, moveSlow);
    }

    private IEnumerator AdvancePathHelper(Actor _actor)
    {
        yield return new WaitForSeconds(.1f);
        AdvancePath(_actor);
    }

    private IEnumerator AdvancePathHelper(Actor _actor, float _seconds)
    {
        yield return new WaitForSeconds(_seconds);
        AdvancePath(_actor);
    }

    public int ProcessMovingThrough( Movement _move, Coord _next, out HashSet<MovingThroughItem> _toAvoid, out HashSet<Coord> _toAvoidFirstStep )
    {
        /* Posibilities, in any drawings we are U is us and S is collisions start point and E is collisions end point
         * 1) mt is 0, nothing there no collision
         * 2) We are traveling in the same direction. We if so we need to lookup the speed of the actor we just bumped into. If we're faster the we recalc the with _toAvoid on the next 3 tiles dead ahead.
         * 3) T-Bone. If we are T-Boning in start from location, then simply wait. If we T-Boned in against the destination and moving towards the start puts us close to the destination then create a new path that start with a move in that direction followed by a recalc.
         *  SE
         *   U move if our desitnation is <- otherwise wait
         * 4) Head on collision since we got here second we're always the victum. Recalculate with toAvoid on the two blocked tiles
         * All of this will be reduce to a 3 point scale, where the lower items on the scale always take precidence over greater ones
         * 1) You MUST calculate a new path that incorporates to avoid and start that path instead.
         * 2) Calculate a new path (np) and compare it to the current path (cp). If np.length <= cp.length + 1, then replace the old path with the new path and start navigating that.
         * 3) No new path required, just wait
         * for your path to clear.
         */
        _toAvoid = null;
        _toAvoidFirstStep = null;
        HashSet<MovingThroughItem> myMt = _move.CalculateMovingThrough(_move.TileCoord, _next);
        Dictionary<string,List<MovingThroughItem>> toProcess = null;
        _move.MyTiles.SetMovingThrough(_move.CalculateMovingThrough(_move.TileCoord, _move.TileCoord));
        foreach ( MovingThroughItem aMt in myMt)
        {
            MovingThroughItem temp = _move.MyTiles.GetMovingThrough(aMt.P);
            if(temp.mt != 0 && temp.myActor != _move.MyActor)
            {
                if (toProcess == null)
                    toProcess = new Dictionary<string, List<MovingThroughItem>>();
                if (!toProcess.ContainsKey(temp.myActor.ID))
                    toProcess.Add(temp.myActor.ID, new List<MovingThroughItem>());
                toProcess[temp.myActor.ID].Add(temp);
            }
        }
        if (toProcess == null)
        {
            _move.RemoveMovingThrough();
            return 0;
        }

        _toAvoid = new HashSet<MovingThroughItem>();
        Movement.Directions myDir = Movement.CoordsToDirection(_move.TileCoord, _next);
        int code = int.MaxValue;
        foreach (KeyValuePair<string, List<MovingThroughItem>> entry in toProcess)
            code = UDF.Min(code, ProcessMovingThroughHelper(_move, myDir, _toAvoid, GameManager.instance.ActorDictionary[entry.Key], entry.Value));

        if (code < 3){
            _toAvoidFirstStep = new HashSet<Coord>();
            foreach (Node aNode in _move.MyTiles.GetNode(_move.TileCoord).edges)
                if (Coord.AreDiagonal(_move.TileCoord, aNode.P))
                    _toAvoidFirstStep.Add(aNode.P);
        }
        return code;
    }

    public int ProcessMovingThroughHelper(Movement _move, Movement.Directions _myDir, HashSet<MovingThroughItem> _toAvoid, Actor _cActor, List<MovingThroughItem> _cList)
    {
        bool presentAtEnd = false;
        Movement.Directions aDir = Movement.Directions.South;
        Movement cMove = _cActor.myMovement;
        _toAvoid.UnionWith(cMove.MyMovingThrough);
        foreach (MovingThroughItem aMT in _cList)
        {
            aDir = Movement.Directions.South;
            if (Movement.MovingThroughIntDirection.ContainsKey(aMT.mt))
                aDir = Movement.MovingThroughIntDirection[aMT.mt];
            else if (Movement.MovingThroughIntDirection.ContainsKey(aMT.mt + 1))
            {
                aDir = Movement.MovingThroughIntDirection[aMT.mt + 1];
                presentAtEnd = true;
            }
            else
            {
                Debug.Log("Unable to handle local avoidance becase we could not get a direction from the movingThroughInt of " + aMT.mt);
                return 3; //If we can't get a direction, there was an error, so we'll just wait and hope it goes away
            }
        }
        //Moving same direction, see if my speed is greater than their speed, recal path adding next two of their steps to toAvoid
        if (aDir == _myDir)
        {
            if (_move.MyActor.speed > _cActor.speed)
            {
                CoordLinked aPath = paths[_cActor.ID];
                Coord prev = cMove.TileCoord;
                Coord next = aPath.StartIteration();
                int i = 1;
                while (next != null && i < 3)
                {
                    HashSet<MovingThroughItem> aMt = cMove.CalculateMovingThrough(prev, next);
                    _toAvoid.UnionWith(aMt);
                    prev = next;
                    next = aPath.Next();
                    i++;
                }
                if (GameManager.instance.ShowDebug) Debug.Log("Atempting to find a way around, may wait. The actor " + _move.MyActor.ID + "'s path is blocked by the actor" + _cActor.ID + ".");
                return 2;//tell the pathfinder to TRY recalculating
            }
            else
            {
                if (GameManager.instance.ShowDebug) Debug.Log("Waiting for path to clear. The actor " + _move.MyActor.ID + "'s path is blocked by the actor" + _cActor.ID + ".");
                return 3;//Just wait it out
            }
        }
        int myReverseMt = Movement.MovingThroughDirectionInt[_myDir];
        myReverseMt = (myReverseMt - 8 > 0) ? myReverseMt - 8 : myReverseMt - 8 + 16;
        //If we're on a colision course
        if (myReverseMt == _cList[0].mt || myReverseMt == _cList[0].mt - 1) {
            if (GameManager.instance.ShowDebug) Debug.Log("Finding a new path. The actor " + _move.MyActor.ID + "'s path is blocked by the actor" + _cActor.ID + ".");
            return 1;
        }

        //Tbone
        if (presentAtEnd)
        {
            if (GameManager.instance.ShowDebug) Debug.Log("Atempting to find a way around, may wait.The actor " + _move.MyActor.ID + "'s path is blocked by the actor" + _cActor.ID + ".");
            return 2;
        }
        if (GameManager.instance.ShowDebug) Debug.Log("Waiting for path to clear. The actor " + _move.MyActor.ID + "'s path is blocked by the actor" + _cActor.ID + ".");
        return 3;
    }

    private void ProcessCodeOne(Actor _actor, HashSet<MovingThroughItem> _toAvoid, HashSet<Coord> _toAvoidFirstStep)
    {
        HashSet<Coord> toAvoid = new HashSet<Coord>();
        foreach (MovingThroughItem aMt in _toAvoid)
            toAvoid.Add(aMt.P);
        _actor.myMovement.RemoveMovingThrough();
        MoveActorTo(_actor, paths[_actor.ID].GetFoot(), toAvoid, _toAvoidFirstStep, null);
    }

    private void ProcessCodeTwo(Actor _actor, HashSet<MovingThroughItem> _toAvoid, HashSet<Coord> _toAvoidFirstStep)
    {
        Movement move = _actor.myMovement;
        CoordLinked currentPath = paths[_actor.ID];
        HashSet<Coord> toAvoid = new HashSet<Coord>();
        foreach (MovingThroughItem aMt in _toAvoid)
            toAvoid.Add(aMt.P);
        _actor.myMovement.RemoveMovingThrough();
        CoordLinked aPath = CalculatePath(move.MyTiles, move.TileCoord, currentPath.GetFoot(), toAvoid, _toAvoidFirstStep);
        if (aPath.Length >= currentPath.Length + 1)
            MoveActorTo(_actor, currentPath.GetFoot(), null, null, aPath);
        else
            AdvancePathHelper(_actor);
    }

    /// <summary>
    /// Moves an actor to the specified location by creating a path, and then starts the process of walking that path moving from one node to the next one at a time.
    /// </summary>
    /// <param name="_id">ID of the actor you wish to move</param>
    /// <param name="_destination">The world position you wish to move the actor to</param>
    public void MoveActorTo(string _id, Vector3 _destination)
    {
        MoveActorTo(GameManager.instance.ActorDictionary[_id], _destination);
    }

    public void MoveActorTo(Actor _actor, Vector3 _destination)
    {
        Coord destCord = Coord.Zero();

        //Make sure destination is on the same tiles object as the actor and normalize the position
        if (!_actor.myMovement.MyTiles.WorldPositionToTile(_destination, out destCord, out _destination))
        {
            //If the desitination isn't on the same tiles, we're done
            Debug.Log("The Actor " + _actor.ID + " can't be moved to " + _destination + " becase its not on the same tiles object as the actor.");
            return;
        }

        MoveActorTo(_actor, destCord, null, null, null);
    }

    public void MoveActorTo(string _id, Coord _destination)
    {
        MoveActorTo(GameManager.instance.ActorDictionary[_id], _destination, null, null, null);
    }

    public void MoveActorTo(Actor _actor, Coord _destination)
    {
        MoveActorTo(_actor, _destination, null, null, null);
    }

    /// <summary>
    /// Moves an actor to the specified location by creating a path, and then starts the process of walking that path moving from one node to the next one at a time.
    /// </summary>
    /// <param name="_id">The actor you wish to move</param>
    /// <param name="_destination">The world position you wish to move the actor to</param>
    private void MoveActorTo(Actor _actor, Coord _destination, HashSet<Coord> _toAvoid, HashSet<Coord> _toAvoidFirstStep, CoordLinked _path)
    {
        //Determine if the actor is already moving
        bool alreadyMoving = paths.ContainsKey(_actor.ID);

        //Remove any existing path. Removing this will effective abort the current path. Resulting in either the actor walking our new path, or finishing their current tile to tile move and stopping normally if we couldn't create a new one.
        paths.Remove(_actor.ID);

        if (!pathCalcCount.ContainsKey(_actor.ID))
            pathCalcCount.Add(_actor.ID, 0);

        if (pathCalcCount[_actor.ID] > 5)
        {
            Debug.Log("Actor " + _actor.ID + " has attempted to calculate a path 3 times in a row without actually moving. Cancelling move.");
            //Calling advancePath while no path is present to close the path
            //AdvancePath(_actor);
            return;
        }

        Tiles tiles = _actor.myMovement.MyTiles;
        
        //If we're not already moving we need to set the actor as moving before we calculate the path otherwise they'll be standing on a blocking tiles and no path can be found.
        if (!alreadyMoving)
            ActorMoving(_actor);

        CoordLinked newPath;
        if (_path == null)
            newPath = CalculatePath(tiles, _actor.myMovement.TileCoord, _destination, _toAvoid, _toAvoidFirstStep);
        else
            newPath = _path;

        if (newPath == null)
        {
            //If already moving let the actor come to a stop first, if not stop it to remove it from the graph
            if (!alreadyMoving)
                ActorStopped(_actor);
            return;
        }

        //Store the path
        paths.Add(_actor.ID, newPath);
        pathCalcCount[_actor.ID]++;

        DrawDebugPath(_actor, newPath, tiles);

        //If not already moving start moving, if already moving cancel the path so we will start on this new path at the next frame
        if (!alreadyMoving || !_actor.myMovement.InStep)
            AdvancePath(_actor);
        else
            _actor.myMovement.CancelMove();
    }

    private void DrawDebugPath(Actor _actor, CoordLinked _path, Tiles _tiles)
    {
        //Draw path for debugging
        Vector3 offset = new Vector3(GameManager.instance.tileSize / 2, GameManager.instance.tileSize / 2, GameManager.instance.tileSize / 2);
        Coord aCoord = _path.StartIteration();
        Vector3 start = _tiles.GetTile(_actor.myMovement.TileCoord).WorldPosition + offset;
        Vector3 end = _tiles.GetTile(aCoord).WorldPosition + offset;
        Debug.DrawLine(start, end, Color.red, 10, false);
        while (aCoord != null)
        {
            start = _tiles.GetTile(aCoord).WorldPosition + offset;
            aCoord = _path.Next();
            if (aCoord == null)
                break;
            end = _tiles.GetTile(aCoord).WorldPosition + offset;
            Debug.DrawLine(start, end, Color.red, 10, false);
        }
    }

    private CoordLinked CalculatePath(Tiles _tiles, Coord _start, Coord _end)
    {
        return CalculatePath(_tiles, _start, _end, null, null);
    }

    private CoordLinked CalculatePath(Tiles _tiles, Coord _start, Coord _end, HashSet<Coord> _toAvoid, HashSet<Coord> _toAvoidFirstStep)
    {
        Stopwatch timer = Stopwatch.StartNew();

        //Make sure both coords are valid
        if (!UDF.Between(_start.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_start.Y, 0, _tiles.tiles.GetLength(1))
            || !UDF.Between(_end.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_end.Y, 0, _tiles.tiles.GetLength(1)))
        {
            Debug.Log("Can't calculated path between " + _start + " and " + _end + " because one or more points are off the passed tiles objects");
            return null;
        }

        Node source = _tiles.GetNode(_start);
        Node target = _tiles.GetNode(_end);
        
        FNodeList open = new FNodeList(new FNode(source, 0, target), _tiles.tiles.GetLength(0), _tiles.tiles.GetLength(1));
        HashSet<Node> closed = new HashSet<Node>();

        //Initalize and start loop
        FNode minNode = open.PopMin();
        while(minNode != null)
        {
            closed.Add(minNode.node);
            //Stop if target is reached
            if (minNode.node == target)
                break;

            //Loop over all edges
            foreach( Node aEdge in minNode.node.edges)
            {
                //Don't process and edge if its closed, or the edge is in _toAvoid
                if (!closed.Contains(aEdge) && UseEdge(_toAvoid, _toAvoidFirstStep, aEdge.P))
                {
                    //Caluclate true distance
                    int g = Node.DistanceBetween(_tiles, minNode.node, aEdge) + minNode.G;
                    //Create new FNode. This will calulcate H based on this the target, and any additional weight provided by WeightOfMove. Once we have H, F will then be caluculated from g and h
                    FNode aFNode = new FNode(aEdge, g, target, Node.WeightOfMove(_tiles, minNode.node, aEdge));
                    //Process node will either add the node to open if its new, or Update the F value is the new F values is less than the old one. It will then return true if an action is taken
                    if (open.ProcessNode(aFNode))
                        aFNode.prev = minNode;
                }
            }

            //Iterate the loop
            _toAvoidFirstStep = null;
            minNode = open.PopMin();
        }

        //We could not find a path.
        if (minNode == null || minNode.node != target)
        {
            Debug.Log("Can't calculated path between " + _start + " and " + _end + " because no path could be found.");
            return null;
        }

        //Inialize the new path and iterator
        CoordLinked currentPath = new CoordLinked();
        FNode aNode = minNode;

        //Create our path by stepping through the prev chain
        while (aNode != null)
        {
            if(aNode.node.P != _start)
                currentPath.Add(aNode.node);
            aNode = aNode.prev;
        };

        timer.Stop();
        if (GameManager.instance.ShowDebug)
        {
            Debug.Log("A path of " + currentPath.Length + " length calculated in " + timer.ElapsedTicks + " ticks. " + closed.Count + " nodes of a total " + _tiles.graph.Length + " nodes visisted.");
            Debug.Log(currentPath.ToString());
        }

        return currentPath;
    }

    private CoordLinked CalculatePathD( Tiles _tiles, Coord _start, Coord _end, HashSet<Coord> _toAvoid )
    {
        Stopwatch timer = Stopwatch.StartNew();

        //Make sure both coords are valid
        if (!UDF.Between(_start.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_start.Y, 0, _tiles.tiles.GetLength(1))
            || !UDF.Between(_end.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_end.Y, 0, _tiles.tiles.GetLength(1)))
        {
            Debug.Log("Can't calculated path between " + _start  + " and " + _end + " because one or more points are off the passed tiles objects");
            return null;
        }

        Node source = _tiles.GetNode(_start);
        Node target = _tiles.GetNode(_end);

        // The list of nodes we haven't checked yet.
        DNodeList open = new DNodeList(new DNode(source,0), _tiles.tiles.GetLength(0), _tiles.tiles.GetLength(1));
        HashSet<Node> closed = new HashSet<Node>();

        DNode aNode = open.PopMin();
        while (aNode != null)
        {
            closed.Add(aNode.node);
            //If we've reached the target, stop
            if (aNode.node == target)
                break;  // Exit the while loop!

            //Loop over all the edges in this node
            foreach (Node aEdge in aNode.node.edges)
            {
                //Don't process and edge if its closed, or the edge is in _toAvoid
                if (!closed.Contains(aEdge) && UseEdge(_toAvoid, null, aEdge.P))
                {
                    //Calculate the distnace and add it to the distance to get here
                    int dist = aNode.dist + Node.DistanceBetween(_tiles, aNode.node, aEdge);
                    DNode aDEdge = new DNode(aEdge, dist);

                    //If the total distance here is less than the distance stored (remember these all start as infinate) than store this as the new best path
                    if (open.ProcessNode(aDEdge))
                        aDEdge.prev = aNode;
                }
            }

            aNode = open.PopMin();
        }

        //We could not find a path.
        if (aNode == null || aNode.prev == null)
        {
            Debug.Log("Can't calculated path between " + _start + " and " + _end + " because no path could be found.");
            return null;
        }

        //Inialize the new path and iterator
        CoordLinked currentPath = new CoordLinked();
        DNode pathNode = aNode;

        //Create our path by stepping through the prev chain
        while (pathNode != null)
        {
            if (aNode.node.P != _start)
                currentPath.Add(pathNode.node);
            pathNode = pathNode.prev;
        };

        timer.Stop();
        if (GameManager.instance.ShowDebug)
        {
            Debug.Log("A path of " + currentPath.Length + " length calculated in " + timer.ElapsedTicks + " ticks. " + closed.Count + " nodes of a total " + _tiles.graph.Length + " nodes visisted.");
            Debug.Log(currentPath.ToString());
        }

        return currentPath;
    }

    private static bool UseEdge( HashSet<Coord> _toAvoid, HashSet<Coord> _toAvoidFirstStep, Coord _edge )
    {
        if (_toAvoid == null && _toAvoidFirstStep == null)
            return true;
        return ( _toAvoid == null || !_toAvoid.Contains(_edge) ) && ( _toAvoidFirstStep == null || !_toAvoidFirstStep.Contains(_edge) );
    }
}
