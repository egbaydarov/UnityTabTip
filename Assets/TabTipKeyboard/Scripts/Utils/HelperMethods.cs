
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Util extension methods for TabTipUnity
/// </summary>
public static class HelperMethods
{
    /// <summary>
	/// Search for children of object
	/// </summary>
	/// <param name="go">Parent go</param>
	/// <returns>list of children</returns>
    public static List<GameObject> GetChildren(this GameObject go)
    {
        List<GameObject> children = new List<GameObject>();
        foreach (Transform tran in go.transform)
        {
            children.Add(tran.gameObject);
        }
        return children;
    }
}