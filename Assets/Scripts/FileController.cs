using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileController : MonoBehaviour
{
    # region File Parameters 
    public string fileName;
    public GameObject photoParent;
    public GameObject[] photosInFile;
    private int photoCount;
    private int showPage;
    public GameObject topMask;
    //public GameObject photoMask;
    //private GameObject photoMaskInstance;

    //
    public GameObject[] photos;

    # endregion

    // Start is called before the first frame update
    void Start()
    {
        photosInFile = new GameObject[GameParameter.maxPhotoInFile];
        photoCount = 0;
        showPage = 0;

        for (int i = 0; i < photos.Length; i++)
        {
            AddPhoto(photos[i]);
        }

        HideAllPhoto();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddPhoto(GameObject photo)
    {
        photosInFile[photoCount] = Instantiate(photo, photoParent.transform);
        photosInFile[photoCount].transform.localPosition = new Vector3(
            (float)((photoCount / 3 - 1) * 1.3),
            (float)((1 - photoCount % 3) * 1.3),
            0);
        photosInFile[photoCount].transform.localScale = new Vector3(
            0.24f,
            0.24f,
            0.24f);
        photosInFile[photoCount].GetComponent<SpriteRenderer>().sortingOrder = 21;

        photoCount++;
        if (photoCount > 9)
        {
            photosInFile[photoCount - 1].SetActive(false);
        }
    }

    public IEnumerator SwipePage(bool ifHold, string direction)
    {
        if (ifHold)
        {
            ShowAllPhoto();
            int hidePage = showPage;
            switch (direction)
            {
                case "Right":
                    if (hidePage < (photoCount - 1) / 9)
                    {
                        showPage = hidePage + 1;
                    }
                    break;
                case "Left":
                    if (hidePage > 0)
                    {
                        showPage = hidePage - 1;
                    }
                    break;
                default:
                    break;
            }

            if (showPage != hidePage)
            {
                yield return StartCoroutine(MovePhoto(direction));
            }
            else
            {
                yield return StartCoroutine(LittleMovePhoto(direction));
            }
        }

        HideOtherPhoto();
    }

    public void ShowAllPhoto()
    {
        ResetSelectPhoto();
        //Destroy(photoMaskInstance);
        //photoMaskInstance = Instantiate(photoMask, transform.parent);
        for (int i = 0; i < photoCount; i++)
        {
            photosInFile[i].SetActive(true);
        }
    }

    public void HideAllPhoto()
    {
        ResetSelectPhoto();
        //Destroy(photoMaskInstance);
        for (int i = 0; i < photoCount; i++)
        {
            photosInFile[i].SetActive(false);
        }
    }

    public void HideOtherPhoto()
    {
        //Destroy(photoMaskInstance);
        for (int i = 0; i < photoCount; i++)
        {
            if (i / 9 != showPage)
            {
                photosInFile[i].SetActive(false);
            }
            else if (i / 9 == showPage)
            {
                photosInFile[i].SetActive(true);
            }
        }
    }

    private IEnumerator MovePhoto(string direction)
    {
        for (int i = 0; i < 10; i++)
        {
            switch (direction)
            {
                case "Right":
                    photoParent.transform.localPosition -= new Vector3(3.9f / 10, 0, 0);
                    break;
                case "Left":
                    photoParent.transform.localPosition += new Vector3(3.9f / 10, 0, 0);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator LittleMovePhoto(string direction)
    {
        for (int i = 0; i < 2; i++)
        {
            switch (direction)
            {
                case "Right":
                    photoParent.transform.localPosition += new Vector3(-3.9f / 10, 0, 0);
                    break;
                case "Left":
                    photoParent.transform.localPosition += new Vector3(3.9f / 10, 0, 0);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        for (int i = 0; i < 2; i++)
        {
            switch (direction)
            {
                case "Right":
                    photoParent.transform.localPosition += new Vector3(3.9f / 10, 0, 0);
                    break;
                case "Left":
                    photoParent.transform.localPosition += new Vector3(-3.9f / 10, 0, 0);
                    break;
                default:
                    break;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void SelectPhoto(int x, int y)
    {
        if (x * 3 - y + 2 + showPage * 9 < photoCount)
        {
            photosInFile[x * 3 - y + 2 + showPage * 9].GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f);
        }
    }

    public IEnumerator ZoomInPhoto(int x, int y)
    {
        ResetSelectPhoto();
        HideAllPhoto();
        photosInFile[x * 3 - y + 2 + showPage * 9].SetActive(true);
        Vector3 oldPos = photosInFile[x * 3 - y + 2 + showPage * 9].transform.localPosition;
        for (int i = 0; i < 5; i++)
        {
            photosInFile[x * 3 - y + 2 + showPage * 9].transform.localScale += new Vector3((0.82f - 0.24f) / 5, (0.8f - 0.24f) / 5, (0.8f - 0.24f) / 5);
            photosInFile[x * 3 - y + 2 + showPage * 9].transform.localPosition += new Vector3((0f - oldPos.x) / 5, (0f - oldPos.y) / 5, (0f - oldPos.z) / 5);

            yield return new WaitForSeconds(0.1f);
        }
    }

    public void ResetZoomIn()
    {
        for (int i = 0; i < photoCount; i++)
        {
            photosInFile[i].transform.localPosition = new Vector3(
            (float)((i / 3 - 1) * 1.3),
            (float)((1 - i % 3) * 1.3),
            0);
            photosInFile[i].transform.localScale = new Vector3(
                0.24f,
                0.24f,
                0.24f);
            photosInFile[i].GetComponent<SpriteRenderer>().sortingOrder = 21;
        }

        HideOtherPhoto();
    }

    public void ResetSelectPhoto()
    {
        for (int i = 0; i < photoCount; i++)
        {
            photosInFile[i].GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f);
        }
    }

    public void SetTopMask(string color)
    {
        topMask.SetActive(true);
        switch (color)
        {
            case "r":
                topMask.GetComponent<SpriteRenderer>().color = new Color(1f, 0f, 0f, 0.5f);
                break;
            case "g":
                topMask.GetComponent<SpriteRenderer>().color = new Color(0f, 1f, 0f, 0.5f);
                break;
            case "b":
                topMask.GetComponent<SpriteRenderer>().color = new Color(0f, 0f, 1f, 0.5f);
                break;
            default:
                break;
        }
    }

    public void HideTopMask()
    {
        topMask.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        topMask.SetActive(false);
    }
}
