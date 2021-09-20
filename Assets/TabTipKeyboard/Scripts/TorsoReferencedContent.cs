using UnityEngine;

/// <summary>
/// Attach object to another object
/// </summary>
public class TorsoReferencedContent : MonoBehaviour
{
    /// <summary>
	/// Trandsorm camera reference
	/// </summary>
#pragma warning disable 649
    [SerializeField]
    protected new Transform camera;
#pragma warning restore 649

    /// <summary>
	/// distance from camera
	/// </summary>
    [Min(0f)]
    [SerializeField]
    protected float distanceFromCamera = 2.5f;

    /// <summary>
	/// Z-Axis rotation
	/// </summary>
    [SerializeField]
    protected float pitch = 0f;

    /// <summary>
	/// offset
	/// </summary>
    protected Vector3 offset;

    /// <summary>
	/// Linear interpolation coefficent
	/// </summary>
    protected static readonly float POSITION_LERP_SPEED = 5f;

    /// <summary>
	/// Check scene setup
	/// </summary>
    protected virtual void Start()
    {
        if (camera == null)
        {
            Debug.LogError("TorsoReferencedContent: The 'Camera' field cannot be left unassigned. Disabling the script");
            enabled = false;
            return;
        }

        Quaternion rotation = Quaternion.Euler(pitch, 0f, 0f);
        offset = rotation * (Vector3.forward * distanceFromCamera);
    }

    /// <summary>
	/// Move object to referenced object
	/// </summary>
    protected virtual void Update()
    {
        Vector3 posTo = camera.position + offset;

        float posSpeed = Time.deltaTime * POSITION_LERP_SPEED;
        transform.position = Vector3.SlerpUnclamped(transform.position, posTo, posSpeed);
    }

    /// <summary>
	/// Switch state
	/// </summary>
    public virtual void SwitchEnabled()
    {
        enabled = !enabled;
    }
}
