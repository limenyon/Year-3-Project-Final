using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    Vector3 pos;
    bool goingUp = true;
    int counter = 0;
    // Start is called before the first frame update
    void Start()
    {
        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        counter++;
        if(counter >= 1000)
        {
            goingUp = !goingUp;
            counter = 0;
        }
        if(goingUp)
        {
            pos.x += 0.005f;
        }
        else
        {
            pos.x -= 0.005f;
        }

        transform.position = pos;
    }
}
