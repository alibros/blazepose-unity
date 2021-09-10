using System.Threading;
using Cysharp.Threading.Tasks;
using TensorFlowLite;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// BlazePose form MediaPipe
/// https://github.com/google/mediapipe
/// https://viz.mediapipe.dev/demo/pose_tracking
/// </summary>
public sealed class BlazePoseSample : MonoBehaviour
{
    public bool DrawStickFigure { get { return _drawStickFigure; } set { _drawStickFigure = value; } }

    [Header("Model files")]
    [SerializeField, FilePopup("*.tflite")] string poseDetectionModelFile = "mediapipe/pose_detection.tflie";
    [SerializeField, FilePopup("*.tflite")] string poseLandmarkModelFile = "mediapipe/pose_landmarks.tflite";

	[Header("Camera Settings")]
	[Tooltip("Used when running on phones or tablets")]
	[SerializeField] bool useFrontFacingCamera;
	[Tooltip("example: FaceTime HD Camera (Built-in) " )]
	[SerializeField] string customCameraName;
	[SerializeField] int resolutionW = 1280;
	[SerializeField] int resolutionH = 720;
	[SerializeField] int frameRate = 30;


	[Header("Skeletal objects")]
	[SerializeField] GameObject nose;

	[SerializeField] GameObject leftEyeInner;
    [SerializeField] GameObject leftEye;
	[SerializeField] GameObject leftEyeOuter;

    [SerializeField] GameObject rightEyeInner;
    [SerializeField] GameObject rightEye;
    [SerializeField] GameObject rightEyeOuter;

    [SerializeField] GameObject leftEar;
    [SerializeField] GameObject rightEar;

    [SerializeField] GameObject leftMouth;
    [SerializeField] GameObject rightMouth;

	[SerializeField] GameObject leftShoulder;
    [SerializeField] GameObject rightShoulder;

    [SerializeField] GameObject leftElbow;
    [SerializeField] GameObject rightElbow;

    [SerializeField] GameObject leftWrist;
    [SerializeField] GameObject rightWrist;

	[SerializeField] GameObject leftPinky;
	[SerializeField] GameObject rightPinky;

    [SerializeField] GameObject leftIndex;
    [SerializeField] GameObject rightIndex;

    [SerializeField] GameObject leftThumb;
    [SerializeField] GameObject rightThumb;

    [SerializeField] GameObject leftHip;
    [SerializeField] GameObject rightHip;

    [SerializeField] GameObject leftKnee;
    [SerializeField] GameObject rightKnee;

    [SerializeField] GameObject leftAnkle;
    [SerializeField] GameObject rightAnkle;

    [SerializeField] GameObject leftHeel;
    [SerializeField] GameObject rightHeel;

    [SerializeField] GameObject leftFootIndex;
    [SerializeField] GameObject rightFootIndex;




    // [SerializeField] // for debug raw data
    public Vector4[] worldJoints;

	[Header("GUI Settings")]
	[SerializeField] private bool _drawStickFigure = true;
	[SerializeField] RawImage cameraView = null;
    [SerializeField] Canvas canvas = null;
      [SerializeField] Text infoText = null;


	[Header("Other Settings")]
    [SerializeField] bool useLandmarkFilter = true;
    [SerializeField] Vector3 filterVelocityScale = Vector3.one * 10;
    [SerializeField] bool runBackground;
    [SerializeField, Range(0f, 1f)] float visibilityThreshold = 0.5f;



    WebCamTexture webcamTexture;
    PoseDetect poseDetect;
    PoseLandmarkDetect poseLandmark;

    Vector3[] rtCorners = new Vector3[4]; // just cache for GetWorldCorners


    PrimitiveDraw draw;
    PoseDetect.Result poseResult;
    PoseLandmarkDetect.Result landmarkResult;
    UniTask<bool> task;
    CancellationToken cancellationToken;

    bool NeedsDetectionUpdate => poseResult == null || poseResult.score < 0.5f;

    void Start()
    {

   infoText.text =  "Starting";


        // Init camera
        string cameraName = WebCamUtil.FindName(new WebCamUtil.PreferSpec()
        {
            isFrontFacing = useFrontFacingCamera,
            kind = WebCamKind.WideAngle,
        });
          infoText.text = "Camera: " + cameraName;


		if (customCameraName != null) {
		webcamTexture = new WebCamTexture(customCameraName, resolutionW, resolutionH, frameRate);
		} else {
	 	webcamTexture = new WebCamTexture(cameraName, resolutionW, resolutionH, frameRate);
		}


        cameraView.texture = webcamTexture;
        webcamTexture.Play();
        Debug.Log($"Starting camera: {cameraName}");

        // Init model
        poseDetect = new PoseDetect(poseDetectionModelFile);
        poseLandmark = new PoseLandmarkDetect(poseLandmarkModelFile);

        infoText.text = "Camera: " + cameraName + "PosesInitied";
        draw = new PrimitiveDraw(Camera.main, gameObject.layer);
        worldJoints = new Vector4[PoseLandmarkDetect.JointCount];

        cancellationToken = this.GetCancellationTokenOnDestroy();
    }

    void OnDestroy()
    {
        webcamTexture?.Stop();
        poseDetect?.Dispose();
        poseLandmark?.Dispose();
        draw?.Dispose();
    }

    void Update()
    {
        if (runBackground)
        {
            if (task.Status.IsCompleted())
            {
                task = InvokeAsync();
            }
        }
        else
        {
            Invoke();
        }

        if (poseResult != null && poseResult.score > 0f)
        {
            DrawFrame(poseResult);
			Debug.Log("Pose results" + poseResult);
        }

        if (landmarkResult != null && landmarkResult.score > 0.2f)
        {
            DrawCropMatrix(poseLandmark.CropMatrix);
            DrawJoints(landmarkResult.joints);

        }


		// Update the position of game objects
		if (worldJoints != null) {

            nose.transform.position = GetPoseFor(PoseLandmarks.NOSE);

            leftEyeInner.transform.position = GetPoseFor(PoseLandmarks.LEFT_EYE_INNER);
            leftEye.transform.position = GetPoseFor(PoseLandmarks.LEFT_EYE);
            leftEyeOuter.transform.position = GetPoseFor(PoseLandmarks.LEFT_EYE_OUTER);

            rightEyeInner.transform.position = GetPoseFor(PoseLandmarks.RIGHT_EYE_INNER);
            rightEye.transform.position = GetPoseFor(PoseLandmarks.RIGHT_EYE);
            rightEyeOuter.transform.position = GetPoseFor(PoseLandmarks.RIGHT_EYE_OUTER);

            leftEar.transform.position = GetPoseFor(PoseLandmarks.LEFT_EAR);
            rightEar.transform.position = GetPoseFor(PoseLandmarks.RIGHT_EAR);

            leftMouth.transform.position = GetPoseFor(PoseLandmarks.LEFT_MOUTH);
            rightMouth.transform.position = GetPoseFor(PoseLandmarks.RIGHT_MOUTH);

            leftShoulder.transform.position = GetPoseFor(PoseLandmarks.LEFT_SHOULDER);
            rightShoulder.transform.position = GetPoseFor(PoseLandmarks.RIGHT_SHOULDER);

            leftElbow.transform.position = GetPoseFor(PoseLandmarks.LEFT_ELBOW);
            rightElbow.transform.position = GetPoseFor(PoseLandmarks.RIGHT_ELBOW);

            leftWrist.transform.position = GetPoseFor(PoseLandmarks.LEFT_WRIST);
            rightWrist.transform.position = GetPoseFor(PoseLandmarks.RIGHT_WRIST);

            leftPinky.transform.position = GetPoseFor(PoseLandmarks.LEFT_PINKY);
            rightPinky.transform.position = GetPoseFor(PoseLandmarks.RIGHT_PINKY);

            leftIndex.transform.position = GetPoseFor(PoseLandmarks.LEFT_INDEX);
            rightIndex.transform.position = GetPoseFor(PoseLandmarks.RIGHT_INDEX);

            leftThumb.transform.position = GetPoseFor(PoseLandmarks.LEFT_THUMB);
            rightThumb.transform.position = GetPoseFor(PoseLandmarks.RIGHT_THUMB);

            leftHip.transform.position = GetPoseFor(PoseLandmarks.LEFT_HIP);
            rightHip.transform.position = GetPoseFor(PoseLandmarks.RIGHT_HIP);

            leftKnee.transform.position = GetPoseFor(PoseLandmarks.LEFT_KNEE);
            rightKnee.transform.position = GetPoseFor(PoseLandmarks.RIGHT_KNEE);

            leftAnkle.transform.position = GetPoseFor(PoseLandmarks.LEFT_ANKLE);
            rightAnkle.transform.position = GetPoseFor(PoseLandmarks.RIGHT_ANKLE);

            leftHeel.transform.position = GetPoseFor(PoseLandmarks.LEFT_HEEL);
            rightHeel.transform.position = GetPoseFor(PoseLandmarks.RIGHT_HEEL);

            leftFootIndex.transform.position = GetPoseFor(PoseLandmarks.LEFT_FOOT_INDEX);
            rightFootIndex.transform.position = GetPoseFor(PoseLandmarks.RIGHT_FOOT_INDEX);
        }

    }


    private Vector3 GetPoseFor(int jointId) {
        if (worldJoints != null) {
            return new Vector3(worldJoints[jointId][0], worldJoints[jointId][1], worldJoints[jointId][2]);
        }

        return Vector3.zero;
    }

    void DrawFrame(PoseDetect.Result pose)
    {
        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];

        draw.color = Color.green;
        draw.Rect(MathTF.Lerp(min, max, pose.rect, true), 0.02f, min.z);

        foreach (var kp in pose.keypoints)
        {
            draw.Point(MathTF.Lerp(min, max, (Vector3)kp, true), 0.05f);
        }
        draw.Apply();
    }

    void DrawCropMatrix(in Matrix4x4 matrix)
    {
        draw.color = Color.red;

        Vector3 min = rtCorners[0];
        Vector3 max = rtCorners[2];

        var mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored)
            * matrix.inverse;
        Vector3 a = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(0, 0, 0)));
        Vector3 b = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(1, 0, 0)));
        Vector3 c = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(1, 1, 0)));
        Vector3 d = MathTF.LerpUnclamped(min, max, mtx.MultiplyPoint3x4(new Vector3(0, 1, 0)));

        draw.Quad(a, b, c, d, 0.02f);
        draw.Apply();
    }

    void DrawJoints(Vector4[] joints)
    {
        draw.color = Color.blue;

        // Vector3 min = rtCorners[0];
        // Vector3 max = rtCorners[2];
        // Debug.Log($"rtCorners min: {min}, max: {max}");

        // Apply webcam rotation to draw landmarks correctly
        Matrix4x4 mtx = WebCamUtil.GetMatrix(-webcamTexture.videoRotationAngle, false, webcamTexture.videoVerticallyMirrored);

        // float zScale = (max.x - min.x) / 2;
        float zScale = 1;
        float zOffset = canvas.planeDistance;
        float aspect = (float)Screen.width / (float)Screen.height;
        Vector3 scale, offset;
        if (aspect > 1)
        {
            scale = new Vector3(1f / aspect, 1f, zScale);
            offset = new Vector3((1 - 1f / aspect) / 2, 0, zOffset);
        }
        else
        {
            scale = new Vector3(1f, aspect, zScale);
            offset = new Vector3(0, (1 - aspect) / 2, zOffset);
        }

        // Update world joints
        var camera = canvas.worldCamera;
        for (int i = 0; i < joints.Length; i++)
        {
            Vector3 p = mtx.MultiplyPoint3x4((Vector3)joints[i]);
            p = Vector3.Scale(p, scale) + offset;
            p = camera.ViewportToWorldPoint(p);

            // w is visibility
            worldJoints[i] = new Vector4(p.x, p.y, p.z, joints[i].w);
        }



		// Draw
		if (_drawStickFigure){
        for (int i = 0; i < worldJoints.Length; i++)
        {
            Vector4 p = worldJoints[i];
            if (p.w > visibilityThreshold)
            {
                draw.Cube(p, 0.2f);
            }
        }
        var connections = PoseLandmarkDetect.Connections;
        for (int i = 0; i < connections.Length; i += 2)
        {
            var a = worldJoints[connections[i]];
            var b = worldJoints[connections[i + 1]];
            if (a.w > visibilityThreshold || b.w > visibilityThreshold)
            {
                draw.Line3D(a, b, 0.05f);
            }
        }
		}
        draw.Apply();
    }

    void Invoke()
    {
        if (NeedsDetectionUpdate)
        {
            poseDetect.Invoke(webcamTexture);
            cameraView.material = poseDetect.transformMat;
            cameraView.rectTransform.GetWorldCorners(rtCorners);
            poseResult = poseDetect.GetResults(0.7f, 0.3f);
        }
        if (poseResult.score < 0)
        {
            poseResult = null;
            landmarkResult = null;
            return;
        }
        poseLandmark.Invoke(webcamTexture, poseResult);


        if (useLandmarkFilter)
        {
            poseLandmark.FilterVelocityScale = filterVelocityScale;
        }
        landmarkResult = poseLandmark.GetResult(useLandmarkFilter);

        if (landmarkResult.score < 0.3f)
        {
            poseResult.score = landmarkResult.score;
        }
        else
        {
            poseResult = PoseLandmarkDetect.LandmarkToDetection(landmarkResult);
        }
    }

    async UniTask<bool> InvokeAsync()
    {
        if (NeedsDetectionUpdate)
        {
            // Note: `await` changes PlayerLoopTiming from Update to FixedUpdate.
            poseResult = await poseDetect.InvokeAsync(webcamTexture, cancellationToken, PlayerLoopTiming.FixedUpdate);
        }
        if (poseResult.score < 0)
        {
            poseResult = null;
            landmarkResult = null;
            return false;
        }

        if (useLandmarkFilter)
        {
            poseLandmark.FilterVelocityScale = filterVelocityScale;
        }
        landmarkResult = await poseLandmark.InvokeAsync(webcamTexture, poseResult, useLandmarkFilter, cancellationToken, PlayerLoopTiming.Update);

        // Back to the update timing from now on
        if (cameraView != null)
        {
            cameraView.material = poseDetect.transformMat;
            cameraView.rectTransform.GetWorldCorners(rtCorners);
        }


        // Generate poseResult from landmarkResult
        if (landmarkResult.score < 0.3f)
        {
            poseResult.score = landmarkResult.score;
        }
        else
        {
            poseResult = PoseLandmarkDetect.LandmarkToDetection(landmarkResult);
        }

        return true;
    }
}
