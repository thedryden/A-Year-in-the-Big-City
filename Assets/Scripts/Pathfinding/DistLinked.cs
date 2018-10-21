/// <summary>
/// The represents a single item in the MinDist linked list. 
/// It holds a node, and a referance to the next and previous node along with a total distance to this node.
/// </summary>
public class DistLinked
{
    /// <summary>
    /// Link to prev and next node in the sequence
    /// </summary>
    public DistLinked prev, next;
    /// <summary>
    /// Total distance to this node from the origin
    /// </summary>
    public float dist;
    /// <summary>
    /// The node we care about
    /// </summary>
    public Node node;

    /// <summary>
    /// Create a new DistLinked with null prev and next
    /// </summary>
    /// <param name="_node">The node to store</param>
    /// <param name="_dist">The total distance to this node form the origin</param>
    public DistLinked(Node _node, float _dist)
    {
        node = _node;
        prev = null;
        next = null;
        dist = _dist;
    }

    /// <summary>
    /// Creates a new DistLinked with the _prev and _next values already set
    /// </summary>
    /// <param name="_node">The node to store</param>
    /// <param name="_prev">The previous DistLinked in the list</param>
    /// <param name="_next">The next DistLinked in the list</param>
    /// <param name="_dist">The total distance to this node form the origin</param>
    public DistLinked(Node _node, DistLinked _prev, DistLinked _next, float _dist)
    {
        node = _node;
        prev = _prev;
        next = _next;
        dist = _dist;
    }
}