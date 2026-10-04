using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public float inputAngle;
    void Start()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    void Update()
    {
        Gyroscope gyro = Input.gyro;
        gyro.enabled = true;

        Quaternion gyroRotation = Input.gyro.attitude;
        Vector3 eulerAngle = gyroRotation.eulerAngles;

        float tempinputAngle = Math.Abs(90f-eulerAngle.y);
        if (tempinputAngle > 180){
            inputAngle = 180-tempinputAngle;
        }
        else{
            inputAngle = tempinputAngle;
        }

        Debug.Log(inputAngle);
    }
}
