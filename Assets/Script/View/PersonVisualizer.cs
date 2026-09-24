using System.Collections.Generic;
using UnityEngine;

public class PersonVisualizer : MonoBehaviour
{
    [SerializeField] private List<PersonRegistry> buildings;
    [SerializeField] private PlayerController player;
    [SerializeField] private float detectionPlayerDistance;

    private Dictionary<string, Person> view = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (var registry in buildings)
        {
            foreach (var prefab in registry.GetView())
            {
                GameObject gameObject = Instantiate(prefab);
                Person person = gameObject.GetComponent<Person>();
                person.player = player;
                view.Add(gameObject.name, person);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (var persons in view)
        {
            float dist = Vector2.Distance(persons.Value.transform.position, player.transform.position);
            if (dist < detectionPlayerDistance)
            {
                persons.Value.ClosedPlayer();
            }
            else
            {
                persons.Value.FarPlayer();
            }
        }
    }
}
