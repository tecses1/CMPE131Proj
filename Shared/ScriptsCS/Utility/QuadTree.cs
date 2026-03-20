namespace Shared;
using System.Drawing;
public class Quadtree {
    private const int MAX_OBJECTS = 4;
    private const int MAX_LEVELS = 5; // Prevents infinite splitting

    private int _level;
    private List<GameObject> _objects;
    private RectangleF _bounds;
    private Quadtree[] _nodes;

    public Quadtree(int level, RectangleF bounds) {
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
        float x = _bounds.X;
        float y = _bounds.Y;

        _nodes[0] = new Quadtree(_level + 1, new RectangleF(x + subWidth, y, subWidth, subHeight));
        _nodes[1] = new Quadtree(_level + 1, new RectangleF(x, y, subWidth, subHeight));
        _nodes[2] = new Quadtree(_level + 1, new RectangleF(x, y + subHeight, subWidth, subHeight));
        _nodes[3] = new Quadtree(_level + 1, new RectangleF(x + subWidth, y + subHeight, subWidth, subHeight));
    }

    private int GetIndex(RectangleF rect) {
        int index = -1;
        double verticalMidpoint = _bounds.X + (_bounds.Width / 2);
        double horizontalMidpoint = _bounds.Y + (_bounds.Height / 2);

        bool topQuadrant = (rect.Y < horizontalMidpoint && rect.Y + rect.Height < horizontalMidpoint);
        bool bottomQuadrant = (rect.Y > horizontalMidpoint);

        if (rect.X < verticalMidpoint && rect.X + rect.Width < verticalMidpoint) {
            if (topQuadrant) index = 1;
            else if (bottomQuadrant) index = 2;
        } else if (rect.X > verticalMidpoint) {
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
    public void Retrieve(List<GameObject> returnObjects, RectangleF searchArea) {
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