using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Control calibration tips visibility
/// </summary>
public class EnableDisableCalibrationTips : MonoBehaviour
{
    /// <summary>
	/// Children of parent of tips
	/// </summary>
    List<GameObject> children;

    /// <summary>
	/// Gets all children
	/// </summary>
    private void Awake()
    {
        children = gameObject.GetChildren();
    }

    /// <summary>
	/// Make tips disabled
	/// </summary>
    private void Start()
    {
        foreach (var obj in children)
        {
            obj.SetActive(false);
        }
    }

    /// <summary>
	/// Start update timer
	/// </summary>
	/// <param name="IsEnabled">Flag of tips visibility</param>
    public void EnableDisableTips(bool IsEnabled)
    {
        StartCoroutine(UpdateTipsState(IsEnabled));
    }

    /// <summary>
	/// Updates tip state after time
	/// </summary>
	/// <param name="IsEnabled">tips state</param>
	/// <returns>Tips visability update with delay</returns>
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
