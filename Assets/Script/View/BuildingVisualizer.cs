using System.Collections.Generic;
using UnityEngine;

public class BuildingVisualizer : MonoBehaviour
{
    [SerializeField] private List<BuildingRegistry> buildings = new List<BuildingRegistry>();

    private Dictionary<string, GameObject> view = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var registry in buildings)
        {
            foreach (var build in registry.GetView())
            {
                GameObject gameObject = Instantiate(build);
                view.Add(build.name, gameObject);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
