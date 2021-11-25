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
    private float _disappearRate = 0.5f;
    private float _radiansPerSecond = 1f;
    private float _radians;

    public void Start()
    {
        _script = transform.gameObject.GetComponent<TextMeshPro>();
        _velocity = 1f;
        _startTimer = 0.01f;
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
            _radians = Mathf.Repeat(_radians + (Time.deltaTime * _radiansPerSecond), 6.28f);
            transform.position += new Vector3(Mathf.Sin(_radians), _velocity, 0) * Time.deltaTime;
            if (_color == null)
                _color = _script.color;
            _color.a -= _disappearRate * Time.deltaTime;
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

    public void SetColor(Color color)
    {
        _color = color;
    }
}
