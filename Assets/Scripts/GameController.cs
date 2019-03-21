using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameController : MonoBehaviour
{
    public TouchHandler touchHandler;
    public CardHandler cardHandler;

    public GameObject[] photoSeries;
    public GameObject parentTransform;

    public GameObject[,] photoInstances;

    public GameObject copyParent;
    private GameObject[] copyInstances;

    // Start is called before the first frame update
    void Start()
    {
        photoInstances = new GameObject[RFIBParameter.stageCol, RFIBParameter.stageRow];

        for (int i = 0; i < RFIBParameter.stageCol; i++)
        {
            for (int j = 0; j < RFIBParameter.stageRow; j++)
            {
                photoInstances[i, j] = Instantiate(photoSeries[i * RFIBParameter.stageRow + j], parentTransform.transform);
                photoInstances[i, j].transform.localPosition = new Vector3(
                    i * GameParameter.stageGap,
                    j * GameParameter.stageGap,
                    0);
            }
        }

        copyInstances = new GameObject[RFIBParameter.maxTouch];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator CopyPhoto(int selectCount, Tuple<int, int>[] selectPhoto, Tuple<int, int> targetFile)
    {
        for (int i = 0; i < selectCount; i++)
        {
            copyInstances[i] = Instantiate(photoSeries[selectPhoto[i].Item1 * RFIBParameter.stageRow + selectPhoto[i].Item2], copyParent.transform);
            copyInstances[i].transform.localPosition = new Vector3(
                    selectPhoto[i].Item1 * GameParameter.stageGap,
                    selectPhoto[i].Item2 * GameParameter.stageGap,
                    0);
            copyInstances[i].GetComponent<SpriteRenderer>().sortingOrder = 40;
            StartCoroutine(MovePhoto(i, selectPhoto[i].Item1, selectPhoto[i].Item2, targetFile.Item1, targetFile.Item2));
            yield return new WaitForSeconds(0.5f);
        }

        StartCoroutine(touchHandler.ResetSelect(GameParameter.moveTime + 0.2f));
    }

    IEnumerator MovePhoto(int instanceId, int fromX, int fromY, int toX, int toY)
    {
        for (int i = 0; i < GameParameter.moveStep; i++)
        {
            copyInstances[instanceId].transform.localPosition += new Vector3(
                (toX - fromX) * GameParameter.stageGap / GameParameter.moveStep,
                (toY - fromY) * GameParameter.stageGap / GameParameter.moveStep,
                0f);
            copyInstances[instanceId].transform.localScale -= new Vector3(
                0.7f / GameParameter.moveStep,
                0.7f / GameParameter.moveStep,
                0.7f / GameParameter.moveStep);
            yield return new WaitForSeconds(GameParameter.moveTime / GameParameter.moveStep);
        }

        cardHandler.cardInstance[toX, toY].GetComponent<FileController>().AddPhoto(photoSeries[fromX * RFIBParameter.stageRow + fromY]);
        Destroy(copyInstances[instanceId]);
    }
}
