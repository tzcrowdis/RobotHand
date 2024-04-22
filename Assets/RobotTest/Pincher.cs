using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class Pincher : MonoBehaviour
{
    public InputDevice leftController;
    public InputDevice rightController;

    public XRDirectInteractor rHand;
    public XRDirectInteractor lHand;

    public Transform leftPincher;
    public Transform rightPincher;

    bool grabbed;
    bool grabbedLeft;

    float maxRot = 20f;
    Vector3 closed = Vector3.zero;
    Vector3 rightOpen;
    Vector3 leftOpen;
    Vector3 clamped = new Vector3(2.5f, 0f, 0f); //stop point when holding needle

    float rotationSpeed = 0.5f;
    Vector3 currentRotation = Vector3.zero;

    float clampThresh = 0.5f;
    float breakThresh = 0.8f;

    GameObject needle;
    bool needleInRange = false;

    void Start()
    {
        grabbed = false;
        grabbedLeft = false;

        rightOpen = new Vector3(-maxRot, 0, 0);
        leftOpen = new Vector3(maxRot, 0, 0);
    }

    void Update()
    {
        if (leftController == null | rightController == null)
            FindControllers();
        else
        {
            //determine if the object is grabbed and by which hand
            if (rHand.hasSelection & lHand.hasSelection)
            {
                Debug.Log("WARNING! ONLY USE ONE HAND!");
            }
            else if (rHand.hasSelection)
            {
                grabbed = true;
            }
            else if (lHand.hasSelection)
            {
                grabbed = true;
                grabbedLeft = true;
            }
            else
            {
                grabbed = false;
                grabbedLeft = false;
            }

            //when grabbed rotate pincher based on trigger value of grabbed hand
            if (grabbed)
                MovePincher(grabbedLeft);

            HoldNeedle();
        }
    }

    void MovePincher(bool left)
    {
        //get trigger value 
        float triggerValue;
        if (left)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            leftController.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
        }
        else
        {
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            rightController.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
        }

        if (triggerValue == 0)
        {
            //open robot hand when trigger isn't held
            if (leftPincher.localEulerAngles.x < maxRot)
            {
                leftPincher.Rotate(rotationSpeed, 0f, 0f);
                rightPincher.Rotate(-rotationSpeed, 0f, 0f);
            }
            else
            {
                leftPincher.localEulerAngles = leftOpen;
                rightPincher.localEulerAngles = rightOpen;
            }
        }
        else if (triggerValue <= clampThresh)
        {
            //rotate robot hand so open at trigger value 0 and closed at 0.7
            currentRotation.x = -maxRot * triggerValue / clampThresh + maxRot;
            if (leftPincher.localEulerAngles.x > currentRotation.x)
            {
                leftPincher.Rotate(-rotationSpeed, 0f, 0f);
                rightPincher.Rotate(rotationSpeed, 0f, 0f);
            }
            else if (leftPincher.localEulerAngles.x - 1 <= currentRotation.x & currentRotation.x <= leftPincher.localEulerAngles.x + 1)
            {
                //TODO: smooth motion of pinchers better near the trigger value
                leftPincher.localEulerAngles = currentRotation;
                rightPincher.localEulerAngles = -currentRotation;
            }
            else
            {
                leftPincher.Rotate(rotationSpeed, 0f, 0f);
                rightPincher.Rotate(-rotationSpeed, 0f, 0f);
            }
        }
        else if (triggerValue <= breakThresh)
        {
            //keep robot hand at closed position
            if (leftPincher.localEulerAngles.x > closed.x & leftPincher.localEulerAngles.x < maxRot)
            {
                leftPincher.Rotate(-rotationSpeed, 0f, 0f);
                rightPincher.Rotate(rotationSpeed, 0f, 0f);
            }
            else
            {
                leftPincher.localEulerAngles = closed;
                rightPincher.localEulerAngles = -closed;
            }
        }
        else
        {
            //if needle is held then it breaks under this pressure
            if (leftPincher.localEulerAngles.x > closed.x & leftPincher.localEulerAngles.x < maxRot)
            {
                leftPincher.Rotate(-rotationSpeed, 0f, 0f);
                rightPincher.Rotate(rotationSpeed, 0f, 0f);
            }
            else
            {
                leftPincher.localEulerAngles = closed;
                rightPincher.localEulerAngles = -closed;

                //check if needle held here
                if (closed == clamped)
                {
                    if (needle != null)
                    {
                        Debug.Log("NEEDLE BROKEN!");
                        needle.gameObject.GetComponent<Needle>().DestroyNeedle();
                        needle = null;
                    }
                }
            }
        }
    }

    //assigns the controllers if they aren't found for any reason
    void FindControllers()
    {
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    //set needle object
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Needle")
        {
            needleInRange = true;

            closed = clamped;

            if (needle == null)
                needle = other.gameObject;
        }
    }

    //handle holding needle
    void HoldNeedle()
    {
        if (needleInRange & needle != null)
        {
            if (leftPincher.localEulerAngles == clamped)
            {
                if (!needle.gameObject.GetComponent<Needle>().locked)
                    needle.gameObject.GetComponent<Needle>().LockMotion(gameObject);
            }
            else
            {
                if (needle.gameObject.GetComponent<Needle>().locked)
                    needle.gameObject.GetComponent<Needle>().UnlockMotion();
            }
        }
    }

    //needle dropped
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Needle")
        {
            needleInRange = false;
            
            closed = Vector3.zero;

            if (other.gameObject.GetComponent<Needle>().locked)
                other.gameObject.GetComponent<Needle>().UnlockMotion();
        }
    }
}
