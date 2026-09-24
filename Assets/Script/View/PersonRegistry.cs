using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PersonRegistry", menuName = "Scriptable Objects/PersonRegistry")]
public class PersonRegistry : ScriptableObject
{
    [SerializeField] private List<GameObject> view = new();

    public IEnumerable<GameObject> GetView()
    {
        foreach (var result in view)
        {
            yield return result;
        }
    }
}
