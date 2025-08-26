using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public float speed = 4.5f;
    CharacterController cc;
    // Start is called before the first frame update
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 move = transform.right * Input.GetAxis("Horizontal")
            + transform.forward * Input.GetAxis("Vertical");
        cc.Move(move * speed * Time.deltaTime);
    }
}
