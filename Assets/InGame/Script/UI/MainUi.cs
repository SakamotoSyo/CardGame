//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class MainUi : MonoBehaviour
{
    public static Transform MainUiTransform { get; private set; }
    [SerializeField] private Transform _MainUiTransform;

    public void SetUp() 
    {
        MainUiTransform = _MainUiTransform;
    }
}
