namespace Shared;


public class ObjChangeWrapper
{

    public List<GameObject> myGroup;
    public GameObject myObj;
    public ObjChangeWrapper(GameObject obj, List<GameObject> group)
    {
        this.myObj = obj;
        this.myGroup = group;
    }

}