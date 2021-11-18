using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleActorGraphicScript : MonoBehaviour
{
    // TODO figure out how GameObject graphics and animations work
    // This script might be unnecessary

    [SerializeField]
    private TextMesh _healthDisplay;

    [SerializeField]
    private TextMesh _nameDisplay;

    public bool isSelected { get; set; }
    public bool isTargeted { get; set; }

    public void UpdateHealth(int current, int max)
    {
        _healthDisplay.text = $"{current}/{max}";
    }

    public void UpdateName(string name)
    {
        _nameDisplay.text = $"{name}";
    }

    public void RotateTowards(Transform target)
    {
        _healthDisplay.transform.LookAt(target);
        _healthDisplay.transform.rotation = Quaternion.Euler(0f, _healthDisplay.transform.rotation.eulerAngles.y + 180f, 0f);

        _nameDisplay.transform.LookAt(target);
        _nameDisplay.transform.rotation = Quaternion.Euler(0f, _nameDisplay.transform.rotation.eulerAngles.y + 180f, 0f);
    }
}
