using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TouchHandler : MonoBehaviour
{
    public RFIBManager rFIBManager;
    public CardHandler cardHandler;
    public GameController gameController;
    //public FileController fileController;

    # region Touch Parameters
    private bool ifTouch;
    private int touchTime;
    private int notTouchTime;

    private Tuple<int, int> nowTouch;
    private Tuple<int, int> lastTouch;

    private string touchAction;

    private int clickCount;

    private Tuple<int, int>[] touchHistory;
    private int touchHistoryCount;
    private string swipeDirection;

    public GameObject selectParent;
    public GameObject[] selectPrefab;
    public GameObject[] selectInstances;
    private int selectCount;

    private Tuple<int, int>[] selectPos;
    private Tuple<int, int> targetPos;

    public Tuple<bool, int, int> ifHold;
    public Tuple<bool, int, int> ifZoom;

    # endregion

    // Start is called before the first frame update
    void Start()
    {
        ifTouch = false;
        touchTime = 0;
        notTouchTime = 0;

        nowTouch = Tuple.Create(-1, -1);
        lastTouch = Tuple.Create(-1, -1);

        touchAction = "Idle";

        clickCount = 0;

        touchHistory = new Tuple<int, int>[RFIBParameter.maxTouch];
        touchHistoryCount = 0;
        swipeDirection = "None";

        selectCount = 0;
        selectPos = new Tuple<int, int>[RFIBParameter.maxTouch];
        selectInstances = new GameObject[RFIBParameter.maxTouch];
        targetPos = Tuple.Create(-1, -1);

        ResetIfHold();
        ResetIfZoom();
    }

    // Update is called once per frame
    void Update()
    {
        SenseTouch();
        DoAction();
        KeyPressed();

        // Debug msg
        bool ifDebug = false;
        if (ifDebug)
        {
            Debug.Log(string.Format("touchTime:{0} notTouchTime:{1} - {2}({4}) - clickCount:{3}", touchTime, notTouchTime, touchAction, clickCount, swipeDirection));

            string str = "Position history: ";
            for (int i = 0; i < touchHistoryCount; i++)
            {
                str += string.Format("({0}, {1}) ", touchHistory[i].Item1, touchHistory[i].Item2);
            }
            if (touchHistoryCount != 0)
            {
                Debug.Log(str);
            }
        }
    }

    private void SenseTouch()
    {
        ifTouch = false;
        nowTouch = Tuple.Create(-1, -1);
        // Find touching position (*Guarantee one touch per frame)
        for (int i = 0; i < RFIBParameter.touchCol; i++)
        {
            for (int j = 0; j < RFIBParameter.touchRow; j++)
            {
                if (rFIBManager.touchBlock[i, j])
                {
                    ifTouch = true;
                    nowTouch = Tuple.Create(i, j);
                }
            }
        }

        // Touch calculating
        if (ifTouch)
        {
            if (touchTime == 0 || !nowTouch.Equals(lastTouch))
            {
                if (!nowTouch.Equals(lastTouch))
                {
                    clickCount = 0;
                }

                clickCount++;

                touchAction = "ClickAgain";
                touchHistory[touchHistoryCount] = Tuple.Create(nowTouch.Item1, nowTouch.Item2);
                touchHistoryCount++;
            }

            touchTime++;
            notTouchTime = 0;
            lastTouch = Tuple.Create(nowTouch.Item1, nowTouch.Item2);
        }
        else
        {
            touchTime = 0;
            notTouchTime++;
        }

        // Identify the touch action
        IdentifyAction();
    }

    private void ResetTouch()
    {
        touchHistory = new Tuple<int, int>[RFIBParameter.maxTouch];
        clickCount = 0;
        touchHistoryCount = 0;
        swipeDirection = "None";
        lastTouch = Tuple.Create(-1, -1);
    }

    private void DoAction()
    {
        switch (touchAction)
        {
            case "Click":
                Click(touchHistory[touchHistoryCount - 1].Item1, touchHistory[touchHistoryCount - 1].Item2);
                break;
            case "DoubleClick":
                DoubleClick(touchHistory[touchHistoryCount - 1].Item1, touchHistory[touchHistoryCount - 1].Item2);
                break;
            case "Hold":
                Hold(touchHistory[touchHistoryCount - 1].Item1, touchHistory[touchHistoryCount - 1].Item2);
                break;
            case "Swipe":
                Swipe(swipeDirection, touchHistory[touchHistoryCount - 3].Item1, touchHistory[touchHistoryCount - 3].Item2);
                break;
            case "Idle":
                Idle();
                break;
        }
    }

    private void Click(int x, int y)
    {
        if (ifZoom.Item1)
        {
            cardHandler.cardInstance[ifZoom.Item2 / 3, ifZoom.Item3 / 3].GetComponent<FileController>().ResetZoomIn();
            ResetIfZoom();
        }
        else if (ifHold.Item1)
        {
            if (!cardHandler.IfFile(x / 3, y / 3))
            {
                cardHandler.cardInstance[ifHold.Item2, ifHold.Item3].GetComponent<FileController>().HideAllPhoto();
                ResetIfHold();
            }
            else
            {
                SelectPhotoInFile(x, y);
            }
        }
        else if (!cardHandler.IfFile(x / 3, y / 3) || selectCount > 0)
        {
            Select(x / 3, y / 3);
        }
        else
        {
            SelectPhotoInFile(x, y);
        }

        touchAction = "ClickDone";
    }

    private void DoubleClick(int x, int y)
    {
        Debug.Log(string.Format("DoubleClick ({0}, {1})", x / 3, y / 3));

        if (ifHold.Item1)
        {
            if (cardHandler.IfFile(x / 3, y / 3))
            {
                ZoomInPhoto(x, y);
            }
        }
        else
        {
            SetTarget(x / 3, y / 3);

            if (cardHandler.IfFile(x / 3, y / 3) && selectCount > 0)
            {
                StartCoroutine(gameController.CopyPhoto(selectCount, selectPos, targetPos));
            }
            else
            {
                StartCoroutine(ResetSelect(0.5f));
            }
        }

        touchAction = "DoubleClickDone";
    }

    private void Hold(int x, int y)
    {
        Debug.Log(string.Format("Hold ({0}, {1})", x / 3, y / 3));

        if (cardHandler.IfFile(x / 3, y / 3))
        {
            cardHandler.cardInstance[x / 3, y / 3].GetComponent<FileController>().HideOtherPhoto();
            ifHold = Tuple.Create(true, x / 3, y / 3);
        }

        touchAction = "HoldDone";
    }

    private void Swipe(string direction, int x, int y)
    {
        Debug.Log(string.Format("Swipe {0} ({1}, {2})", direction, x, y));
        StartCoroutine(ResetSelect(0.1f));

        if (cardHandler.IfFile(x / 3, y / 3))
        {
            Debug.Log(ifHold);
            StartCoroutine(cardHandler.cardInstance[x / 3, y / 3].GetComponent<FileController>().SwipePage(ifHold.Item1, direction));
        }

        touchAction = "SwipeDone";
    }

    private void Idle()
    {
        touchAction = "IdleDone";
    }

    private void Select(int x, int y)
    {
        Debug.Log(string.Format("Select ({0}, {1})", x, y));

        selectInstances[selectCount] = Instantiate(selectPrefab[0], selectParent.transform);
        selectInstances[selectCount].transform.localPosition = new Vector3(
            x * GameParameter.stageGap,
            y * GameParameter.stageGap,
            0);

        selectPos[selectCount] = Tuple.Create(x, y);
        selectCount++;
    }

    private void SelectPhotoInFile(int x, int y)
    {
        if (ifHold.Item1)
        {
            Debug.Log(string.Format("SelectPhoto ({0}, {1}) [{2}, {3}]", x / 3, y / 3, x % 3, y % 3));
            cardHandler.cardInstance[x / 3, y / 3].GetComponent<FileController>().SelectPhoto(x % 3, y % 3);
        }
    }

    private void ZoomInPhoto(int x, int y)
    {
        Debug.Log(string.Format("SelectPhoto ({0}, {1}) [{2}, {3}]", x / 3, y / 3, x % 3, y % 3));
        StartCoroutine(cardHandler.cardInstance[x / 3, y / 3].GetComponent<FileController>().ZoomInPhoto(x % 3, y % 3));
        ifZoom = Tuple.Create(true, x, y);
    }

    private void SetTarget(int x, int y)
    {
        if (selectCount > 0)
        {
            selectCount--;
            Destroy(selectInstances[selectCount]);
        }
        selectInstances[selectCount] = Instantiate(selectPrefab[1], selectParent.transform);
        selectInstances[selectCount].transform.localPosition = new Vector3(
            x * GameParameter.stageGap,
            y * GameParameter.stageGap,
            0);

        targetPos = Tuple.Create(x, y);
    }

    private void IdentifyAction()
    {
        //// Click
        //if (clickCount == 1 && touchAction != "ClickDone")
        //{
        //    touchAction = "ClickDown";
        //}
        //if (clickCount == 1 && touchAction == "ClickDown" && touchTime < 30)
        //{
        //    touchAction = "Click";
        //}
        //// DoubleClick
        //if (clickCount == 2 && touchAction != "DoubleClickDone")
        //{
        //    touchAction = "DoubleClick";
        //}
        //// Hold
        //if (touchTime >= 30 && touchAction != "HoldDone" && touchAction != "SwipeDone")
        //{
        //    touchAction = "Hold";
        //    clickCount = 0;
        //}
        //// Swipe
        //if (touchHistoryCount >= 3 && touchAction != "SwipeDone")
        //{
        //    if (touchHistory[2].Item1 > touchHistory[1].Item1 &&
        //        touchHistory[1].Item1 > touchHistory[0].Item1 &&
        //        touchHistory[2].Item2 == touchHistory[1].Item2 &&
        //        touchHistory[1].Item2 == touchHistory[0].Item2)
        //    {
        //        touchAction = "Swipe";
        //        swipeDirection = "Left";
        //        clickCount = 0;
        //    }
        //    else if (touchHistory[2].Item1 == touchHistory[1].Item1 &&
        //        touchHistory[1].Item1 == touchHistory[0].Item1 &&
        //        touchHistory[2].Item2 > touchHistory[1].Item2 &&
        //        touchHistory[1].Item2 > touchHistory[0].Item2)
        //    {
        //        touchAction = "Swipe";
        //        swipeDirection = "Up";
        //        clickCount = 0;
        //    }
        //    else if (touchHistory[2].Item1 < touchHistory[1].Item1 &&
        //        touchHistory[1].Item1 < touchHistory[0].Item1 &&
        //        touchHistory[2].Item2 == touchHistory[1].Item2 &&
        //        touchHistory[1].Item2 == touchHistory[0].Item2)
        //    {
        //        touchAction = "Swipe";
        //        swipeDirection = "Right";
        //        clickCount = 0;
        //    }
        //    else if (touchHistory[2].Item1 == touchHistory[1].Item1 &&
        //        touchHistory[1].Item1 == touchHistory[0].Item1 &&
        //        touchHistory[2].Item2 < touchHistory[1].Item2 &&
        //        touchHistory[1].Item2 < touchHistory[0].Item2)
        //    {
        //        touchAction = "Swipe";
        //        swipeDirection = "Down";
        //        clickCount = 0;
        //    }
        //}
        //// Idle
        //if (notTouchTime >= 40 && touchAction != "IdleDone")
        //{
        //    touchAction = "Idle";
        //    ResetTouch();
        //}

        // Click
        if (clickCount == 1 && touchAction != "ClickDone")
        {
            touchAction = "Click";
        }
        // DoubleClick
        if (clickCount == 2 && touchAction != "DoubleClickDone")
        {
            touchAction = "DoubleClick";
        }
        // Hold
        if (touchTime >= 30 && touchAction != "HoldDone" && touchAction != "SwipeDone")
        {
            touchAction = "Hold";
            clickCount = 0;
        }
        // Swipe
        if (touchHistoryCount >= 3 && touchAction != "SwipeDone")
        {
            if (touchHistory[2].Item1 > touchHistory[1].Item1 &&
                touchHistory[1].Item1 > touchHistory[0].Item1 &&
                touchHistory[2].Item2 == touchHistory[1].Item2 &&
                touchHistory[1].Item2 == touchHistory[0].Item2)
            {
                touchAction = "Swipe";
                swipeDirection = "Left";
                clickCount = 0;
            }
            else if (touchHistory[2].Item1 == touchHistory[1].Item1 &&
                touchHistory[1].Item1 == touchHistory[0].Item1 &&
                touchHistory[2].Item2 > touchHistory[1].Item2 &&
                touchHistory[1].Item2 > touchHistory[0].Item2)
            {
                touchAction = "Swipe";
                swipeDirection = "Up";
                clickCount = 0;
            }
            else if (touchHistory[2].Item1 < touchHistory[1].Item1 &&
                touchHistory[1].Item1 < touchHistory[0].Item1 &&
                touchHistory[2].Item2 == touchHistory[1].Item2 &&
                touchHistory[1].Item2 == touchHistory[0].Item2)
            {
                touchAction = "Swipe";
                swipeDirection = "Right";
                clickCount = 0;
            }
            else if (touchHistory[2].Item1 == touchHistory[1].Item1 &&
                touchHistory[1].Item1 == touchHistory[0].Item1 &&
                touchHistory[2].Item2 < touchHistory[1].Item2 &&
                touchHistory[1].Item2 < touchHistory[0].Item2)
            {
                touchAction = "Swipe";
                swipeDirection = "Down";
                clickCount = 0;
            }
        }
        // Idle
        if (notTouchTime >= 40 && touchAction != "IdleDone")
        {
            touchAction = "Idle";
            ResetTouch();
        }
    }

    public IEnumerator ResetSelect(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < selectCount + 1; i++)
        {
            Destroy(selectInstances[i]);
            selectInstances[i] = null;
        }

        selectPos = new Tuple<int, int>[RFIBParameter.maxTouch];
        targetPos = Tuple.Create(-1, -1);

        selectCount = 0;
    }

    public void ResetIfHold()
    {
        ifHold = Tuple.Create(false, -1, -1);
    }

    public void ResetIfZoom()
    {
        ifZoom = Tuple.Create(false, -1, -1);
    }

    private void KeyPressed()
    {
        if (Input.GetKey("q"))
        {
            rFIBManager.touchBlock[15, 8] = true;
        }
        else
        {
            rFIBManager.touchBlock[15, 8] = false;
        }
        if (Input.GetKey("w"))
        {
            rFIBManager.touchBlock[16, 8] = true;
        }
        else
        {
            rFIBManager.touchBlock[16, 8] = false;
        }
        if (Input.GetKey("e"))
        {
            rFIBManager.touchBlock[17, 8] = true;
        }
        else
        {
            rFIBManager.touchBlock[17, 8] = false;
        }
        if (Input.GetKey("a"))
        {
            rFIBManager.touchBlock[10, 4] = true;
        }
        else
        {
            rFIBManager.touchBlock[10, 4] = false;
        }
        if (Input.GetKey("s"))
        {
            rFIBManager.touchBlock[13, 4] = true;
        }
        else
        {
            rFIBManager.touchBlock[13, 4] = false;
        }
        if (Input.GetKey("d"))
        {
            rFIBManager.touchBlock[16, 4] = true;
        }
        else
        {
            rFIBManager.touchBlock[16, 4] = false;
        }

        if (Input.GetKey("t"))
        {
            rFIBManager.touchBlock[22, 7] = true;
        }
        else
        {
            rFIBManager.touchBlock[22, 7] = false;
        }

        //if (Input.GetKeyUp("z"))
        //{
        //    ResetSelect();
        //}
    }
}
