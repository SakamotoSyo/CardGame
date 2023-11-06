//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyLayoutGroup : MonoBehaviour
{
    [SerializeField] Vector2 _spacing = Vector2.zero;
    private int _childCount = 0;

    public void AddChild(Transform transform) 
    {
        transform.localPosition += new Vector3(transform.position.x + (_spacing.x * _childCount),
                                          transform.position.y + (_spacing.y * _childCount));
        _childCount++;
    }
}
