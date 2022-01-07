using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class scoreBoardHandler : MonoBehaviour
{
    public TextMeshPro scoreLeft;
    public TextMeshPro scoreRight;
    public TextMeshPro lineLeft;
    public TextMeshPro lineRight;
    const string left = "LEFT";
    const string right = "RIGHT";
    public int leftAlpha = 0;
    public int rightAlpha = 0;
    public Color burgund = new Color(0.61f,0.1f,0.1f);
    public LoggerScript logger;
    // Start is called before the first frame update
    void Start()
    {
        resetScores();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void resetScores()
    {
        scoreLeft.text = "0";
        scoreRight.text = "0";
    }

    public void setPlayerServe(string side,int serve,bool ready)
    {
        Color toUse = Color.white;
        if (ready) {toUse = burgund;}
        if (side == left){scoreLeft.color = toUse;}
        else { scoreRight.color = toUse; }
        toUse.a = serve;
        if (side == left){lineLeft.color = toUse;}
        else { lineRight.color = toUse; }
    }

    public void readyToServe(string side)
    {
        logger.log("Side " + side + " is ready to serve!");
        setPlayerServe(side, 1, true);
        if (side == left) { side = right; } else { side = left; }
        setPlayerServe(side, 0, false);
    }

    public void updateScoreBoard(UDPListener.EventData mrData)
    {
        logger.log("Updating ScoreBoard");
        switch (mrData.eventType)
        {
            case ("onReadyToServe"):
                logger.log("Case onReadyToServe");
                string side = mrData.side;
                readyToServe(side);
                break;
            case ("onScore"):
                logger.log("Case onScore");
                handleScore(mrData.side, mrData.score, mrData.nextServer, mrData.lastServer);
                break;
            default:
                break;
        }
    }

    private void handleScore(string side, int score, string nextServer, string lastServer)
    {
        logger.log("Hadling Score for side " + side + " with score " + score.ToString());
        if (side == left) { scoreLeft.text = score.ToString(); } else { scoreRight.text = score.ToString(); }
        if(lastServer!=null) setPlayerServe(lastServer, 0, false);
        if (nextServer!= null) setPlayerServe(nextServer, 1, false);
    }

    public void flipScores()
    {
        var pos = scoreLeft.transform.localPosition;
        scoreLeft.transform.localPosition = scoreRight.transform.localPosition;
        scoreRight.transform.localPosition = pos;

        var linePos = lineLeft.transform.localPosition;
        lineLeft.transform.localPosition = lineRight.transform.localPosition;
        lineRight.transform.localPosition = linePos;
    }
}
