using UnityEngine;

public class ClampOnEnter : MonoBehaviour
{
    bool   _inArena = false;
    float  _minX, _maxX, _minY, _maxY;
    const float Margin = 0.75f;

    void Start()
    {
        // Calcula limites internos da arena, igual ao GetOffscreenPosition, 
        // mas sem o extra de spawn (só pra entrada/saída):
        var cam = Camera.main;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        _minX = -halfW + Margin;
        _maxX =  halfW - Margin;
        _minY = -halfH + Margin;
        _maxY =  halfH - Margin;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (!_inArena)
        {
            // Verifica se o inimigo já cruzou para dentro da janela
            if (pos.x > -_maxX && pos.x < _maxX && pos.y > -_maxY && pos.y < _maxY)
            {
                _inArena = true; 
                // a posartir de agora ele será clamped
            }
        }
        else
        {
            // Mantém ele dentro, mas sem "pular" de volta se ainda estiver fora
            pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
            pos.y = Mathf.Clamp(pos.y, _minY, _maxY);
            transform.position = pos;
        }
    }
}