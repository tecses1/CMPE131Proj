namespace Shared;
using System.Drawing;
public class Quadtree {
    private const int MAX_OBJECTS = 4;
    private const int MAX_LEVELS = 5; // Prevents infinite splitting

    private int _level;
    private List<GameObject> _objects;
    private Rect _bounds;
    private Quadtree[] _nodes;

    public Quadtree(int level, Rect bounds) {
        _level = level;
        _objects = new List<GameObject>();
        _bounds = bounds;
        _nodes = new Quadtree[4];
    }

    public void Clear() {
        _objects.Clear();
        for (int i = 0; i < _nodes.Length; i++) {
            if (_nodes[i] != null) {
                _nodes[i].Clear();
                _nodes[i] = null;
            }
        }
    }

private void Split() {
        float subWidth = _bounds.Width / 2f;
        float subHeight = _bounds.Height / 2f;
        
        // Calculate the distance from the parent center to the new sub-centers
        float xOffset = subWidth / 2f;
        float yOffset = subHeight / 2f;

        // Pass the actual CENTER of each new quadrant
        _nodes[0] = new Quadtree(_level + 1, new Rect(_bounds.X + xOffset, _bounds.Y - yOffset, subWidth, subHeight)); // Top Right
        _nodes[1] = new Quadtree(_level + 1, new Rect(_bounds.X - xOffset, _bounds.Y - yOffset, subWidth, subHeight)); // Top Left
        _nodes[2] = new Quadtree(_level + 1, new Rect(_bounds.X - xOffset, _bounds.Y + yOffset, subWidth, subHeight)); // Bottom Left
        _nodes[3] = new Quadtree(_level + 1, new Rect(_bounds.X + xOffset, _bounds.Y + yOffset, subWidth, subHeight)); // Bottom Right
    }

    private int GetIndex(Rect rect) {
        int index = -1;
        
        // Because _bounds is a centered Rect, X and Y ARE the midpoints!
        float verticalMidpoint = _bounds.X; 
        float horizontalMidpoint = _bounds.Y;

        // Use the computed edge properties to check if the object fits perfectly in a quadrant
        bool topQuadrant = (rect.Top < horizontalMidpoint && rect.Bottom < horizontalMidpoint);
        bool bottomQuadrant = (rect.Top > horizontalMidpoint);

        if (rect.Left < verticalMidpoint && rect.Right < verticalMidpoint) {
            if (topQuadrant) index = 1;
            else if (bottomQuadrant) index = 2;
        } else if (rect.Left > verticalMidpoint) {
            if (topQuadrant) index = 0;
            else if (bottomQuadrant) index = 3;
        }
        return index;
    }

    public void Insert(GameObject obj) {
        if (_nodes[0] != null) {
            int index = GetIndex(obj.transform.rect);
            if (index != -1) {
                _nodes[index].Insert(obj);
                return;
            }
        }

        _objects.Add(obj);

        if (_objects.Count > MAX_OBJECTS && _level < MAX_LEVELS) {
            if (_nodes[0] == null) Split();

            int i = 0;
            while (i < _objects.Count) {
                int index = GetIndex(_objects[i].transform.rect);
                if (index != -1) {
                    _nodes[index].Insert(_objects[i]);
                    _objects.RemoveAt(i);
                } else {
                    i++;
                }
            }
        }
    }

    // Retrieve by GameObject Bounds
    public void Retrieve(List<GameObject> returnObjects, Rect searchArea) {
        int index = GetIndex(searchArea);
        if (index != -1 && _nodes[0] != null) {
            _nodes[index].Retrieve(returnObjects, searchArea);
        } else if (_nodes[0] != null) {
            // If the search area overlaps multiple quadrants, we must check them all
            for (int i = 0; i < 4; i++) {
                if (_nodes[i]._bounds.IntersectsWith(searchArea)) {
                    _nodes[i].Retrieve(returnObjects, searchArea);
                }
            }
        }
        returnObjects.AddRange(_objects);
    }
}