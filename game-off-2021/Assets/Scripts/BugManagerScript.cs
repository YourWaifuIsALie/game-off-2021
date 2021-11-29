using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BugManagerScript : MonoBehaviour
{
    [SerializeField]
    private Material _groundStaticMaterial;

    [SerializeField]
    private GameObject _groundObject;

    [SerializeField]
    private MusicManagerScript _musicManager;

    [SerializeField]
    private GameObject _skyPlane;
    [SerializeField]
    private Material _skyBugMaterial;

    public int bugLevel { get; set; }
    private Dictionary<string, bool> _bugAchieved;

    private bool _musicChangeOnce;

    public void Start()
    {
        bugLevel = 0;
        _bugAchieved = new Dictionary<string, bool>();
        _bugAchieved["integerOverflow"] = false; // Overflow enemy health
        _bugAchieved["illegalAction"] = false;  // Just the byteswap ability for now

        _musicChangeOnce = true;
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
            case 2:
                _skyPlane.GetComponent<MeshRenderer>().material = _skyBugMaterial;
                break;
            case 3:
                if (_musicChangeOnce)
                {
                    _musicManager.SwitchMusic("bug");
                    _musicChangeOnce = false;
                }
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

    public void IllegalAction()
    {
        _bugAchieved["illegalAction"] = true;
    }

    public void BattleComplete(string value)
    {
        _bugAchieved[value] = true;
    }
}
