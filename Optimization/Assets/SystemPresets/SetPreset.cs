using System;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using Object = UnityEngine.Object;

public class SetPreset : MonoBehaviour
{
    [SerializeField] private Preset _preset;
    [SerializeField] private Transform _target;
    

    private void Start()
    {
        _preset.ApplyTo(_target);
    }
}
