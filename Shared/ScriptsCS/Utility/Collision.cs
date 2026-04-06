namespace Shared;
public class Collision {
    public GameObject ObjectA { get; private set; }
    public GameObject ObjectB { get; private set; }

    public Collision(GameObject a, GameObject b) {
        ObjectA = a;
        ObjectB = b;
    }
}