using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TensorFlowLite;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class PoseDetection : MonoBehaviour
{
    static int maplen = 6;
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
}
