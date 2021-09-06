using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnableDisableCalibrationTips : MonoBehaviour
{
    List<GameObject> children;


    private void Awake()
    {
        children = gameObject.GetChildren();
    }

    private void Start()
    {
        foreach (var obj in children)
        {
            obj.SetActive(false);
        }
    }

    public void EnableDisableTips(bool IsEnabled)
    {
        StartCoroutine(UpdateTipsState(IsEnabled));
    }

    

    IEnumerator UpdateTipsState(bool IsEnabled)
    {
        float time = IsEnabled ? 0 : 1;
        yield return new WaitForSeconds(time);

        foreach (var obj in children)
        {
            obj.SetActive(IsEnabled);
        }
    }
}
