//日本語対応
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyEffectData
{
    public IEffect EnemyEffect => _enemyEffect;
    [SubclassSelector, SerializeReference]
    private IEffect _enemyEffect;

    public AttackTargetRangeType Target => _targetType;
    [SerializeField] private AttackTargetRangeType _targetType;

    public EffectType ImageType => _imageType;
    [SerializeField] private EffectType _imageType;

    public float EffectPower => _effectPower;
    [SerializeField] private float _effectPower;
    public EnemyEffectData(IEffect effect, AttackTargetRangeType target, EffectType image, float power)
    {
        _enemyEffect = effect;
        _targetType = target;
        _imageType = image;
        _effectPower = power;
    }

    public void EffectPowerMultiplication(float value)
    {
        _effectPower *= value;
    }
}