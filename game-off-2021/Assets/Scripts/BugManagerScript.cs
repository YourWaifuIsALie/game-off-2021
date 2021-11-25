using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugManagerScript : MonoBehaviour
{
    [SerializeField]
    private Material _groundStaticMaterial;

    [SerializeField]
    private GameObject _groundObject;

    public int bugLevel { get; set; }
    private Dictionary<string, bool> _bugAchieved;

    public void Start()
    {
        bugLevel = 0;
        _bugAchieved = new Dictionary<string, bool>();
        _bugAchieved["integerOverflow"] = false;
    }

    public void LateUpdate()
    {
        bugLevel = DetermineBugLevel();
        switch (bugLevel)
        {
            case 0:
                break;
            case 1:
                _groundObject.GetComponent<MeshRenderer>().material = _groundStaticMaterial;
                break;
            default:
                break;
        }
    }

    private int DetermineBugLevel()
    {
        int count = 0;
        foreach (KeyValuePair<string, bool> element in _bugAchieved)
        {
            if (element.Value)
                count += 1;
        }
        return count;
    }

    public void IntegerOverflow()
    {
        _bugAchieved["integerOverflow"] = true;
    }
}
