using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public LevelData level;

    public int stage;
    public bool inLevel = false;

    void Start(){
        Debug.Log("Level Manager Start");
    }

    void Update(){
        Debug.Log("Level Manager UPDATE");
    }
    public void test(){
        Debug.Log("Level Manager tests");
    }
}
