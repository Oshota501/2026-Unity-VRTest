using UnityEngine;

public class Lookatter : MonoBehaviour
{
    [SerializeField] private Transform Target;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(Target);
    }
}
