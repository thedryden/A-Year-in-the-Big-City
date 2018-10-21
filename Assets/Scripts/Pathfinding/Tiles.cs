using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// In code representation of a Grid and its tiles.
/// </summary>
public class Tiles
{
    /// <summary>
    /// Array 2d array of TileCell each of which represent a tile in a grid
    /// </summary>
    public TileCell[,] tiles;
    /// <summary>
    /// GameObject grid this represents
    /// </summary>
    public Grid myGrid;
    /// <summary>
    /// This is the graph used by pathfinding. While it is related to the tiles array, it has different properties designed for pathfinding
    /// </summary>
    public Node[,] graph;
    /// <summary>
    /// Holds the tiles actors are currently moving through. This is used for local avoidance, making sure actors don't bump into one aother.
    /// </summary>
    public int[,] movingThrough { get; private set; }
    /// <summary>
    /// Stores min and max x and y corordinantes in world space.
    /// </summary>
    private float minWorldX = float.MaxValue, maxWorldX = float.MinValue
        , minWorldY = float.MaxValue, maxWorldY = float.MinValue;
    /// <summary>
    /// An array of the world position of each x position and y position in the tiles array. 
    /// Since not all position in tiles have a tile in them, and positions without tiles don't have an worldposition, this is used to quickly locate the x and y coordinates of a world position.
    /// </summary>
    private float[] worldX, worldY;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="_grid">The grid you wish to base this Tiles obejct on</param>
    public Tiles( Grid _grid )
    {
        //See each function to know what it does.
        myGrid = _grid;
        GenerateTiles();
        GenerateGraph();
        GenerateMovingThrough();
    }

    /// <summary>
    /// Populates the tiles object as well as min/maxWorldX/Y and worldX/Y
    /// </summary>
    private void GenerateTiles()
    {
        //Temporary place to hold cells while we work
        List<TileCell> allCells = new List<TileCell>();
        //Used to find the min and max locatoins on the grid, which will more or less corispond to locations in the eventual array
        int minX = 2147483647, maxX = -2147483648, minY = 2147483647, maxY = -2147483648;

        //Loop through all tilemaps in the grid
        foreach (Tilemap aMap in myGrid.GetComponentsInChildren<Tilemap>())
        {
            //Set min and maxes based on the bounds of this tileMap
            minX = UDF.Min(aMap.cellBounds.xMin, minX);
            maxX = UDF.Max(aMap.cellBounds.xMax, maxX);
            minY = UDF.Min(aMap.cellBounds.yMin, minY);
            maxY = UDF.Max(aMap.cellBounds.yMax, maxY);

            //Loop over the tiles in the tileMap
            for (int x = aMap.cellBounds.xMin; x < aMap.cellBounds.xMax; x++)
            {
                for (int y = aMap.cellBounds.yMin; y < aMap.cellBounds.yMax; y++)
                {
                    //Get the world position of the tile
                    Vector3Int localPlace = (new Vector3Int(x, y, (int)aMap.transform.position.y));
                    Vector3 place = aMap.CellToWorld(localPlace);

                    //Set min/maxWorldX/Y positions based on the world position
                    minWorldX = UDF.Min(place.x, minWorldX);
                    maxWorldX = UDF.Max(place.x, maxWorldX);
                    minWorldY = UDF.Min(place.y, minWorldY);
                    maxWorldY = UDF.Max(place.y, maxWorldY);

                    //If theis is actually a tile here, place it in allCells
                    if (aMap.HasTile(localPlace))
                        allCells.Add(new TileCell(new Coord(x,y,false), place, TileCell.TagToTileType(aMap.tag)));
                }
            }
        }

        //Now that we know how big to make them, create the tiles and worldX/Y arrays
        tiles = new TileCell[maxX - minX, maxY - minY];
        worldX = new float[maxX - minX];
        worldY = new float[maxY - minY];
        //Initalize array worldX/Y arrays
        for (int i = 0; i < worldX.Length; i++)
            worldX[i] = float.MinValue;
        for (int i = 0; i < worldY.Length; i++)
            worldY[i] = float.MinValue;

        //Calculate an offset to tranlate the x/y in allCells to the x/y in tiles
        int xOffset = 0 - minX;
        int yOffset = 0 - minY;

        foreach (TileCell aCell in allCells)
        {
            //Calculate the x and y position of the cell in tiles, and set it in the cell
            Coord p = aCell.P;
            p.X = aCell.P.X + xOffset;
            p.Y = (maxY - minY) - (aCell.P.Y + yOffset);//Y axis is flipped to match world position of tiles
            //Now that we're adjusted to the final postion make it readOnly
            p.SetReadOnly();

            //If the position is empty set it in tiles and worldX/Y
            if (GetTile(p) == null)
            {
                tiles[p.X, p.Y] = aCell;
                if(minWorldX > worldX[p.X])
                    worldX[p.X] = GetTile(p).WorldPosition.x;
                if(minWorldY > worldY[p.Y])
                    worldY[p.Y] = GetTile(p).WorldPosition.y;
            }
            //If it already exists overwrite it only if this tileType has a precednace of the last type. For instance a type of Blocking has a higher precidence than walkable.
            else if (TileCell.TileTypeRank[GetTile(p).Type] > TileCell.TileTypeRank[aCell.Type])
                tiles[p.X, p.Y] = aCell;
        }

        //Loop over all positions in tiles and set looking for empty cells. When found insert a "placeholder" tile that is Blocking, but does not have a world position.
        for (int x = 0; x < tiles.GetLength(0); x++)
            for (int y = 0; y < tiles.GetLength(1); y++)
                if (tiles[x, y] == null)
                    tiles[x, y] = new TileCell(new Coord(x,y), Vector3Int.zero, TileCell.TileType.Blocking, true);
    }

    /// <summary>
    /// Generates a graph of Nodes for pathfinding. This code required the tiles array to be set to work
    /// </summary>
    private void GenerateGraph()
    {
        //Initalize graph
        graph = new Node[tiles.GetLength(0),tiles.GetLength(1)];
        for (int x = 0; x < graph.GetLength(0); x++)
            for (int y = 0; y < graph.GetLength(1); y++)
                graph[x, y] = new Node(x, y);
        //Set the edges of every position in the graph
        for (int x = 0; x < graph.GetLength(0); x++)
            for(int y = 0; y < graph.GetLength(1); y++)
                SetEdges(new Coord(x,y));
    }

    /// <summary>
    /// Sets the edges of the node in the array graph at the passed position
    /// </summary>
    /// <param name="_x">Column of the node</param>
    /// <param name="_y">Row of the node</param>
    public void SetEdges(Coord _p)
    {
        //Clear the edges
        graph[_p.X, _p.Y].edges.Clear();
        //If this node is blocking, it has no edges.
        if (tiles[_p.X, _p.Y].Type == TileCell.TileType.Blocking)
            return;
        //Loop through all 8 positions surounding the graph
        for (int x = _p.X- 1; x <= _p .X+ 1; x++)
        {
            for (int y = _p.Y - 1; y <= _p.Y + 1; y++)
            {
                //if we're at self increase y by 1
                if (x == _p.X && y == _p.Y)
                    y++;

                Coord aCoord = new Coord(x, y);
                TileCell aCell = GetTile(aCoord);

                //If the x and y coordinates are in the array, and that the position isn't blocking
                if (aCell != null && aCell.Type != TileCell.TileType.Blocking)
                    //Add the edge if this is either not diagonal (x or y is is same as self) or this is a diagonal move, and we're not cutting through a blocked square: i.e. the neither of the adjecent non-diagonal paths aren't blocking
                    if (x == _p.X || y == _p.Y || (tiles[_p.X, y].Type != TileCell.TileType.Blocking && tiles[x, _p.Y].Type != TileCell.TileType.Blocking))
                        GetNode(_p).edges.Add(GetNode(aCoord));
            }
        }
    }

    /// <summary>
    /// Sets the edges of node at the passed position, as well as the edges of all nodes adjacent to the passed node. Used to update nodes if the type of this cell is changed, i.e. a walkable cell become blocking.
    /// </summary>
    /// <param name="_x">Column of the node</param>
    /// <param name="_y">Row of the node</param>
    public void SetEdgesOfEdges(Coord _p)
    {
        //Loop over surounding positions and self.
        for (int x = _p.X - 1; x <= _p.X + 1; x++)
        {
            for (int y = _p.Y - 1; y <= _p.Y + 1; y++)
            {
                //Make sure the position is in the array
                if (UDF.Between(x, 0, tiles.GetLength(0) - 1) && UDF.Between(y, 0, tiles.GetLength(1) - 1))
                    SetEdges(new Coord(x, y));
            }
        };
    }

    /// <summary>
    /// Generates the moving through array. This code required the tiles array to be set to work
    /// </summary>
    private void GenerateMovingThrough()
    {
        //Initalize graph
        movingThrough = new int[tiles.GetLength(0), tiles.GetLength(1)];
        for (int x = 0; x < graph.GetLength(0); x++)
            for (int y = 0; y < graph.GetLength(1); y++)
                movingThrough[x, y] = 0;
    }

    /// <summary>
    /// Returns the Node at the position defined by the passed Coord. If the passed position isn't in Graph then null will be returned.
    /// </summary>
    /// <param name="_p">A Coord the represents a postions in teh 2D array graph</param>
    /// <returns>The graph at the position _p, or null if _p isn't in Graph</returns>
    public Node GetNode(Coord _p)
    {
        if (!UDF.Between(_p.X, 0, graph.GetLength(0) - 1) || !UDF.Between(_p.Y, 0, graph.GetLength(1) - 1))
            return null;
        return graph[_p.X, _p.Y];
    }

    /// <summary>
    /// Returns the TileCell at the position defined by the passed Coord. If the passed position isn't in Tiles then null will be returned.
    /// </summary>
    /// <param name="_p">A Coord the represents a postions in teh 2D array graph</param>
    /// <returns>The TileCell at the position _p, or null if _p isn't in Tiles</returns>
    public TileCell GetTile(Coord _p)
    {
        if (!UDF.Between(_p.X, 0, tiles.GetLength(0) - 1) || !UDF.Between(_p.Y, 0, tiles.GetLength(1) - 1))
            return null;
        return tiles[_p.X, _p.Y];
    }

    /// <summary>
    /// Takes a world position and find the cell that world position is (_x and _y) and also returns an idealPosition.
    /// The ideal position is a worldposition vector3 that represents the center of cell returned by _x and _y.
    /// If you pass a position that is not within this object this function will return false.
    /// If you pass a position that is in a blocked cell, this function will search around that cell to find an unblocked one and return the information for that.
    /// </summary>
    /// <param name="_worldPosition">Position to check</param>
    /// <param name="_x">Out parameter, returns the row position in the tiles array</param>
    /// <param name="_y">Out parameter, returns the column position in the tiles array</param>
    /// <param name="_idealPosition">A worldposition vector3 that represents the center of cell returned by _x and _y.</param>
    /// <returns>If true a location was found, if false the passed location is not in this tiles object</returns>
    public bool WorldPositionToTile(Vector3 _worldPosition, out Coord _p, out Vector3 _idealPosition )
    {
        return WorldPositionToTile(_worldPosition, out _p, out _idealPosition, false);
    }

    /// <summary>
    /// Takes a world position and find the cell that world position is (_x and _y) and also returns an idealPosition.
    /// The ideal position is a worldposition vector3 that represents the center of cell returned by _x and _y.
    /// If you pass a position that is not within this object this function will return false.
    /// If ignoreBlocking is set to false and you pass a position that is in a blocking cell, this function will search around that cell to find an unblocked one and return the information for that.
    /// If ignoreBlocking is set to true this function will return a blocking cell, but if the position you pass is a placeholder cell, this function will search around that cell to find a cell that is not a placeholder and return the information for that.
    /// </summary>
    /// <param name="_worldPosition">Position to check</param>
    /// <param name="_x">Out parameter, returns the row position in the tiles array</param>
    /// <param name="_y">Out parameter, returns the column position in the tiles array</param>
    /// <param name="_idealPosition">A worldposition vector3 that represents the center of cell returned by _x and _y.</param>
    /// <returns>If true a location was found, if false the passed location is not in this tiles object</returns>
    public bool WorldPositionToTile( Vector3 _worldPosition, out Coord _p, out Vector3 _idealPosition, bool _ignoreBlocking )
    {
        //Set up out parameters
        _p = Coord.Zero();
        _idealPosition = _worldPosition;
        float tileSize = GameManager.instance.tileSize;

        //Check to see if worldposition is in this tile
        if (!UDF.Between(_worldPosition.x, minWorldX, maxWorldX) || !UDF.Between(_worldPosition.y, minWorldY, maxWorldY))
            return false;

        //Do some math to get our best guess for _x and _y
        _p.X = (int)((_worldPosition.x - minWorldX) / tileSize);
        _p.Y = (int)((_worldPosition.y - minWorldY) / tileSize);

        int iterations = 0, direction = 0;
        while (iterations < worldX.Length && !UDF.Between( _worldPosition.x, worldX[_p.X], worldX[_p.X] + tileSize))
        {
            //Decide which direction to move _x
            int newDirection = (_worldPosition.x > worldX[_p.X]) ? -1 : 1;

            //If we're reversed directions it means the tile we're looking for is a place holder.
            if (direction != 0 && direction != newDirection)
                break;
            _p.X += newDirection;
            //If we've stubmbled off the grid, return false
            if (!UDF.Between(_p.X, 0, tiles.GetLength(0) - 1))
                return false;
            direction = newDirection;
            iterations++;
        }

        iterations = 0;
        direction = 0;
        while (iterations < worldY.Length && !UDF.Between(_worldPosition.y, worldY[_p.Y], worldY[_p.Y] + tileSize))
        {
            //Decide which direction to move _y
            int newDirection = (_worldPosition.y > worldY[_p.Y]) ? -1 : 1;

            //If we're reversed directions it means the tile we're looking for is a place holder.
            if (direction != 0 && direction != newDirection)
                break;
            //If we've stubmbled off the grid, return false
            if (!UDF.Between( _p.Y, 0, tiles.GetLength(1)-1))
                return false;
            _p.Y += newDirection;
            direction = newDirection;
            iterations++;
        }
        //If the found tile isn't blocking or a placerholder export it
        if((_ignoreBlocking || GetTile(_p).Type != TileCell.TileType.Blocking) && !GetTile(_p).Placeholder)
        {
            _idealPosition = GetTile(_p).WorldPosition + GameManager.instance.TileOffset;
            return true;
        }
        //Search for a non-blocked cell up to 10 tiles away.
        for (int i = 0; i < 10; i++)
        {
            for(int x = -1; x <= 1; x++)
            {
                for(int y = -1; y <= 1; y++)
                {
                    Coord temp = _p + new Coord( (x * i), ( y * i ), false );
                    if (UDF.Between(temp.X,0,tiles.GetLength(0)-1) && UDF.Between(temp.Y,0,tiles.GetLength(1)-1) && (_ignoreBlocking || GetTile(temp).Type != TileCell.TileType.Blocking) && !GetTile(temp).Placeholder)
                    {
                        _p = temp;
                        _idealPosition = GetTile(_p).WorldPosition + GameManager.instance.TileOffset;
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public void TilesToConsole()
    {
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            string output = "row " + y + ": ";
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                if (x > 0)
                    output += ", ";
                if (tiles[x, y].Type == TileCell.TileType.Walkable)
                    output += "W";
                else if (tiles[x, y].Type == TileCell.TileType.Difficult)
                    output += "D";
                else if (tiles[x, y].Type == TileCell.TileType.Blocking)
                    output += "B";
            }
            Debug.Log(output);
        }
    }

    public void PositionToConsole()
    {
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            string output = "row " + y + ": ";
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                output += tiles[x, y].WorldPosition;
            }
            Debug.Log(output);
        }
    }

    public void EdgeCountToConsole()
    {
        for (int y = 0; y < tiles.GetLength(1); y++)
        {
            string output = "row " + y + ": ";
            for (int x = 0; x < tiles.GetLength(0); x++)
            {
                output += ", " + graph[x, y].edges.Count;
            }
            Debug.Log(output);
        }
    }

    
}
