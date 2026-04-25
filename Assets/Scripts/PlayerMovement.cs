using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovement : MonoBehaviour

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private float speed = 1f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float xInput = Input.GetAxisRaw("Horizontal");
        Vector3 movement = new Vector3(xInput, 0, 0);

        movement.Normalize();

        transform.Translate(movement * speed * Time.deltaTime);

    }
}
