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
    /// <summary>
    /// An array containing a Tiles object for each grid in the scene
    /// </summary>
    public Tiles[] tiles;
    /// <summary>
    /// The index in tiles array for the last tiles object that used by this class
    /// </summary>
    private int activeTile;

	/// <summary>
    /// Initalizes the class. This is done in Awake because it must always happen before Actor are started
    /// </summary>
	void Awake () {
        GetGrids();
        paths = new Dictionary<string, CoordLinked>();
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
        Tiles aTiles = _actor.myMovement.MyTiles;
        if (aTiles == null)
            return;
        Coord actorCoord = _actor.myMovement.TileCoord;

        //Get current type of tile, revert tileOverride, if the new tile type is the same as the old tile type we're done
        TileCell.TileType startType = aTiles.GetTile(actorCoord).Type;
        aTiles.GetTile(actorCoord).RevertOverride();
        if (startType == aTiles.GetTile(actorCoord).Type)
            return;

        //If the tile type is differnt update the graph
        aTiles.SetEdgesOfEdges(actorCoord);

        //Since an non moving actor could block an entire path, all paths are recalculated.
        foreach( Actor aActor in GameManager.instance.Actors)
        {
            //Don't recalculate this actors path, and don't recalculat if the actor doens't already have a path
            if (_actor.ID != aActor.ID && paths.ContainsKey(aActor.ID)){
                //Get destination of the path, and recalculate
                Coord aFoot = paths[aActor.ID].GetFoot();
                Vector3 end = aTiles.GetTile(aFoot).WorldPosition;
                MoveActorTo(aActor, end);
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

        CoordLinked aPath = paths[_actor.ID];

        //If this actor no longer has a path, stop the actor
        if (aPath == null)
        {
            ActorStopped(_actor);
            return;
        }

        Coord next = aPath.Pop();
            
        //If the actor's path is empty stop the actor and remove the path
        if(next == null)
        {
            paths.Remove(_actor.ID);
            ActorStopped(_actor);
            return;
        }
        
        //If the requested "move" is to the position the actor is already at, call this againt to get the next node and then do nothing
        if (next == _actor.myMovement.TileCoord)
        {
            AdvancePath(_actor);
            return;
        }

        //Check for colision with actor
        int mt = tiles.GetMovingThrough(next);
        /* Posibilities, in any drawings we are U is us and S is collisions start point and E is collisions end point
         * 1) mt is 0, nothing there no collision
         * 2) We are traveling in the same direction. We if so we need to lookup the speed of the actor we just bumped into. If we're faster the we recalc the with _toAvoid on the next 3 tiles dead ahead.
         * 3) T-Bone. If we are T-Boning in start from location, then simply wait. If we T-Boned in against the destination and moving towards the start puts us close to the destination then create a new path that start with a move in that direction followed by a recalc.
         *  SE
         *   U move if our desitnation is <- otherwise wait
         * 4) Head on collision since we got here second we're always the victum. Recalculate with toAvoid on the two blocked tiles
         */

        if (mt != 0)
        {
            Movement.Directions myDirection = Movement.CoordsToDirection(_actor.myMovement.TileCoord, next);
            bool hitStart = true;
            bool wait = false;
            Movement.Directions theirDirection = 0;
            if (Movement.MovingThroughIntDirection.ContainsKey(mt))
                theirDirection = Movement.MovingThroughIntDirection[mt];
            else if (Movement.MovingThroughIntDirection.ContainsKey(mt - 1)) {
                theirDirection = Movement.MovingThroughIntDirection[mt - 1];
                hitStart = false;
            }
            else //If we can't figure out the direction just wait and hopefully it will resolve itself.
                wait = true;
            if(myDirection == theirDirection)
            {
                Actor aActor = tiles.GetActorMovingThrough(next);
                if (_actor.speed >= aActor.speed)
                    //Recal path but set toAvoid the next 3 steps
                else
                    wait = true;
            }
                
        }

        //Determine if this is the last part of the path, if it is set moveSlow to true
        bool moveSlow = (aPath.Length == 0);

        //Move the actor
        _actor.myMovement.MoveOne(next, moveSlow);
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
            //If the desitination isn't on the same tiles, we're done
            return;

        MoveActorTo(_actor, destCord);
    }

    /// <summary>
    /// Moves an actor to the specified location by creating a path, and then starts the process of walking that path moving from one node to the next one at a time.
    /// </summary>
    /// <param name="_id">The actor you wish to move</param>
    /// <param name="_destination">The world position you wish to move the actor to</param>
    public void MoveActorTo(Actor _actor, Coord _destination)
    {
        //Determine if the actor is already moving
        bool alreadyMoving = paths.ContainsKey(_actor.ID);

        Tiles tiles = _actor.myMovement.MyTiles;
        //Remove any existing path. Removing this will effective abort the current path. Resulting in either the actor walking our new path, or finishing their current tile to tile move and stopping normally if we couldn't create a new one.
        paths.Remove(_actor.ID);

        //If we're not already moving we need to set the actor as moving before we calculate the path otherwise they'll be standing on a blocking tiles and no path can be found.
        if (!alreadyMoving)
            ActorMoving(_actor);
        CoordLinked newPath = CalculatePath(tiles, _actor.myMovement.TileCoord, _destination);
        if (newPath == null)
        {
            //If already moving let the actor come to a stop first, if not stop it to remove it from the graph
            if(!alreadyMoving)
                ActorStopped(_actor);
            return;
        }

        //Store the path
        paths.Add(_actor.ID, newPath);

        //Draw path for debugging
        Vector3 offset = new Vector3(GameManager.instance.tileSize / 2, GameManager.instance.tileSize / 2, GameManager.instance.tileSize / 2);
        Coord aCoord = newPath.StartIteration();
        while (aCoord != null)
        {
            Vector3 start = tiles.GetTile(aCoord).WorldPosition + offset;
            aCoord = newPath.Next();
            if (aCoord == null)
                break;
            Vector3 end = tiles.GetTile(aCoord).WorldPosition + offset;
            Debug.DrawLine(start, end, Color.red, 10, false);
        }

        //If not already moving start moving, if already moving cancel the path so we will start on this new path at the next frame
        if (!alreadyMoving)
            AdvancePath(_actor);
        else
            _actor.myMovement.CancelMove();
    }

    private CoordLinked CalculatePath(Tiles _tiles, Coord _start, Coord _end)
    {
        return CalculatePath(_tiles, _start, _end, null);
    }

    private CoordLinked CalculatePath( Tiles _tiles, Coord _start, Coord _end, List<Coord> _toAvoid )
    {
        Stopwatch timer = Stopwatch.StartNew();

        //Make sure both coords are valid
        if (!UDF.Between(_start.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_start.Y, 0, _tiles.tiles.GetLength(1))
            || !UDF.Between(_end.X, 0, _tiles.tiles.GetLength(0)) || !UDF.Between(_end.Y, 0, _tiles.tiles.GetLength(1)))
        {
            Debug.Log("Can't calculated path between " + _start  + " and " + _end + " because one or more points are off the passed tiles objects");
            return null;
        }

        Dictionary<Node, float> dist = new Dictionary<Node, float>();
        Dictionary<Node, Node> prev = new Dictionary<Node, Node>();

        Node source = _tiles.graph[_start.X, _start.Y];
        Node target = _tiles.graph[_end.X, _end.Y];

        // Setup the "Q" -- the list of nodes we haven't checked yet.
        MinDist unvisited = new MinDist(source, 0, _tiles.tiles.GetLength(0), _tiles.tiles.GetLength(1));

        dist[source] = 0;
        prev[source] = null;

        // Initialize everything to have INFINITY distance, since
        // we don't know any better right now. Also, it's possible
        // that some nodes CAN'T be reached from the source,
        // which would make INFINITY a reasonable value
        foreach (Node n in _tiles.graph)
        {
            if (n != source)
            {
                dist[n] = Mathf.Infinity;
                prev[n] = null;
            }

            unvisited.AddToFoot(n);
        }

        int totalNodes = unvisited.Length;

        //Loop until all nodes are visited
        while (unvisited.Length > 0)
        {
            //Get the node the min distance, at the start this will always be the source
            Node aUnvisited = unvisited.PopMin();

            //If we've reached the target, stop
            if (aUnvisited == target)
                break;  // Exit the while loop!

            float unDist = dist[aUnvisited];
            //If the min from Unvisited is ever Infinity it means we could not find a path between source and anything useful.
            if (unDist == Mathf.Infinity)
            {
                Debug.Log("Can't calculated path between " + _start + " and " + _end + " because no path could be found because all edges of source were inacessable.");
                return null;
            }

            //Loop over all the edges in this node
            foreach (Node aEdge in aUnvisited.edges)
            {
                bool useEdge = true;
                if (_toAvoid != null)
                {
                    foreach (Coord aCoord in _toAvoid)
                    {
                        if (aEdge.P == aCoord)
                        {
                            useEdge = false;
                            break;
                        }

                    }
                }

                if (useEdge)
                {
                    //Calculate the distnace and add it to the distance to get here
                    float newDistance = unDist + Node.DistanceBetween(_tiles, aUnvisited, aEdge);

                    //If the total distance here is less than the distance stored (remember these all start as infinate) than store this as the new best path
                    if (newDistance < dist[aEdge])
                    {
                        dist[aEdge] = newDistance;
                        prev[aEdge] = aUnvisited;
                        //Change the distance of edge in unvisited to no longer be infinity. This will cause it to eventually be picked by popMin
                        unvisited.ChangeDistAt(aEdge, newDistance);
                    }
                }
            }
        }

        //We could not find a path.
        if (prev[target] == null)
        {
            Debug.Log("Can't calculated path between " + _start + " and " + _end + " because no path could be found.");
            return null;
        }

        //Inialize the new path and iterator
        CoordLinked currentPath = new CoordLinked();
        Node aNode = target;

        //Create our path by stepping through the prev chain
        while (aNode != null)
        {
            currentPath.Add(aNode);
            aNode = prev[aNode];
        };

        timer.Stop();
        if(GameManager.instance.ShowDebug) Debug.Log("A path of " + currentPath.Length + " length calculated in " + timer.ElapsedMilliseconds + " milliseconds. " + ( totalNodes - unvisited.Length ) + " nodes of a total " + totalNodes + " nodes visisted.");

        return currentPath;
    }
}
