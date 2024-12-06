using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemPillar : MonoBehaviour
{
    public PillarConfig config; // Reference to the ScriptableObject for initial values

    // Local editable variables
    [Header("Editable Variables (Loaded from SO if assigned)")]
    public string axiom = "F"; // Starting axiom
    public List<LSystemRule> rules = new List<LSystemRule>(); // Rules for L-system
    public int iterations = 3; // Number of iterations
    public float angle = 90f; // Rotation angle for "+" and "-"
    public float segmentLength = 1f; // Length of each segment
    public float height = 5f; // Height of the pillar
    public float thickness = 0.1f; // Wall thickness

    private Dictionary<char, string> ruleMap;

    private void Start()
    {
        // Load values from the ScriptableObject if assigned
        if (config != null)
        {
            LoadFromConfig();
        }

        // Initialize the rules and generate the pillar
        InitializeRules();
        GeneratePillar();
    }

    private void LoadFromConfig()
    {
        axiom = config.axiom;
        rules = new List<LSystemRule>(config.rules); // Copy rules
        iterations = config.iterations;
        angle = config.angle;
        segmentLength = config.segmentLength;
        height = config.height;
        thickness = config.thickness;
    }

    private void InitializeRules()
    {
        ruleMap = new Dictionary<char, string>();
        foreach (var rule in rules)
        {
            if (!ruleMap.ContainsKey(rule.symbol))
            {
                ruleMap.Add(rule.symbol, rule.replacement);
            }
        }
    }

    private void GeneratePillar()
    {
        List<Vector2> crossSection = GenerateCrossSection();
        Mesh pillarMesh = GeneratePillarMesh(crossSection);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = pillarMesh;
        }
    }

    private List<Vector2> GenerateCrossSection()
    {
        string currentString = axiom;

        for (int i = 0; i < iterations; i++)
        {
            string nextString = "";
            foreach (char c in currentString)
            {
                nextString += ruleMap.TryGetValue(c, out var replacement) ? replacement : c.ToString();
            }
            currentString = nextString;
        }

        List<Vector2> points = new List<Vector2>();
        Vector2 position = Vector2.zero;
        float currentAngle = 0;

        Stack<(Vector2, float)> stateStack = new Stack<(Vector2, float)>();

        foreach (char c in currentString)
        {
            switch (c)
            {
                case 'F':
                    Vector2 newPosition = position + new Vector2(
                        Mathf.Cos(Mathf.Deg2Rad * currentAngle),
                        Mathf.Sin(Mathf.Deg2Rad * currentAngle)
                    ) * segmentLength;
                    points.Add(newPosition);
                    position = newPosition;
                    break;

                case '+':
                    currentAngle -= angle;
                    break;

                case '-':
                    currentAngle += angle;
                    break;

                case '[':
                    stateStack.Push((position, currentAngle));
                    break;

                case ']':
                    if (stateStack.Count > 0)
                    {
                        (position, currentAngle) = stateStack.Pop();
                    }
                    break;
            }
        }

        return points;
    }

    private void OnValidate()
    {
        //if (config != null)
        //{
        //    LoadFromConfig(); // Refresh variables from SO if it changes
        //}

        InitializeRules();
        GeneratePillar();
    }

    private Mesh GeneratePillarMesh(List<Vector2> crossSection)
    {
        if (crossSection.Count < 3)
        {
            Debug.LogWarning("Cross-section must have at least 3 points to create a valid mesh.");
            return new Mesh();
        }

        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // Create vertices for outer and inner walls
        for (int i = 0; i < crossSection.Count; i++)
        {
            Vector2 outerPoint = crossSection[i];
            Vector2 direction = outerPoint.normalized;
            Vector2 innerPoint = outerPoint - direction * thickness;

            // Bottom and top vertices for both shells
            vertices.Add(new Vector3(outerPoint.x, 0, outerPoint.y));
            vertices.Add(new Vector3(outerPoint.x, height, outerPoint.y));
            vertices.Add(new Vector3(innerPoint.x, 0, innerPoint.y));
            vertices.Add(new Vector3(innerPoint.x, height, innerPoint.y));

            // UV mapping
            float u = (float)i / crossSection.Count;
            uvs.Add(new Vector2(u, 0));
            uvs.Add(new Vector2(u, 1));
            uvs.Add(new Vector2(u, 0));
            uvs.Add(new Vector2(u, 1));
        }

        // Create triangles to form the mesh
        for (int i = 0; i < crossSection.Count; i++)
        {
            int next = (i + 1) % crossSection.Count;

            // Outer wall
            triangles.Add(i * 4 + 0); triangles.Add(next * 4 + 0); triangles.Add(i * 4 + 1);
            triangles.Add(i * 4 + 1); triangles.Add(next * 4 + 0); triangles.Add(next * 4 + 1);

            // Inner wall
            triangles.Add(i * 4 + 2); triangles.Add(i * 4 + 3); triangles.Add(next * 4 + 2);
            triangles.Add(next * 4 + 2); triangles.Add(i * 4 + 3); triangles.Add(next * 4 + 3);

            // Bottom cap
            triangles.Add(i * 4 + 0); triangles.Add(next * 4 + 2); triangles.Add(i * 4 + 2);
            triangles.Add(i * 4 + 0); triangles.Add(next * 4 + 0); triangles.Add(next * 4 + 2);

            // Top cap
            triangles.Add(i * 4 + 1); triangles.Add(i * 4 + 3); triangles.Add(next * 4 + 3);
            triangles.Add(i * 4 + 1); triangles.Add(next * 4 + 3); triangles.Add(next * 4 + 1);
        }

        // Assign data to mesh
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        return mesh;
    }
}