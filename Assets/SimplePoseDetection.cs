using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;
using System.IO;
using System.Linq;
using UnityEngine.UI;
using System;

public class SimplePoseDetection : MonoBehaviour
{

    BlazePoseSample BlazePoseSampleScript;
    int frameCounter = 0;
    //averaging a gesture over a number of frames for better stability
    public int maxFrameCount = 8;
    int[] poseFrames = { 0,0,0,0};
    //Index of Detected pose 0: None, 1: Y, 2: O, 3: X 
    public int currentPoseIndex = 0;
    [SerializeField] Text poseText;

    // Start is called before the first frame update
    void Start()
    {
        GameObject BlazePoseObject = GameObject.Find("BlazePose");
        BlazePoseSampleScript = BlazePoseObject.GetComponent<BlazePoseSample>();
        
    }

    // Update is called once per frame
    void Update()
    {
    


        if (BlazePoseSampleScript == null || BlazePoseSampleScript.landmarkResult == null)
        {
            return;
        }
        if (frameCounter > maxFrameCount)
        {
            currentPoseIndex = poseFrames.ToList().IndexOf(poseFrames.Max());
            frameCounter = 0;
            poseFrames = new int[] { 0, 0, 0, 0 };
            poseText.text = currentPoseIndex.ToString();

        } else
        {
            frameCounter = frameCounter + 1;
            GetCurrentPoseIndex();
            
        }


    }



    void GetCurrentPoseIndex()
    {

       


        Vector4[] joints = BlazePoseSampleScript.landmarkResult.joints;
        //Debug.Log("HAND DISTANCE: " + distance(joints[16],joints[15]));
        //Y
        if (joints[13][1] > joints[11][1]
            && joints[14][1] > joints[12][1]
            && joints[15][1] > joints[3][1]
            && joints[16][1] > joints[3][1]
            && joints[15][0] < joints[13][0]
            && joints[14][0] < joints[16][0]
            && distance(joints[16], joints[15]) > 0.6)
        {
            
            poseFrames[1] = poseFrames[1] + 1;

        }
        //O
        else if (distance(joints[16], joints[15])<0.25
            && joints[15][1] > joints[11][1]
            && joints[16][1] > joints[12][1]) {
            poseFrames[2] = poseFrames[2] + 1;
        }

        //X
        else if (distance(joints[16], joints[15]) < 0.25
            && joints[13][1] < joints[11][1]
            && joints[14][1] < joints[12][1])
        {
            poseFrames[3] = poseFrames[3] + 1;
        }
        //None;
        else if (

            joints[15][1] < joints[13][1]
            && joints[16][1] < joints[14][1]
            && distance(joints[13],joints[23])<0.3
            && distance(joints[16], joints[24]) < 0.3


            )
        {
            poseFrames[0] = poseFrames[0] + 1;
        }



    }

    static double distance(Vector4 j1, Vector4 j2)
    {
        // Calculating distance
        return Math.Sqrt(Math.Pow(j2[0] - j1[0], 2) +
                      Math.Pow(j2[1] - j1[1], 2) * 1.0);
    }

}
