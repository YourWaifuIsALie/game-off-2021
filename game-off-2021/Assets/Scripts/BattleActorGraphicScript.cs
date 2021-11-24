using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleActorGraphicScript : MonoBehaviour
{
    // TODO figure out how GameObject graphics and animations work
    [SerializeField]
    private GameObject _mainObject;

    [SerializeField]
    private TextMeshPro _healthDisplay;

    [SerializeField]
    private TextMeshPro _nameDisplay;

    [SerializeField]
    private GameObject _selectionCollider;

    [SerializeField]
    private GameObject _rotationPoint;

    public bool isFlipped { get; set; }
    public bool isSelected { get; set; }
    public bool isTargeted { get; set; }

    public BattleActorGraphicScript()
    {
        isFlipped = false;
    }

    public void Update()
    {
        if (isFlipped)
            _mainObject.GetComponent<SpriteRenderer>().flipX = true;
        else
            _mainObject.GetComponent<SpriteRenderer>().flipX = false;
    }

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
        // Simple rotation towards camera
        _rotationPoint.transform.LookAt(target);
        _rotationPoint.transform.rotation = Quaternion.Euler(0f, _rotationPoint.transform.rotation.eulerAngles.y + 180f, 0f);

        // Partial rotation towards camera
        // Vector3 targetDirection = _rotationPoint.transform.position - target.transform.position;
        // Quaternion rotationValue = Quaternion.Lerp(Quaternion.identity, Quaternion.LookRotation(targetDirection, Vector3.up), 0.2f);
        // _rotationPoint.transform.rotation = rotationValue;
    }

    public void UpdateSelected(bool selected)
    {
        if (selected)
            _mainObject.GetComponent<Renderer>().material.color = Color.red;
        else
            _mainObject.GetComponent<Renderer>().material.color = Color.white;
    }
    public void PlayAnimation(string value)
    {
        ((Animator)_mainObject.GetComponent(typeof(Animator))).SetTrigger(value);
    }

    public void SetGraphics(Sprite sprite, RuntimeAnimatorController animation)
    {
        _mainObject.GetComponent<SpriteRenderer>().sprite = sprite;
        _mainObject.GetComponent<Animator>().runtimeAnimatorController = animation;
    }
    public void SetTextColor(Color color)
    {
        _healthDisplay.color = color;
        _nameDisplay.color = color;
    }
}
