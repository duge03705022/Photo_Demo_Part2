using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CardHandler : MonoBehaviour
{
    public RFIBManager rFIBManager;
    public TouchHandler touchHandler;

    #region Card Parameter
    public GameObject[] file;

    public GameObject[,] cardInstance;
    private string[,,] lastBlockId;

    # endregion

    // Start is called before the first frame update
    void Start()
    {
        cardInstance = new GameObject[RFIBParameter.stageCol, RFIBParameter.stageRow];
        lastBlockId = new string[RFIBParameter.stageCol, RFIBParameter.stageRow, RFIBParameter.maxHight];

        for (int i = 0; i < RFIBParameter.stageCol; i++)
        {
            for (int j = 0; j < RFIBParameter.stageRow; j++)
            {
                for (int k = 0; k < RFIBParameter.maxHight; k++)
                {
                    lastBlockId[i, j, k] = "0000";
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        updateCards();
    }

    private void updateCards()
    {
        for (int i = 0; i < RFIBParameter.stageCol; i++)
        {
            for (int j = 0; j < RFIBParameter.stageRow; j++)
            {
                if (lastBlockId[i, j, 0] != rFIBManager.blockId[i, j, 0])
                {
                    if (rFIBManager.blockId[i, j, 0] != "0000")
                    {
                        PlaceCard(i, j);
                    }
                    else
                    {
                        RemoveCard(i, j);
                    }

                    lastBlockId[i, j, 0] = rFIBManager.blockId[i, j, 0];
                }
                if (rFIBManager.blockId[i, j, 0] != "0000")
                {
                    for (int k = 1; k < RFIBParameter.maxHight; k++)
                    {
                        if (rFIBManager.blockId[i, j, k] != "0000")
                        {
                            AddEffect(i, j, k);
                        }
                        else
                        {
                            HideEffect(i, j, k);
                        }
                    }
                }
            }
        }
    }

    private void PlaceCard(int x, int y)
    {
        cardInstance[x, y] = file[RFIBParameter.SearchCard(rFIBManager.blockId[x, y, 0])];
        cardInstance[x, y].SetActive(true);
        cardInstance[x, y].transform.localPosition = new Vector3(
            x * GameParameter.stageGap,
            y * GameParameter.stageGap,
            0);
        cardInstance[x, y].GetComponent<FileController>().ResetZoomIn();
        cardInstance[x, y].GetComponent<FileController>().HideAllPhoto();
        cardInstance[x, y].GetComponent<FileController>().ResetSelectPhoto();
        touchHandler.ResetIfHold();
        touchHandler.ResetIfZoom();
    }

    private void RemoveCard(int x, int y)
    {
        cardInstance[x, y].SetActive(false);
        cardInstance[x, y] = null;
    }

    private void AddEffect(int x, int y, int z)
    {
        switch (RFIBParameter.SearchCard(rFIBManager.blockId[x, y, z]))
        {
            case 10:    // red mask
                cardInstance[x, y].GetComponent<FileController>().SetTopMask("r");
                break;
            case 11:    // green mask
                cardInstance[x, y].GetComponent<FileController>().SetTopMask("g");
                break;
            case 12:    // blue mask
                cardInstance[x, y].GetComponent<FileController>().SetTopMask("b");
                break;
            default:
                break;
        }
    }

    private void HideEffect(int x, int y, int z)
    {
        cardInstance[x, y].GetComponent<FileController>().HideTopMask();
    }

    public bool IfFile(int x, int y)
    {
        if (cardInstance[x, y] != null)
            return true;
        else
            return false;
    }
}
