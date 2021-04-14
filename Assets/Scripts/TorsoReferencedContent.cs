using UnityEngine;

public class TorsoReferencedContent : MonoBehaviour
{
#pragma warning disable 649
    [SerializeField]
    protected new Transform camera;
#pragma warning restore 649

    [Min(0f)]
    [SerializeField]
    protected float distanceFromCamera = 2.5f;

    [SerializeField]
    protected float pitch = 0f;


    protected Vector3 offset;

    protected static readonly float POSITION_LERP_SPEED = 5f;

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

    protected virtual void Update()
    {
        Vector3 posTo = camera.position + offset;

        float posSpeed = Time.deltaTime * POSITION_LERP_SPEED;
        transform.position = Vector3.SlerpUnclamped(transform.position, posTo, posSpeed);
    }

    public virtual void SwitchEnabled()
    {
        enabled = !enabled;
    }
}
