using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using NICER_Unity_API;

public class RunNICER : MonoBehaviour
{
    private List<string> csvList = new List<string>();

    private int row = 0;

    private int currentFrame = 1;

    public Transform sh;
    public Transform el;
    public Transform wt;
    public Transform hd;

    public NICER_API nicerAPI;

    public double fatigueLevel;

    private float delta;
    private float totalTime;
    private string gender = "Male";

    // Start is called before the first frame update
    void Start()
    {
        ReadCSVFile();

        var data1 = csvList[1].Split(',');

        var data2 = csvList[2].Split(',');

        delta = float.Parse(data2[1]) - float.Parse(data1[1]);
       
    }


    // Update is called once per frame
    void Update()
    {

        if (currentFrame < row)
        {
            string currentRow = csvList[currentFrame];

            var data_values = currentRow.Split(',');
           
            sh.position = new Vector3(float.Parse(data_values[2]), float.Parse(data_values[3]), float.Parse(data_values[4]));

            el.position = new Vector3(float.Parse(data_values[5]), float.Parse(data_values[6]), float.Parse(data_values[7]));

            wt.position = new Vector3(float.Parse(data_values[8]), float.Parse(data_values[9]), float.Parse(data_values[10]));

            hd.position = new Vector3(float.Parse(data_values[11]), float.Parse(data_values[12]), float.Parse(data_values[13]));

            totalTime = float.Parse(data_values[1]);

            fatigueLevel = nicerAPI.generatePrediction(hd, wt, el, sh, gender, delta, totalTime)[1]; // NICER API will return [Endurance Time (second), Fatigue Level (%)]
        }

        currentFrame += 1;
    }

    void ReadCSVFile()
    {
        StreamReader strReader = new StreamReader("Assets\\Sample_Mocap.csv");

        bool endOfFile = false;

        while (!endOfFile)
        {
            string data_String = strReader.ReadLine();

            if (data_String == null)
            {
                endOfFile = true;
                break;
            }

            row += 1;

            csvList.Add(data_String);
        }
    }
}
