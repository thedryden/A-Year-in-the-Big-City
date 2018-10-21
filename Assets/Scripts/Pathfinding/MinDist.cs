using UnityEngine;

/// <summary>
/// Custom implmentation of a linked list sorted on distance.
/// </summary>
public class MinDist {
    /// <summary>
    /// Start of the list. The DistLink with the minDist
    /// </summary>
    private DistLinked head;
    /// <summary>
    /// Last item of the list. The DistLinked with the maxDist
    /// </summary>
    private DistLinked foot;
    /// <summary>
    /// The number of items in the list
    /// </summary>
    public int Length { get; private set; }
    /// <summary>
    /// Array that allows O(1) lookup of any node that is in the list if you know its x and y coord (which are stored in the node)
    /// </summary>
    private DistLinked[,] map; //Array that allows quick lookup based on x,y. x,y will match the locations in graph

    /// <summary>
    /// Creates a new linked list, with a sinlge item, and also initalizes map based on the sizes _maxX and _maxY
    /// </summary>
    /// <param name="_node">New head/foot to intailze the list</param>
    /// <param name="_dist">Distance value of the new head / foot</param>
    /// <param name="_maxX">number of columns in the map array</param>
    /// <param name="_maxY">number of rows in the map array</param>
    public MinDist(Node _node, float _dist, int _maxX, int _maxY) {
        head = new DistLinked(_node, _dist);
        foot = head;
        Length = 1;
        map = new DistLinked[_maxX, _maxY];
        map[_node.P.X, _node.P.Y] = head;
    }

    /// <summary>
    /// Adds a new node to the list, making sure its in the proper sorted order based on the past dist
    /// </summary>
    /// <param name="_node">The node to add</param>
    /// <param name="_dist">The distance from the origin of this node, the node will be placed in sorted order based on this value</param>
    public void Add(Node _node, float _dist) {
        DistLinked aDist = head;
        DistLinked newDist = null;

        //Loop in order, starting from head
        while (aDist != null) {
            //If the new dist is equal to or less than then the value of the current node, insert the new node before the current item
            if (aDist.dist >= _dist) {
                //Create node
                newDist = new DistLinked(_node, aDist.prev, aDist, _dist);
                //If we're creating the new head
                if (newDist.prev == null)
                    head = newDist;
                else
                    aDist.prev.next = newDist;
                aDist.prev = newDist;
                break;
            }
            aDist = aDist.next;
        }
        //If this is the new max
        if (aDist == null) {
            newDist = new DistLinked(_node, foot, null, _dist);
            foot.next = newDist;
            foot = newDist;
        }
        map[_node.P.X, _node.P.Y] = newDist;
        Length++;
    }

    /// <summary>
    /// Adds a new node as the new foot with an Mathf.Infinate dist. This was created to make the process of initalizing the list with a bunch of infinate dist values faster O(1) vs O(2).
    /// </summary>
    /// <param name="_node">Node to add</param>
    public void AddToFoot( Node _node ) {
        //Create new DistLinked and add it as the foot, moving the existing foot down one position
        DistLinked newDist = new DistLinked(_node, foot, null, Mathf.Infinity);
        foot.next = newDist;
        foot = newDist;
        map[_node.P.X, _node.P.Y] = newDist;
        Length++;
    }

    /// <summary>
    /// Returns the min node (head)
    /// </summary>
    /// <returns>The min node (head)</returns>
    public Node GetMin() {
        return head.node;
    }

    /// <summary>
    /// Removes the min node (head)
    /// </summary>
    public void RemoveMin() {
        head = head.next;
        head.prev = null;
        Length--;
    }

    /// <summary>
    /// Returns the min node (head) and then removes it.
    /// </summary>
    /// <returns>The min node (head)</returns>
    public Node PopMin() {
        if (head == null)
            return null;
        Node output = head.node;
        head = head.next;
        if(head != null)
            head.prev = null;
        Length--;
        return output;
    }

    /// <summary>
    /// Takes a node, that must be in this list, and changes its distance and then readds it to the list to update the sort order.
    /// </summary>
    /// <param name="_node">The node who's distance you wish to udpate</param>
    /// <param name="_dist">The new distance</param>
    public void ChangeDistAt(Node _node, float _dist) {
        //If node is null do nothing
        if (_node == null)
            return;
        //If node is not in the map, do nothing
        DistLinked node = map[_node.P.X, _node.P.Y];
        if (node == null)
            return;


        /* General logic is to cut this node out, then add it back in.
         * Possiblites: I'm head and foot, set net _dist and done.
        * I'm head but not foot, set next to head and remove prev from head
        * I'm foot but not head, set prev to foot and remove next for foot
        * I'm neither head nor foot. set prev of my next to my prev, set next of my prev to my next
        */
        if (node.prev == null && node.next == null)
        {
            node.dist = _dist;
            return;
        }
        else if (node.prev == null)
        {
            head = node.next;
            head.prev = null;
        }
        else if (node.next == null)
        {
            foot = node.prev;
            foot.next = null;
        }
        else
        {
            node.prev.next = node.next;
            node.next.prev = node.prev;
        }
        Length--;
        Add(node.node, _dist);
    }
}