using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PillarConfig", menuName = "ScriptableObjects/PillarConfig", order = 1)]
public class PillarConfig : ScriptableObject
{
    public string name = "";
    public string axiom = "F+XF+F+XF"; // Starting axiom
    public LSystemRule[] rules; // L-system rules
    public int iterations = 3; // Number of L-system iterations
    public float angle = 90f; // Rotation angle
    public float segmentLength = 0.2f; // Length of each line segment
    public float height = 5f; // Height of the pillar
    public float thickness = 0.1f; // Wall thickness for hollow pillars
    public bool carved = false; // If the pillar should have a carved region
    public float carveHeightStart = 1.0f; // Start height of carving
    public float carveHeightEnd = 3.0f; // End height of carving
    public float carveDepth = 0.3f; // Depth of carving

    public event Action OnConfigChanged;

    public void NotifyChange()
    {
        OnConfigChanged?.Invoke();
    }
}

[System.Serializable]
 public class LSystemRule
{
    public char symbol; // Character to replace
    public string replacement; // Replacement string
}
