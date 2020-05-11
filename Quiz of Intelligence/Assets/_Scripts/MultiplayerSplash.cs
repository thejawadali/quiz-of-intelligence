using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerSplash : MonoBehaviour
{
    public GameObject splash;
    public GameObject friendsListPanel;
    public Image line;
    public RectTransform vsImage;
    public TextMeshProUGUI nameText_mine;
    public TextMeshProUGUI nameText_other;
    public float time;

    public static MultiplayerSplash instance = null;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        if (ChallengeFriend.isComingFromInvitation)
        {
            AnimateSplash();
        }
    }

    public void AnimateSplash()
    {
        friendsListPanel.SetActive(false);
        splash.SetActive(true);
        line.DOFade(1, time);
        vsImage.DOScale(new Vector2(1, 1), time).OnComplete(() =>
        {
            nameText_mine.GetComponent<RectTransform>().DOAnchorPosX(0, time);
            nameText_other.GetComponent<RectTransform>().DOAnchorPosX(0, time).OnComplete(() => { StartCoroutine(StartQuiz()); });
        });
    }

    IEnumerator StartQuiz()
    {
        yield return new WaitForSeconds(time);
        splash.SetActive(false);
        GameSceneAnimations.instance.AllComponentsAnimations_IN(0.2f);
    }
}