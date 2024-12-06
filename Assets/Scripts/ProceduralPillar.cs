using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralPillar : MonoBehaviour
{
    public float height = 5f; // Height of the pillar
    public float radius = 1f; // Radius of the pillar
    public int segments = 32; // Number of vertical segments (controls smoothness)
    public int flutes = 8; // Number of flutes (grooves)

    private void Start()
    {
        GeneratePillar();
    }

    private void GeneratePillar()
    {
        Mesh mesh = new Mesh();

        // Vertices, UVs, and Triangles
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        Vector2[] uvs = new Vector2[vertices.Length];
        int[] triangles = new int[segments * 6];

        float angleStep = 2 * Mathf.PI / segments; // Angle between segments
        float fluteDepth = 0.1f; // Depth of fluting
        float fluteAngleStep = 2 * Mathf.PI / flutes;

        // Generate Vertices
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;

            // Calculate fluting (grooves)
            float fluteAngle = Mathf.Sin(angle * flutes) * fluteDepth;

            // Bottom vertex
            vertices[i] = new Vector3(
                Mathf.Cos(angle) * (radius + fluteAngle),
                0,
                Mathf.Sin(angle) * (radius + fluteAngle)
            );

            // Top vertex
            vertices[i + segments + 1] = new Vector3(
                Mathf.Cos(angle) * (radius + fluteAngle),
                height,
                Mathf.Sin(angle) * (radius + fluteAngle)
            );

            // UV mapping
            uvs[i] = new Vector2((float)i / segments, 0);
            uvs[i + segments + 1] = new Vector2((float)i / segments, 1);
        }

        // Generate Triangles
        for (int i = 0; i < segments; i++)
        {
            // Bottom triangle
            triangles[i * 6] = i;
            triangles[i * 6 + 1] = i + segments + 1;
            triangles[i * 6 + 2] = i + 1;

            // Top triangle
            triangles[i * 6 + 3] = i + 1;
            triangles[i * 6 + 4] = i + segments + 1;
            triangles[i * 6 + 5] = i + segments + 2;
        }

        // Assign mesh
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // Apply to the MeshFilter
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
