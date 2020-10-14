/// <summary>
/// The represents a single item in the MinDist linked list. 
/// It holds a node, and a referance to the next and previous node along with a total distance to this node.
/// </summary>
public class DNodeLinked
{
    /// <summary>
    /// Link to prev and next node in the sequence
    /// </summary>
    public DNodeLinked prev, next;
    /// <summary>
    /// The node we care about
    /// </summary>
    public DNode node;

    /// <summary>
    /// Create a new DNodeLinked with null prev and next
    /// </summary>
    /// <param name="_node">The node to store</param>
    /// <param name="_dist">The total distance to this node form the origin</param>
    public DNodeLinked(DNode _node)
    {
        node = _node;
        prev = null;
        next = null;
    }

    /// <summary>
    /// Creates a new DNodeLinked with the _prev and _next values already set
    /// </summary>
    /// <param name="_node">The node to store</param>
    /// <param name="_prev">The previous DNodeLinked in the list</param>
    /// <param name="_next">The next DNodeLinked in the list</param>
    /// <param name="_dist">The total distance to this node form the origin</param>
    public DNodeLinked(DNode _node, DNodeLinked _prev, DNodeLinked _next)
    {
        node = _node;
        prev = _prev;
        next = _next;
    }
}