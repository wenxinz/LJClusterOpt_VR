using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementControl : MonoBehaviour {

    private float sideWidth = .5f;
    private float speed = 1.0f;

    private Vector3? MovingDirection()
    {
        // Do not move
        if (GvrControllerInput.IsTouching == false || GvrControllerInput.ClickButtonDown == false)
        {
            return null;
        }

        float yPos = GvrControllerInput.TouchPosCentered.y;

        // Move forward
        if(yPos >= sideWidth)
        {
            return new Vector3(0.0f, 0.0f, 1.0f);
        }

        // Move backward
        if(yPos <= -sideWidth)
        {
            return new Vector3(0.0f, 0.0f, -1.0f);
        }

        return null;
    }

	// Update is called once per frame
	void Update () {

        Vector3? direction = MovingDirection();

        if(direction != null)
        {
            transform.Translate((Vector3)direction * speed);
        }
    }

}
