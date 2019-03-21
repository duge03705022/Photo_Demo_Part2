using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class BlockController : MonoBehaviour
{
    //public TouchHandler touchHandler;
    public RFIBManager rFIBManager;

    public int blockSeriesRow;
    public int blockSeriesCol;
    public float blockSize;
    public int touchedColBlock;
    public int touchedRowBlock;

    public float threshold;
    public string touchStr = "";
    public string touchStr2 = "";
    public string tmpTest = "";
    private string lastTouchStr = "";
    private string lastTouchStr2 = "";

    private float[] rowData;
    private float[] colData;
    private Vector2 tmpRowPos;
    private Vector2 tmpColPos;


    // Start is called before the first frame update
    void Start()
    {
        rowData = new float[blockSeriesRow];
        colData = new float[blockSeriesCol];
    }

    // Update is called once per frame
    void Update()
    {
        Stack<string> touchShow = Arduino.touchShowID;
        Stack<string> touchShow2 = Arduino2.touchShowID;
        while (touchShow.Count > 0)
        {
            touchStr = touchShow.Pop();
        }
        while (touchShow2.Count > 0)
        {
            touchStr2 = touchShow2.Pop();
        }

        int touchedRow = 0, touchedCol = 0;
        float touchedRowData = 0, touchedColData = 0;

        string[] newData = touchStr.Split(',');
        if (touchStr != lastTouchStr)
        {
            newData = touchStr.Split(',');
            if (newData[0] == "R")
            {
                try
                {
                    for (int j = 0; j < blockSeriesRow; j++)
                    {
                        int i = blockSeriesRow -1 - j;
                        rowData[i] = float.Parse(newData[j + 1]);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Invalid Row data -- " + newData);

                }
            }
            else if (newData[0] == "C")
            {
                try
                {
                    for (int i = 0; i < blockSeriesCol; i++)
                    {
                        colData[i] = float.Parse(newData[i + 1]);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Invalid Col data -- " + newData);

                }
            }
            else if (newData[0] == "P")
            {
                touchedCol = int.Parse(newData[1]);
                touchedRow = int.Parse(newData[2]);
                touchedRowData = float.Parse(newData[3]);
                touchedColData = touchedRowData;
            }
            else
            {
                Debug.Log("Invalid Touch String" + newData);
            }
        }

        //TODO Test code
        newData = tmpTest.Split(',');
        if (newData[0] == "P")
        {
            touchedCol = int.Parse(newData[1]);
            touchedRow = int.Parse(newData[2]);
            touchedRowData = float.Parse(newData[3]);
            touchedColData = touchedRowData;
        }

        if (touchStr2 != lastTouchStr2)
        {
            newData = touchStr2.Split(',');
            if (newData[0] == "R")
            {
                try
                {
                    for (int j = 0; j < blockSeriesRow; j++)
                    {
                        int i = blockSeriesRow - 1 - j;
                        rowData[i] = float.Parse(newData[j + 1]);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Invalid Row data -- " + newData);
                }
            }
            else if (newData[0] == "C")
            {
                try
                {
                    for (int i = 0; i < blockSeriesCol; i++)
                    {
                        colData[i] = float.Parse(newData[i + 1]);
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Invalid Col data -- " + newData);

                }
            }
            else if (newData[0] == "P")
            {
                touchedCol = int.Parse(newData[1]);
                touchedRow = int.Parse(newData[2]);
                touchedRowData = float.Parse(newData[3]);
                touchedColData = touchedRowData;
                touchedRowBlock = touchedRow;
            }
            else
            {
                Debug.Log("Invalid Touch String" + newData);
            }
        }

        
        for (int i = 0; i < blockSeriesRow; i++)
        {
            if(rowData[i]>touchedRowData){
                touchedRow = i;
                touchedRowData = rowData[i];
            }
        }
        for (int i = 0; i < blockSeriesCol; i++)
        {
            if (colData[i] > touchedColData)
            {
                touchedCol = i;
                touchedColData = colData[i];
            }
        }
        

        for (int i = 0; i < blockSeriesRow; i++)
        {
            for (int j = 0; j < blockSeriesCol; j++)
            {
                if (i == touchedCol && j == touchedRow && touchedRowData > threshold && touchedColData > threshold) continue;
            }
        }

        if ((touchedRowData > threshold && touchedColData > threshold) && touchedCol!=-1)
        {
            touchedColBlock = touchedCol;
            touchedRowBlock = touchedRow;

            rFIBManager.touchBlock[touchedColBlock, touchedRowBlock] = true;
        }
        else
        {
            touchedColBlock = -1;
            touchedRowBlock = -1;

            for (int i = 0; i < RFIBParameter.stageCol; i++)
            {
                for (int j = 0; j < RFIBParameter.stageRow; j++)
                {
                    rFIBManager.touchBlock[i, j] = false;
                }
            }
        }

        lastTouchStr = touchStr;
    }
}
