

using System;
using UnityEngine;

[Serializable]
public class EOSATOrbit
{
    public float[] pos_eci_km;
}

[Serializable]
public class EOSATAttitude
{
    public float[] q_body_to_eci;
}

[Serializable]
public class EOSATFrame
{
    public string type;
    public int seq;
    public EOSATOrbit orbit;
    public EOSATAttitude attitude;
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

        string[] lines = satelliteFile.text.Split('\n');
        int count = 0;

        foreach (string line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            EOSATFrame frame;

            try
            {
                frame = JsonUtility.FromJson<EOSATFrame>(line);
            }
            catch (Exception)
            {
                Debug.LogWarning("Invalid JSON line skipped");
                continue;
            }

            if (frame == null || frame.type != "state" ||
                frame.orbit == null || frame.attitude == null ||
                frame.orbit.pos_eci_km == null ||
                frame.attitude.q_body_to_eci == null ||
                frame.orbit.pos_eci_km.Length != 3 ||
                frame.attitude.q_body_to_eci.Length != 4)
                continue;

            count++;

            if (count == 1)
            {
                Debug.Log("Position: " +
                    string.Join(", ", frame.orbit.pos_eci_km));

                Debug.Log("Rotation: " +
                    string.Join(", ", frame.attitude.q_body_to_eci));
            }
        }

        Debug.Log("Satellite frames read: " + count);
    }
}

