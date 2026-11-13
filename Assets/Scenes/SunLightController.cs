
using UnityEngine;

public class SunLightController : MonoBehaviour
{
    public Transform sun;
    public Transform earth;

    void LateUpdate()
    {
        if (sun == null || earth == null)
            return;

        // Point the directional light from Sun toward Earth
        Vector3 direction = earth.position - sun.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(direction);
        }
    }
}
