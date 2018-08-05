using System.Collections.Generic;
using GoogleARCore;
using GoogleARCore.Examples.HelloAR;
using UnityEngine;

#if UNITY_EDITOR
// Set up touch input propagation while using Instant Preview in the editor.
using Input = GoogleARCore.InstantPreviewInput;
#endif

public class BowlTouchMove : MonoBehaviour
{
    //AR Camera
    public Camera FirstPersonCamera;

    //save the "Input.GetTouch(0).deltaPosition;" 
    private Vector3 touchposition;

    private Vector3 initPos;
    void Update()
    {
        //When the user slides the touch, he should be able to control the bowling to move on the screen.
        if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved)) 
        {
            //Make sure the bowling ball doesn't fall off due to gravity when the user controls it
            GetComponent<Rigidbody>().useGravity = false;
            //Get the position delta since last change.
            touchposition = Input.GetTouch(0).deltaPosition; 
            //Move the bowl
            this.transform.Translate(touchposition.x * 0.003f, touchposition.y * 0.003f, 0);//(Editor model)
            //this.transform.Translate(touchposition.x * 0.0007f, touchposition.y * 0.0007f, 0);//(Real devices model)
        }

        //When touching is ended, bowl should be thrown 
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended) {
            //save the instant position when touching is ended
            Vector3 currentPos = Input.GetTouch(0).position;
            //currentPos.z == 0, so we give it a z-axis value
            currentPos.z = 20f;
            //Calculate one position according to currentPos
            initPos = FirstPersonCamera.ScreenToWorldPoint(currentPos);
            //Calculate the other position
            Vector3 newPos = FirstPersonCamera.ScreenToWorldPoint(new Vector3(currentPos.x,
                                                                              currentPos.y,
                                                                              Mathf.Clamp(currentPos.y / 10, 5, 70)));

            //We set Gravity false just now. When the bowl is thrown, it should have gravity
            GetComponent<Rigidbody>().useGravity = true;

            //Give bowl a force to move
            addForce(newPos);
            this.transform.parent = null;
        }    
    }

    //Add a force to bowl to move
    void addForce(Vector3 newPos)
    {
        newPos.y += 5;
        this.GetComponent<Rigidbody>().AddForce(((newPos - initPos).normalized)
                                                     * (Vector3.Distance(newPos, initPos))
                                                     * 10
                                                     * this.GetComponent<Rigidbody>().mass);
    }
}