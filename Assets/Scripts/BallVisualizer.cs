using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallVisualizer : MonoBehaviour
{
    const int maxVisualPointNumber = 20; // show maximum this number of points in the scene
    const int maxVisualLineNumber = 2; // show maximum this number of lines in the scene
    int visualPointIndex = 0;
    int visualLineIndex = 0;
    int visualRecordPointIndex = 0;
    int visualRecordLineIndex = 0;
    public GameObject prefabBall;
    public GameObject prefabCorner;
    public GameObject table;
    public GameObject tableAnchor;
    public GameObject replayText;
    List<GameObject> visualPoints = new List<GameObject>(maxVisualPointNumber);
    List<GameObject> visualLines = new List<GameObject>(maxVisualLineNumber);
    List<GameObject> visualRecordPoints = new List<GameObject>(maxVisualPointNumber);
    List<GameObject> visualRecordLines = new List<GameObject>(maxVisualLineNumber);
    List<RecordTrackpoint> recordPoints = new List<RecordTrackpoint>();
    bool showBalls = true;
    bool showLines = true;
    bool isReplaying = false;
    bool testStop = false;

    struct RecordTrackpoint
    {
        public float time;
        public UDPListener.trackPoint point;
    };

    // Save a trackpoint for replay
    public void AddTrackpointToRecord(UDPListener.trackPoint trackPoint)
    {
        var obj = new RecordTrackpoint{ time = Time.time, point = trackPoint};
        recordPoints.Add(obj);
    }
    
    // Clear the replay record
    public void ClearRecord()
    {
        recordPoints.Clear();
    }

    // set isReplaying and show/hide the "Replay" text
    public void setIsReplaying(bool flag)
    {
        isReplaying = flag;
        replayText.SetActive(isReplaying);
    }

    // Replay tracks saved in the record
    IEnumerator ReplayRecordCoroutine()
    {
        if(recordPoints.Count <= 0 || !isReplaying)
        {
            yield break;
        }

        VisualizeTrackpoint(recordPoints[0].point, true);
        AddTrackPointToLine(recordPoints[0].point, true, true);
        float prevTimestmp = recordPoints[0].time;

        for(int i=1; i<recordPoints.Count; i++)
        {
            float timestmp = recordPoints[i].time;
            UDPListener.trackPoint point = recordPoints[i].point;
            yield return new WaitForSeconds(timestmp - prevTimestmp);
            if (!isReplaying)
            {
                yield break;
            }
            VisualizeTrackpoint(point, true);
            AddTrackPointToLine(point, true, true);
            prevTimestmp = timestmp;
        }

        yield return new WaitForSeconds(1.0f);
        setIsReplaying(false);
        SwitchToLiveMode();
    }

    void SwitchToReplayMode()
    {
        // hide live balls and lines
        setActiveBalls(false);
        setActiveLines(false);
        // clear record lines points
        foreach(var line in visualRecordLines)
        {
            line.GetComponent<LineRenderer>().positionCount = 0;
        }
        ReplayRecord();
    }

    void SwitchToLiveMode()
    {
        // show live balls and lines if they were visible
        if (showBalls)
        {
            setActiveBalls(true);
        }
        if (showLines)
        {
            setActiveLines(true);
        }
        // hide replay balls and lines
        setActiveRecordBalls(false);
        setActiveRecordLines(false);
    }

    public void ToggleReplay()
    {
        setIsReplaying(!isReplaying);
        if (isReplaying)
        {
            SwitchToReplayMode();
        }
        else
        {
            SwitchToLiveMode();
        }
    }

    public void ReplayRecord()
    {
        StartCoroutine("ReplayRecordCoroutine");
    }

    //public void VisualizeTrack(UDPListener.trackPoint[] track)
    //{
    //    var linePoints = new Vector3[track.Length];
    //    for(int i=0; i<track.Length; i++)
    //    {
    //        linePoints[i] = getUnityPositionInTableFrame(track[i].positionX, track[i].positionY, track[i].positionZ);
    //    }

    //    var newLine = new GameObject();
    //    newLine.transform.SetParent(tableAnchor.transform);
    //    var drawLine = newLine.AddComponent<LineRenderer>();
    //    drawLine.material = new Material(Shader.Find("Sprites/Default"));
    //    drawLine.startColor = Color.white;
    //    drawLine.endColor = Color.white;
    //    drawLine.startWidth = 0.02f;
    //    drawLine.endWidth = 0.02f;
    //    drawLine.useWorldSpace = false;

    //    drawLine.positionCount = linePoints.Length;
    //    drawLine.SetPositions(linePoints);

        
    //}

    public void AddTrackPointToLine(UDPListener.trackPoint trackPoint, bool canStartNewLine, bool isReplay=false)
    {
        var line = isReplay ? visualRecordLines[visualRecordLineIndex] : visualLines[visualLineIndex];
        line.SetActive(isReplay ? true : showLines);
        var drawLine = line.GetComponent<LineRenderer>();
        var newPoint = getUnityPositionInTableFrame(trackPoint.positionX, trackPoint.positionY, trackPoint.positionZ);
        newPoint = tableAnchor.transform.TransformPoint(newPoint); // transform to world
        if(canStartNewLine && drawLine.positionCount > 0 && (drawLine.GetPosition(drawLine.positionCount - 1) - newPoint).magnitude > 0.10f)
        {
            // new point is too far from latest point: create new line
            if (isReplay)
            {
                visualRecordLineIndex = (visualRecordLineIndex + 1) % maxVisualLineNumber;
            }
            else
            {
                visualLineIndex = (visualLineIndex + 1) % maxVisualLineNumber;
            }
            var nextLine = isReplay ? visualRecordLines[visualRecordLineIndex] : visualLines[visualLineIndex];
            nextLine.SetActive(isReplay ? true : showLines);
            var nextDrawLine = nextLine.GetComponent<LineRenderer>();
            nextDrawLine.positionCount = 1;
            nextDrawLine.SetPosition(0, newPoint);
        }
        else
        {
            drawLine.positionCount++;
            drawLine.SetPosition(drawLine.positionCount - 1, newPoint);

        }
    }

    public void VisualizeTrackpoint(UDPListener.trackPoint trackPoint, bool isReplay=false)
    {
        var posX = trackPoint.positionX;
        var posY = trackPoint.positionY;
        var posZ = trackPoint.positionZ;
        var color = trackPoint.color;

        if (!isReplay)
        {
            var visualPoint = visualPoints[visualPointIndex];
            visualPoint.transform.localPosition = getUnityPositionInTableFrame(posX, posY, posZ);
            visualPoint.SetActive(showBalls);

            visualPointIndex = (visualPointIndex + 1) % maxVisualPointNumber;
        }
        else
        {
            var visualPoint = visualRecordPoints[visualRecordPointIndex];
            visualPoint.transform.localPosition = getUnityPositionInTableFrame(posX, posY, posZ);
            visualPoint.SetActive(true);

            visualRecordPointIndex = (visualRecordPointIndex + 1) % maxVisualPointNumber;
        }
    }

    Vector3 getUnityPositionInTableFrame(int posX, int posY, double posZ, bool ignoreZ = true)
    {
        // Convert position from [-1000, 1000] scale in image frame to meters in unity frame, with origin at the table centre

        var tableWidth = table.transform.localScale.x;
        var tableLength = table.transform.localScale.y;
        var unityToImageLengthRatio = tableLength / 2000.0;
        var unityToImageWidthRatio = tableWidth / 1.0;

        var ballPosXInTableFrame = (posZ - 0.5) * unityToImageWidthRatio; // coord along table width
        var ballPosYInTableFrame = posX * unityToImageLengthRatio; // coord along table length
        var ballPosZInTableFrame = -1 * posY * unityToImageLengthRatio;  // coord along height 

        if (ignoreZ)
        {
            ballPosXInTableFrame = 0.0f;
        }

        return new Vector3((float)ballPosXInTableFrame, (float)ballPosYInTableFrame, (float)ballPosZInTableFrame);
    }

    Color getUnityColor(int color)
    {
        int A = (color >> 24) & 0xff;
        int R = (color >> 16) & 0xff;
        int G = (color >> 8) & 0xff;
        int B = color & 0xff;

        if (R == G && R == B)
        {
            // past trajectory
            return Color.white;
        }
        else
        {
            // future trajectory
            return Color.red;
        }
    }

    void DisplayTableCorners()
    {     
        var visualPoint = Instantiate(original: prefabCorner, parent: tableAnchor.transform);
        visualPoint.transform.localPosition = getUnityPositionInTableFrame(-1000, 0, 0.0, false);
        visualPoint.transform.localEulerAngles = new Vector3(-90, 0, 0);

        var renderer = visualPoint.GetComponent<Renderer>();
        renderer.material.SetColor("_Color", Color.yellow);

        visualPoint.SetActive(true);

        var visualPoint2 = Instantiate(original: prefabCorner, parent: tableAnchor.transform);
        visualPoint2.transform.localPosition = getUnityPositionInTableFrame(-1000, 0, 1.0, false);
        visualPoint2.transform.localEulerAngles = new Vector3(0, -90, 90);

        var renderer2 = visualPoint2.GetComponent<Renderer>();
        renderer2.material.SetColor("_Color", Color.red);

        visualPoint2.SetActive(true);

        var visualPoint3 = Instantiate(original: prefabCorner, parent: tableAnchor.transform);
        visualPoint3.transform.localPosition = getUnityPositionInTableFrame(1000, 0, 0.0, false);
        visualPoint3.transform.localEulerAngles = new Vector3(0, 90, -90);


        var renderer3 = visualPoint3.GetComponent<Renderer>();
        renderer3.material.SetColor("_Color", Color.green);

        visualPoint3.SetActive(true);

        var visualPoint4 = Instantiate(original: prefabCorner, parent: tableAnchor.transform);
        visualPoint4.transform.localPosition = getUnityPositionInTableFrame(1000, 0, 1.0, false);
        visualPoint4.transform.localEulerAngles = new Vector3(90, 0, 180);

        var renderer4 = visualPoint4.GetComponent<Renderer>();
        renderer4.material.SetColor("_Color", Color.blue);

        visualPoint4.SetActive(true);
    }

    float parabola(float x, float scale = 1.0f)
    {
        //scale = 1 -> max height = 500
        return -(x + 1500) * (x - 500) / 2000 * scale;
    }

    public void SimulateTrajectory()
    {
        ClearRecord();
        StartCoroutine("ShowExampleTrajectory");
    }
    IEnumerator ShowExampleTrajectory()
    {
        int maxNum = 40;
        int numExchange = 6;
        var tt = new UDPListener.trackPoint[maxNum];
        float parabolaScale = Random.Range(0.3f, 1.0f);
        float prevParScale;
        for (int k=0; k < numExchange; k++)
        {
            int fact = k % 2 == 0 ? 1 : -1; // ball goes from left to right when k even, else right to left
            int bound0 = Random.Range(15, 45);
            int bound1 = bound0 + Random.Range(3, 10);
            prevParScale = parabolaScale;
            parabolaScale = Random.Range(0.3f, 1.0f);
            for (int i = 0; i < maxNum; i++)
            {
                tt[i] = new UDPListener.trackPoint();
                int x = (2000 * i / maxNum - 1000);
                tt[i].positionX = fact * x;
                tt[i].positionY = (int)parabola(x <= 500 ? x : x - 2000, x <= 500 ? prevParScale : parabolaScale);
                tt[i].positionZ = 0.0;
                if (i < bound0 || i > bound1) // create discontinuities
                {
                    AddTrackPointToLine(tt[i], true);
                    VisualizeTrackpoint(tt[i]);
                    AddTrackpointToRecord(tt[i]);
                }
                yield return new WaitForSeconds(Random.Range(0.010f, 0.015f));
            }
        }
    }

    public void toggleBallsVisible()
    {
        showBalls = !showBalls;
        for (int i = 0; i < visualPoints.Count; i++)
        {
            visualPoints[i].SetActive(showBalls && this.gameObject.activeInHierarchy);
        }
    }

    public void toggleLinesVisible()
    {
        showLines = !showLines;
        for (int i = 0; i < visualLines.Count; i++)
        {
            visualLines[i].SetActive(showLines && this.gameObject.activeInHierarchy);
        }
    }

    public void setActiveBalls(bool flag)
    {
        for (int i = 0; i < visualPoints.Count; i++)
        {
            visualPoints[i].SetActive(flag && this.gameObject.activeInHierarchy);
        }
    }

    public void setActiveLines(bool flag)
    {
        for (int i = 0; i < visualLines.Count; i++)
        {
            visualLines[i].SetActive(flag && this.gameObject.activeInHierarchy);
        }
    }

    public void setActiveRecordBalls(bool flag)
    {
        for (int i = 0; i < visualRecordPoints.Count; i++)
        {
            visualRecordPoints[i].SetActive(flag && this.gameObject.activeInHierarchy);
        }
    }

    public void setActiveRecordLines(bool flag)
    {
        for (int i = 0; i < visualRecordLines.Count; i++)
        {
            visualRecordLines[i].SetActive(flag && this.gameObject.activeInHierarchy);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        var colors = new List<Color> { Color.red, Color.magenta, Color.yellow, Color.green, Color.cyan, Color.blue, Color.grey, Color.black };
        //instantiate all balls
        for(int i=0; i<maxVisualPointNumber; i++)
        {
            var point = Instantiate(original: prefabBall, parent: tableAnchor.transform);
            point.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 1.0f, 1.0f, 0.5f));
            point.SetActive(false);
            visualPoints.Add(point);
        }

        //instantiate all lines
        for(int k=0; k<maxVisualLineNumber; k++)
        {
            var line = new GameObject("Line_"+k);
            line.transform.SetParent(tableAnchor.transform);
            var drawLine = line.AddComponent<LineRenderer>();
            drawLine.material = new Material(Shader.Find("Sprites/Default"));
            drawLine.startColor = new Color(1.0f, 0.0f, 0.0f, 0.3f);
            drawLine.endColor = new Color(1.0f, 0.0f, 0.0f, 0.3f);
            drawLine.startWidth = 0.01f;
            drawLine.endWidth = 0.01f;
            drawLine.useWorldSpace = false;
            drawLine.positionCount = 0;
            visualLines.Add(line);
        }

        //instantiate all record replay balls
        for (int i = 0; i < maxVisualPointNumber; i++)
        {
            var point = Instantiate(original: prefabBall, parent: tableAnchor.transform);
            point.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.6f, 0.8f, 1.0f, 0.5f));
            point.SetActive(false);
            visualRecordPoints.Add(point);
        }

        //instantiate all record replay lines
        for (int k = 0; k < maxVisualLineNumber; k++)
        {
            var line = new GameObject("ReplayLine_"+k);
            line.transform.SetParent(tableAnchor.transform);
            var drawLine = line.AddComponent<LineRenderer>();
            drawLine.material = new Material(Shader.Find("Sprites/Default"));
            drawLine.startColor = new Color(0.0f, 0.0f, 1.0f, 0.3f);
            drawLine.endColor = new Color(0.0f, 0.0f, 1.0f, 0.3f);
            drawLine.startWidth = 0.01f;
            drawLine.endWidth = 0.01f;
            drawLine.useWorldSpace = false;
            drawLine.positionCount = 0;
            visualRecordLines.Add(line);
        }

        // Trajectory can be simulated using voice command say "Menu"
    }

    // Update is called once per frame
    void Update()
    {
        if (table.activeInHierarchy && !testStop)
        {
            DisplayTableCorners();
            testStop = true;
        }
    }
}
