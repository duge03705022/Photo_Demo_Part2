using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardHandler : MonoBehaviour
{
    public RFIBManager rFIBManager;

    #region Card Parameter
    public GameObject[] file;

    public GameObject[,] cardInstance;
    private string[,] lastBlockId;

    # endregion

    // Start is called before the first frame update
    void Start()
    {
        cardInstance = new GameObject[RFIBParameter.stageCol, RFIBParameter.stageRow];
        lastBlockId = new string[RFIBParameter.stageCol, RFIBParameter.stageRow];

        for (int i = 0; i < RFIBParameter.stageCol; i++)
        {
            for (int j = 0; j < RFIBParameter.stageRow; j++)
            {
                lastBlockId[i, j] = "0000";
                Debug.Log("AAA");
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
                if (lastBlockId[i, j] != rFIBManager.blockId[i, j, 0])
                {
                    if (rFIBManager.blockId[i, j, 0] != "0000")
                    {
                        PlaceCard(i, j);
                    }
                    else
                    {
                        RemoveCard(i, j);
                    }

                    lastBlockId[i, j] = rFIBManager.blockId[i, j, 0];
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
    }

    private void RemoveCard(int x, int y)
    {
        cardInstance[x, y].SetActive(false);
        cardInstance[x, y] = null;
    }

    public bool IfFile(int x, int y)
    {
        if (cardInstance[x, y] != null)
            return true;
        else
            return false;
    }
}
