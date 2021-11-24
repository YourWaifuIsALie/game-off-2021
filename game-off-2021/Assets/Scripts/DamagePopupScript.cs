using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopupScript : MonoBehaviour
{
    private int _damage;
    private float _velocity;
    private Color _color;
    private TextMeshPro _script;
    private float _startTimer;

    public void Start()
    {
        _script = transform.gameObject.GetComponent<TextMeshPro>();
        _velocity = 1f;
        _startTimer = 0.01f;
        _color = _script.color;
        _script.text = "";
    }

    public void Update()
    {
        if (_startTimer > 0)
        {
            _startTimer -= Time.deltaTime;
            return;
        }
        else
        {
            _script.text = _damage.ToString();
            transform.position += new Vector3(0, _velocity, 0) * Time.deltaTime;
            _color.a -= Time.deltaTime;
            _script.color = _color;
            if (_color.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }
}
