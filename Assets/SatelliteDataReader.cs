
using System;
using UnityEngine;

[Serializable]
public class SatelliteValues
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class SatelliteData
{
    public SatelliteValues position;
    public SatelliteValues rotation;
}

public class SatelliteDataReader : MonoBehaviour
{
    public TextAsset satelliteFile;

    void Start()
    {
        if (satelliteFile == null)
        {
            Debug.LogError("Satellite file is missing!");
            return;
        }

        SatelliteData data =
            JsonUtility.FromJson<SatelliteData>(
                satelliteFile.text);

        if (data == null ||
            data.position == null ||
            data.rotation == null)
        {
            Debug.LogError("Invalid satellite data!");
            return;
        }

        Debug.Log("Position: " +
            data.position.x + ", " +
            data.position.y + ", " +
            data.position.z);

        Debug.Log("Rotation: " +
            data.rotation.x + ", " +
            data.rotation.y + ", " +
            data.rotation.z);
    }
}


