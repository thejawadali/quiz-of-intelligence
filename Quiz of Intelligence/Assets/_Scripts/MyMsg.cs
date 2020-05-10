using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class MyMsg : MonoBehaviour
{
    public TextMeshProUGUI msgText;
    public static MyMsg instance = null;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        transform.GetChild(0).gameObject.SetActive(false);
    }


    public void Message(string messageText, float duration)
    {
        transform.GetChild(0).gameObject.SetActive(true);
        msgText.text = messageText;
        GetComponent<RectTransform>().DOAnchorPosY(0, 0.2f);
        StartCoroutine(HideMessage(duration));
    }

    IEnumerator HideMessage(float secs)
    {
        yield return new WaitForSeconds(secs);
        GetComponent<RectTransform>().DOAnchorPosY(80, 0.2f);
        msgText.text = "";
        transform.GetChild(0).gameObject.SetActive(false);
    }
}