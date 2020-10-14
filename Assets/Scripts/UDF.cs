using UnityEngine;

/// <summary>
/// A static class containing a useful collection of simple static functions.
/// </summary>
public static class UDF {
    /// <summary>
    /// Determines if the _value is between _min and _max. It doens't matter if _min is larger than _max the terms will be flipped to still give a true between
    /// </summary>
    /// <param name="_value">The value to be compared</param>
    /// <param name="_min">A starting point to compare to _value</param>
    /// <param name="_max">An end point to compare to _value</param>
    /// <returns>Ture is _value is between _min and _max</returns>
    public static bool Between( float _value, float _min, float _max)
    {
        if( _min > _max)
        {
            float temp = _min;
            _min = _max;
            _max = temp;
        }

        if (_value >= _min && _value <= _max)
            return true;
        return false;
    }

    /// <summary>
    /// Determines if the _value is between _min and _max. It doens't matter if _min is larger than _max the terms will be flipped to still give a true between
    /// </summary>
    /// <param name="_value">The value to be compared</param>
    /// <param name="_min">A starting point to compare to _value</param>
    /// <param name="_max">An end point to compare to _value</param>
    /// <returns>Ture is _value is between _min and _max</returns>
    public static bool Between(int _value, int _min, int _max)
    {
        if (_min > _max)
        {
            int temp = _min;
            _min = _max;
            _max = temp;
        }

        if (_value >= _min && _value <= _max)
            return true;
        return false;
    }

    /// <summary>
    /// Determines if the the _value Vector 3 in inside the square formed by _min and _max. Since this is a 2D game the z axis is ignored.
    /// </summary>
    /// <param name="_value">The value to be compared</param>
    /// <param name="_min">A corner of the square</param>
    /// <param name="_max">The other corner of the square</param>
    /// <returns>Ture is _value is inside the squre formed by _min and _max</returns>
    public static bool Between(Vector3 _value, Vector3 _min, Vector3 _max)
    {
        if (Between(_value.x, _min.x, _max.x) && Between(_value.y,_min.y,_max.y))
            return true;
        return false;
    }

    /// <summary>
    /// Determines if the the Coord _value is inside the square formed by _min and _max.
    /// </summary>
    /// <param name="_value">The value to be compared</param>
    /// <param name="_min">A corner of the square</param>
    /// <param name="_max">The other corner of the square</param>
    /// <returns>Ture is _value is inside the squre formed by _min and _max</returns>
    public static bool Between(Coord _value, Coord _min, Coord _max)
    {
        if (Between(_value.X, _min.X, _max.X) && Between(_value.Y, _min.Y, _max.Y))
            return true;
        return false;
    }

    /// <summary>
    /// Takes two parameters and returns the one that is numerically less.
    /// </summary>
    /// <param name="_value">A value</param>
    /// <param name="_min">Another value</param>
    /// <returns>Returns which ever of two parameters is numeric less.</returns>
    public static float Min( float _value, float _min)
    {
        if (_min > _value)
            return _value;
        return _min;
    }

    /// <summary>
    /// Takes two parameters and returns the one that is numerically less.
    /// </summary>
    /// <param name="_value">A value</param>
    /// <param name="_min">Another value</param>
    /// <returns>Returns which ever of two parameters is numeric less.</returns>
    public static int Min(int _value, int _min)
    {
        if (_min > _value)
            return _value;
        return _min;
    }

    /// <summary>
    /// Takes two parameters and returns the one that is numerically greater.
    /// </summary>
    /// <param name="_value">A value</param>
    /// <param name="_min">Another value</param>
    /// <returns>Returns which ever of two parameters is numeric greater.</returns>
    public static float Max(float _value, float _max)
    {
        if (_max < _value)
            return _value;
        return _max;
    }

    /// <summary>
    /// Takes two parameters and returns the one that is numerically greater.
    /// </summary>
    /// <param name="_value">A value</param>
    /// <param name="_min">Another value</param>
    /// <returns>Returns which ever of two parameters is numeric greater.</returns>
    public static int Max(int _value, int _max)
    {
        if (_max < _value)
            return _value;
        return _max;
    }

    /// <summary>
    /// Retruns the absolute value of an int
    /// </summary>
    /// <param name="_value">in that might be negative</param>
    /// <returns>Absolute value of int</returns>
    public static int Abs(int _value)
    {
        if (_value < 0)
            return _value * -1;
        return _value;
    }

    public static void Swap<T>(ref T lhs, ref T rhs)
    {
        T temp;
        temp = lhs;
        lhs = rhs;
        rhs = temp;
    }
}
