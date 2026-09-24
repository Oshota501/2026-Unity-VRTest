using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingRegistry", menuName = "Scriptable Objects/BuildingRegistry")]
public class BuildingRegistry : ScriptableObject
{
    [SerializeField] private List<GameObject> view;

    public IEnumerable<GameObject> GetView()
    {
        foreach (var result in view)
        {
            yield return result;
        }
    }
}
