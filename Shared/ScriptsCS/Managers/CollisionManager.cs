namespace Shared;
using System.Drawing;
public class CollisionManager{
    private Quadtree _tree;
    private List<List<GameObject>> _registeredGroups;
    private Rect _worldBounds;

    public CollisionManager(float worldWidth, float worldHeight) {
        _worldBounds = new Rect(0, 0, worldWidth, worldHeight);
        _tree = new Quadtree(0, _worldBounds);
        _registeredGroups = new List<List<GameObject>>();
    }

    public void Register(List<GameObject> group) {
        if (!_registeredGroups.Contains(group)) {
            _registeredGroups.Add(group);
        }
    }

    // Call this at the start of your frame
    public void UpdateTree() {
        _tree.Clear();
        foreach (var group in _registeredGroups) {
            foreach (var obj in group) {
                if (!obj.disableCollision) {
                    _tree.Insert(obj);
                }
            }
        }
    }

    // Broad Phase + Narrow Phase Collision Check
    public Collision[] CheckCollisions() {
        var collisions = new List<Collision>();
        var candidates = new List<GameObject>();

        foreach (var group in _registeredGroups) {
            foreach (var objA in group) {
                if (objA.disableCollision) continue;

                candidates.Clear();
                _tree.Retrieve(candidates, objA.transform.rect );

                foreach (var objB in candidates) {
                    if (objB.disableCollision) continue;

                    // Prevent checking A vs A, and prevent duplicate pairs (A vs B, then B vs A)
                    if (objA.uid >= objB.uid) continue;

                    // Narrow phase: Actuarl AABB intersection
                    if (objA.transform.rect.IntersectsWith(objB.transform.rect)) {
                        collisions.Add(new Collision(objA, objB));
                    }
                }
            }
        }
        return collisions.ToArray();
    }
    public GameObject[] GetOutOfBoundsObjects() {
        var outOfBounds = new List<GameObject>();

        foreach (var group in _registeredGroups) {
            foreach (var obj in group) {

                // Check if any part of the object is outside the 0 to WorldX/WorldY limits
                // Use the edges!
                Rect objRect = obj.transform.rect;
                bool isOutLeft = objRect.Left < _worldBounds.Left;
                bool isOutRight = objRect.Right > _worldBounds.Right;
                bool isOutTop = objRect.Top < _worldBounds.Top;
                bool isOutBottom = objRect.Bottom > _worldBounds.Bottom;

                if (isOutLeft || isOutRight || isOutTop || isOutBottom) {
                    outOfBounds.Add(obj);
                }
            }
        }

        return outOfBounds.ToArray();
    }

    // Proximity Check using Squared Distance optimization
    public GameObject[] GetNearby(GameObject origin, float range) {
        float rangeSq = range * range;
        
        // Define our circular search area as a bounding box for the Quadtree query
        var searchArea = new Rect(
            origin.transform.rect.X, 
            origin.transform.rect.Y, 
            range*2, 
            range*2
        );

        
        
        var candidates = new List<GameObject>();
        _tree.Retrieve(candidates, searchArea);

        return candidates
            .Where(other => {
                if (other == origin || other.disableCollision) return false;
                
                float dx = origin.transform.rect.X - other.transform.rect.X;
                float dy = origin.transform.rect.Y - other.transform.rect.Y;
                float distSq = (dx * dx) + (dy * dy);
                
                return distSq <= rangeSq; // Filter out corners of the bounding box
            })
            .OrderBy(other => {
                float dx = origin.transform.rect.X - other.transform.rect.X;
                float dy = origin.transform.rect.Y - other.transform.rect.Y;
                return (dx * dx) + (dy * dy); // Sort by squared distance
            })
            .ToArray();
    }

    public GameObject[] GetNearby(System.Numerics.Vector2 point, float range)
{
    float rangeSq = range * range;
    
    var searchArea = new Rect(point.X - range, point.Y - range, range * 2, range * 2);
    
    var candidates = new List<GameObject>();
    _tree.Retrieve(candidates, searchArea);

    return candidates
        .Where(other => {
            if (other.disableCollision) return false;
            
            float dx = point.X - other.transform.rect.X;
            float dy = point.Y - other.transform.rect.Y;
            return (dx * dx) + (dy * dy) <= rangeSq;
        })
        .OrderBy(other => {
            float dx = point.X - other.transform.rect.X;
            float dy = point.Y - other.transform.rect.Y;
            return (dx * dx) + (dy * dy);
        })
        .ToArray();
}
}