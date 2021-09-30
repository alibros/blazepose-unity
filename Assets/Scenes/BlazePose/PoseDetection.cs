using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class PoseDetection : MonoBehaviour
{
    public static int maplen = 5;
    float[,] outputs = new float[1,maplen];
    float[,,] inputs = new float[1,33,4];
    [SerializeField] string poseModel;
    [SerializeField] string poseLabels = "map.json";
    BlazePoseSample BlazePoseSampleScript;

    [SerializeField] public ArrayList inputShow = new ArrayList();

    private Interpreter interpreter;
    private bool isProcessing;

    [SerializeField] public Vector4[] jointsToShow = new Vector4[33];
    [SerializeField] public float[] outputsToShow = new float[maplen];

    [SerializeField] Text poseText;





    [System.Serializable]
    public class ModelMap {
        public string[] labels;
        public string model_type;
    }

    ModelMap modelMap = new ModelMap();


    // Start is called before the first frame update
    void Start()
    {
        GameObject BlazePoseObject = GameObject.Find("BlazePose");
        BlazePoseSampleScript   = BlazePoseObject.GetComponent<BlazePoseSample>();
        StartInterpreter();

        string jsonPath = Application.streamingAssetsPath + "/" + poseLabels;
        string jsonStr = File.ReadAllText(jsonPath);
        modelMap = JsonUtility.FromJson<ModelMap>(jsonStr);
        Debug.Log("lables: " + modelMap.labels.Length);


    }

    // Update is called once per frame
    void Update()
    {
      if (BlazePoseSampleScript!=null && BlazePoseSampleScript.landmarkResult!=null) {
        //Debug.Log("LANDMARKS=============="+BlazePoseSampleScript.landmarkResult.joints.ToString());
        for (int i = 0; i < BlazePoseSampleScript.landmarkResult.joints.Length; i++)
        {
          inputs[0,i,0] = BlazePoseSampleScript.landmarkResult.joints[i][0];
          inputs[0,i,1] = BlazePoseSampleScript.landmarkResult.joints[i][1];
          inputs[0,i,2] = BlazePoseSampleScript.landmarkResult.joints[i][2];
          inputs[0,i,3] = BlazePoseSampleScript.landmarkResult.joints[i][3];

          jointsToShow[i] = new Vector4(inputs[0,i,0],inputs[0,i,1],inputs[0,i,2],inputs[0,i,3]);
        }

       Debug.Log("INPUT:  " +  inputs.ToString());

       //Test Sample Datat
      // setInputToCannedData();


        // Set input data
      interpreter.SetInputTensorData(0, inputs);
    //
    // // Blackbox!!
    interpreter.Invoke();
    //
    // Get data
    interpreter.GetOutputTensorData(0, outputs);
    Debug.Log("OUTPUT:  " +  outputs[0,0]);
    for (int i = 0; i <maplen; i++){
      outputsToShow[i] = outputs[0,i];
    }
    float maxValue = outputsToShow.Max();
    int maxIndex = outputsToShow.ToList().IndexOf(maxValue);
    poseText.text = modelMap.labels[maxIndex];



      }




    }

    void StartInterpreter()
{

    var options = new InterpreterOptions()
    {
        threads = 2,
        useNNAPI = false,
    };
    interpreter = new Interpreter(FileUtil.LoadFile(poseModel), options);
    var inputInfo = interpreter.GetInputTensorInfo(0);
    var outputInfo = interpreter.GetOutputTensorInfo(0);

    Debug.Log("INNNNNPPPPPIUUUUUUUTTTS: " + inputInfo);
    Debug.Log("Outttttppuuuuuuuuuuuuuuts: " + outputInfo);
    interpreter.ResizeInputTensor(0, new int[] { 1, 33, 4});
    interpreter.AllocateTensors();
}

[System.Serializable]
public class Landmark
{
    public float x;
    public float y;
    public float z;
    public float v;
    public float p;
}

[System.Serializable]
public class TestPoseData
{
    public string pose_name;
    public Landmark[] landmarks;
}



void setInputToCannedData() {

  string jsonPath = Application.streamingAssetsPath + "/" + "static_test_pose.json";
  string jsonStr = File.ReadAllText(jsonPath);
  Debug.Log("JSON STRING: " + jsonStr);
  TestPoseData samplePose = JsonUtility.FromJson<TestPoseData>(jsonStr);
  Debug.Log("DATA SAMPLE POSE NAME: " + samplePose.pose_name);
  if(samplePose.landmarks==null){
    Debug.Log("NULLLLLLLL");
  }

  for (int i = 0; i < samplePose.landmarks.Length; i++)
  {
    Landmark lm = samplePose.landmarks[i];
    inputs[0,i,0] = lm.x;
    inputs[0,i,1] = lm.y;
    inputs[0,i,2] = lm.z;
    inputs[0,i,3] = lm.v;

    jointsToShow[i] = new Vector4(inputs[0,i,0],inputs[0,i,1],inputs[0,i,2],inputs[0,i,3]);
  }






}

private static double[,] cannedLeftRight = {
    {
        0.4581562578678131,
        0.4718441069126129,
        -0.7441457509994507,
        0.999978244304657
    },
    {
        0.4762577414512634,
        0.4166503846645355,
        -0.6855759024620056,
        0.999940812587738
    },
    {
        0.48890742659568787,
        0.4183814525604248,
        -0.6856846213340759,
        0.9999299645423889
    },
    {
        0.4999203085899353,
        0.41918399930000305,
        -0.6854707598686218,
        0.9999319314956665
    },
    {
        0.4364975392818451,
        0.41060078144073486,
        -0.6871957182884216,
        0.9999286532402039
    },
    {
        0.4229370057582855,
        0.4087132215499878,
        -0.6871799230575562,
        0.9999261498451233
    },
    {
        0.4092106819152832,
        0.4073556363582611,
        -0.6868606805801392,
        0.9999369978904724
    },
    {
        0.5140172243118286,
        0.4354781210422516,
        -0.3134737014770508,
        0.9999483227729797
    },
    {
        0.3843310475349426,
        0.41576653718948364,
        -0.3173333704471588,
        0.9999372959136963
    },
    {
        0.48036009073257446,
        0.5406127572059631,
        -0.6045722365379333,
        0.9999841451644897
    },
    {
        0.42927584052085876,
        0.5368380546569824,
        -0.6065736413002014,
        0.9999793171882629
    },
    {
        0.5966054201126099,
        0.7473710775375366,
        -0.14490827918052673,
        0.9998167753219604
    },
    {
        0.2733149230480194,
        0.6853494644165039,
        -0.28663671016693115,
        0.99991375207901
    },
    {
        0.6464874744415283,
        1.1164023876190186,
        -0.1795036941766739,
        0.8242507576942444
    },
    {
        0.022135447710752487,
        0.649025559425354,
        -0.869708240032196,
        0.9971326589584351
    },
    {
        0.666108250617981,
        1.5373026132583618,
        -0.4779864251613617,
        0.4408615529537201
    },
    {
        0.19321247935295105,
        0.32265469431877136,
        -1.401546597480774,
        0.9878485202789307
    },
    {
        0.697417140007019,
        1.6571182012557983,
        -0.5073074102401733,
        0.3100578486919403
    },
    {
        0.23056480288505554,
        0.20942969620227814,
        -1.5190975666046143,
        0.9422945976257324
    },
    {
        0.6742565035820007,
        1.660946011543274,
        -0.5634448528289795,
        0.4236919581890106
    },
    {
        0.259833961725235,
        0.22711491584777832,
        -1.433106541633606,
        0.9465038776397705
    },
    {
        0.6554924249649048,
        1.6236611604690552,
        -0.5088503956794739,
        0.4479939937591553
    },
    {
        0.25803107023239136,
        0.27625733613967896,
        -1.3878445625305176,
        0.9493153691291809
    },
    {
        0.5326504111289978,
        1.5485570430755615,
        0.020177248865365982,
        0.004439655225723982
    },
    {
        0.3181159198284149,
        1.5542148351669312,
        -0.01404696237295866,
        0.007579354103654623
    },
    {
        0.521728515625,
        2.2590553760528564,
        -0.22609713673591614,
        0.00006692265014862642
    },
    {
        0.3134002387523651,
        2.2521958351135254,
        -0.15085864067077637,
        0.00006654843309661373
    },
    {
        0.5075073838233948,
        2.8803493976593018,
        0.16103509068489075,
        0
    },
    {
        0.3028521239757538,
        2.8844637870788574,
        0.32196980714797974,
        0.0000014272766293288441
    },
    {
        0.5109896659851074,
        2.9668796062469482,
        0.18197332322597504,
        0.0000015546465874649584
    },
    {
        0.2937973737716675,
        2.972033739089966,
        0.35424262285232544,
        0.0000013679652965947753
    },
    {
        0.4904126226902008,
        3.066995859146118,
        -0.20905128121376038,
        0.000004491237177717267
    },
    {
        0.33994370698928833,
        3.0725388526916504,
        -0.04082145541906357,
        0.000008075334335444495
    }
};

}
