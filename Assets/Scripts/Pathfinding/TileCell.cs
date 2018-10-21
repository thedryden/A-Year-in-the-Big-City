using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TileCell {
    /// <summary>
    /// P for point. Contains my location in the tiles object.
    /// </summary>
    public Coord P { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    private TileType _type;
    public TileType Type {
        get {
            return (typeOverrideOn) ? TypeOverride : _type;
        }
        set { _type = value; }
    }
    private TileType _typeOverride;
    public TileType TypeOverride {
        get { return _typeOverride; }
        set {
            _typeOverride = value;
            typeOverrideOn = true;
        }
    }
    private bool typeOverrideOn = false;
    public bool Placeholder { get; private set; }

    public enum TileType { Walkable, Difficult, Blocking, NotYetDefined }
    public static Dictionary<TileType, int> TileTypeRank = new Dictionary<TileType, int>
    {
        { TileType.Blocking, 1 },
        { TileType.Difficult, 2 },
        { TileType.Walkable, 3 },
        { TileType.NotYetDefined, 100 }
    };

    public TileCell(Coord _p, Vector3 _worldPosition, TileType _tagType, bool _placeHolder)
    {
        P = _p;
        WorldPosition = _worldPosition;
        Type = _tagType;
        Placeholder = _placeHolder;
    }

    public TileCell(Coord _p, Vector3 _worldPosition, TileType _tagType)
        : this(_p, _worldPosition, _tagType, false)
    {
        //Do Nothing
    }

    public void RevertOverride()
    {
        typeOverrideOn = false;
    }

    public static TileType TagToTileType(string _in)
    {
        if (_in == "Blocking")
            return TileType.Blocking;
        else if (_in == "Difficult")
            return TileType.Difficult;
        else
            return TileType.Walkable;
    }
}
