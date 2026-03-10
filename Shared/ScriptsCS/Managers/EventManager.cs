namespace Shared;


public class EventManager
{
    // SPatial hash collision system
    const int cellSize = 96;

    Dictionary<(int,int), List<GameObject>> grid = new Dictionary<(int,int), List<GameObject>>();

    // clear every frame
    public void Clear()
    {
        grid.Clear();
    }

    // ignore disabled collisions, find obj and map to cell (create cell if does not exist)
    public void Register(GameObject obj)
    {
        if (obj.disableCollision) return;

        int cellX = (int)(obj.transform.position.X / cellSize);
        int cellY = (int)(obj.transform.position.Y / cellSize);

        var key = (cellX, cellY);

        if (!grid.ContainsKey(key))
        {
            grid[key] = new List<GameObject>();
        }

        grid[key].Add(obj);
    }

    // takes callback function that handles the event
    public void ProcessCollisions(Action<GameObject, GameObject> handler)
    {
        foreach (var cell in grid.Values)
        {
            int count = cell.Count;

            for (int i = 0; i < count; i++)
            {
                GameObject a = cell[i];

                for (int j = i + 1; j < count; j++)
                {
                    GameObject b = cell[j];
                    handler(a, b);
                }
            }
        }
    }
}