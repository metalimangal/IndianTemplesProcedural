using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LSystemRule
{
    public char symbol;
    public string replacement;
}

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LSystemPillar : MonoBehaviour
{
    [Header("L-System Parameters")]
    public string axiom = "F+XF+F+XF";
    public List<LSystemRule> ruleSet = new List<LSystemRule>
    {
        new LSystemRule { symbol = 'X', replacement = "XF-F+F-XF+F+XF-F+F-X" }
    };
    public int iterations = 3;
    public float angle = 90f;

    [Header("Pillar Properties")]
    public float segmentLength = 0.2f;
    public float height = 5f;
    public float thickness = 0.1f;

    private Dictionary<char, string> rules;

    private void Start()
    {
        rules = new Dictionary<char, string>();
        foreach (var rule in ruleSet)
        {
            rules[rule.symbol] = rule.replacement;
        }

        GeneratePillar();
    }

    private void GeneratePillar()
    {
        List<Vector2> crossSection = GenerateCrossSection();

        Mesh mesh = GeneratePillarMesh(crossSection);

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            meshFilter.mesh = mesh;
        }
    }

     private void OnValidate()
    {
        GeneratePillar();
    }

    private List<Vector2> GenerateCrossSection()
    {
        // Build the final string using the L-system rules
        var currentString = axiom;
        for (int i = 0; i < iterations; i++)
        {
            var nextString = "";
            foreach (char c in currentString)
            {
                if (rules.TryGetValue(c, out var replacement))
                {
                    nextString += replacement;
                }
                else
                {
                    nextString += c.ToString();
                }
            }
            currentString = nextString;
        }

        // Generate 2D points from the final string
        var points = new List<Vector2>();
        var position = Vector2.zero;
        float currentAngle = 0;

        var stateStack = new Stack<(Vector2, float)>();

        foreach (char c in currentString)
        {
            switch (c)
            {
                case 'F': // Move forward
                    var offset = new Vector2(
                        Mathf.Cos(Mathf.Deg2Rad * currentAngle),
                        Mathf.Sin(Mathf.Deg2Rad * currentAngle)
                    ) * segmentLength;
                    position += offset;
                    points.Add(position);
                    break;

                case '+': currentAngle -= angle; break; 
                case '-': currentAngle += angle; break; 
                case '[': stateStack.Push((position, currentAngle)); break; 
                case ']': // Restore state
                    if (stateStack.Count > 0)
                    {
                        (position, currentAngle) = stateStack.Pop();
                    }
                    break;
            }
        }

        return points;
    }

    private Mesh GeneratePillarMesh(List<Vector2> crossSection)
    {
        if (crossSection.Count < 3)
        {
            Debug.LogWarning("Cross-section must have at least 3 points to create a valid mesh.");
            return new Mesh();
        }

        var mesh = new Mesh();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var uvs = new List<Vector2>();

        // Create vertices for outer and inner walls
        for (int i = 0; i < crossSection.Count; i++)
        {
            var outerPoint = crossSection[i];
            var direction = outerPoint.normalized;
            var innerPoint = outerPoint - direction * thickness;

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
