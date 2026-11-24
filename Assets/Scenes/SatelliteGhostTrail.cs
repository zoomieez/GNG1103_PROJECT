using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(1000)]
public class SatelliteGhostTrail : MonoBehaviour
{
    [Header("References")]
    public Transform satellite;

    [Header("Ghost Settings")]
    public float delaySeconds = 0.3f;

    [Range(0f, 1f)]
    public float ghostOpacity = 0.2f;

    public float recordInterval = 0.02f;

    private GameObject ghost;
    private Material ghostMaterial;

    private float nextRecordTime;

    private List<Transform> originalParts =
        new List<Transform>();

    private List<Transform> ghostParts =
        new List<Transform>();

    private List<Snapshot> history =
        new List<Snapshot>();

    private class Snapshot
    {
        public float time;
        public Vector3[] positions;
        public Quaternion[] rotations;
        public Vector3[] scales;
    }

    void Start()
    {
        if (satellite == null)
        {
            Debug.LogError("Assign the satellite!");
            enabled = false;
            return;
        }

        CreateGhost();
    }

    void CreateGhost()
    {
        ghost = new GameObject("Satellite Ghost");

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Unlit"
        );

        if (shader == null)
        {
            Debug.LogError("Ghost shader not found!");
            enabled = false;
            return;
        }

        ghostMaterial = new Material(shader);

        ghostMaterial.SetFloat("_Surface", 1f);
        ghostMaterial.SetFloat("_Blend", 0f);
        ghostMaterial.SetFloat("_ZWrite", 0f);

        ghostMaterial.SetFloat(
            "_SrcBlend",
            (float)UnityEngine.Rendering.BlendMode.SrcAlpha
        );

        ghostMaterial.SetFloat(
            "_DstBlend",
            (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha
        );

        ghostMaterial.EnableKeyword(
            "_SURFACE_TYPE_TRANSPARENT"
        );

        ghostMaterial.SetOverrideTag(
            "RenderType", "Transparent"
        );

        ghostMaterial.renderQueue = 3000;

        MeshRenderer[] renderers =
            satellite.GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer source in renderers)
        {
            if (!source.enabled)
                continue;

            MeshFilter sourceFilter =
                source.GetComponent<MeshFilter>();

            if (sourceFilter == null ||
                sourceFilter.sharedMesh == null)
                continue;

            GameObject part =
                new GameObject(source.name);

            part.transform.SetParent(
                ghost.transform, false
            );

            MeshFilter filter =
                part.AddComponent<MeshFilter>();

            filter.sharedMesh =
                sourceFilter.sharedMesh;

            MeshRenderer renderer =
                part.AddComponent<MeshRenderer>();

            Material[] materials =
                new Material[sourceFilter.sharedMesh.subMeshCount];

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = ghostMaterial;
            }

            renderer.sharedMaterials = materials;

            renderer.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;

            renderer.receiveShadows = false;

            originalParts.Add(source.transform);
            ghostParts.Add(part.transform);
        }

        ghost.SetActive(false);
    }

    void RecordSnapshot()
    {
        int count = originalParts.Count;

        Snapshot snapshot = new Snapshot();

        snapshot.time = Time.time;

        snapshot.positions = new Vector3[count];
        snapshot.rotations = new Quaternion[count];
        snapshot.scales = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            snapshot.positions[i] =
                originalParts[i].position;

            snapshot.rotations[i] =
                originalParts[i].rotation;

            snapshot.scales[i] =
                originalParts[i].lossyScale;
        }

        history.Add(snapshot);
    }

    void LateUpdate()
    {
        if (ghost == null || ghostMaterial == null)
            return;

        if (Time.time >= nextRecordTime)
        {
            RecordSnapshot();

            nextRecordTime =
                Time.time + Mathf.Max(0.001f, recordInterval);
        }

        float targetTime =
            Time.time - Mathf.Max(0f, delaySeconds);

        // Keep enough history for delay adjustments
        float retentionTime =
            Mathf.Max(10f, delaySeconds + 1f);

        while (history.Count > 2 &&
               history[1].time < Time.time - retentionTime)
        {
            history.RemoveAt(0);
        }

        if (history.Count < 2 ||
            targetTime < history[0].time)
        {
            ghost.SetActive(false);
            return;
        }

        int index = 0;

        while (index < history.Count - 2 &&
               history[index + 1].time < targetTime)
        {
            index++;
        }

        Snapshot older = history[index];
        Snapshot newer = history[index + 1];

        float t = Mathf.InverseLerp(
            older.time,
            newer.time,
            targetTime
        );

        for (int i = 0; i < ghostParts.Count; i++)
        {
            ghostParts[i].position =
                Vector3.Lerp(
                    older.positions[i],
                    newer.positions[i],
                    t
                );

            ghostParts[i].rotation =
                Quaternion.Slerp(
                    older.rotations[i],
                    newer.rotations[i],
                    t
                );

            ghostParts[i].localScale =
                Vector3.Lerp(
                    older.scales[i],
                    newer.scales[i],
                    t
                );
        }

        ghostMaterial.SetColor(
            "_BaseColor",
            new Color(
                0.65f,
                0.8f,
                1f,
                ghostOpacity
            )
        );

        ghost.SetActive(true);
    }

    void OnDestroy()
    {
        if (ghost != null)
            Destroy(ghost);

        if (ghostMaterial != null)
            Destroy(ghostMaterial);
    }
}