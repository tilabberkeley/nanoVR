using System.Collections;
using UnityEngine;

 public class HelixComponent : MonoBehaviour
 {

    private int _id; // id of nucl within helix
    private int _helixId; // id of the helix itself
    private int _ssDirection; // 0 = 5' to 3', 1 = 3' to 5'single strand

    private GameObject _prevGO = null;
    private GameObject _nextGO = null;
    private GameObject _prevBB = null;
    private GameObject _nextBB = null;
    private GameObject _complementGO = null;

    private bool _selected = false;

     

    private Renderer _ntRenderer;

    public int GetId() { return _id; }
    public void SetId(int id) { _id = id; }
    public int GetHelixId() { return _helixId; }
    public void SetHelixId(int id) { _helixId = id; }
    public int GetSSId() { return _ssDirection; }
    public void SetSSId(int ssDirection) { _ssDirection = ssDirection; }

    public Vector3 GetPosition() { return transform.position; }
    public void SetPosition(Vector3 p) { transform.position = p; }


    public GameObject GetPrevGO() { return _prevGO; }
    public void SetPrevGO(GameObject p) { _prevGO = p; }

    public GameObject GetPrevBB() { return _prevBB; }
    public void SetPrevBB(GameObject p) { _prevBB = p; }

    public GameObject GetNextGO() { return _nextGO; }
    public void SetNextGO(GameObject n) { _nextGO = n; }

    public GameObject GetNextBB() { return _nextBB; }
    public void SetNextBB(GameObject n) { _nextBB = n; }

    public GameObject GetComplementGO() { return _complementGO; }
    public void SetComplementGO(GameObject c) { _complementGO = c; }

    public bool IsSelected() { return _selected; }
}
