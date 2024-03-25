using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CoralColorTransitionManager : MonoBehaviour
{

    [SerializeField] private Volume _volume;
    [SerializeField] private float _colorShiftDuration;

    public float ColorShiftDuration => _colorShiftDuration;

    public void DoColorShift()
    {
        if (_volume.profile.TryGet(out ColorAdjustments colorAdjustments))
        {
            StartCoroutine(ColorShift(colorAdjustments));
        }
    }


    private IEnumerator ColorShift(ColorAdjustments colorAdjustments)
    {
        float elapsedTime = 0;

        while (elapsedTime <= _colorShiftDuration)
        {
            colorAdjustments.hueShift.Interp(12, 0, elapsedTime / _colorShiftDuration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }
}
