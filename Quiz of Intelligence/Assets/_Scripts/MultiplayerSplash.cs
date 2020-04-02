using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerSplash : MonoBehaviour
{
    public GameObject splash;
    public Image line;
    public RectTransform vsImage;
    public RectTransform ticket_mine;
    public RectTransform ticket_other;
    public float time;

    private void Start()
    {
        splash.SetActive(true);
        line.DOFade(1, time);
        vsImage.DOScale(new Vector2(1, 1), time).OnComplete(() =>
        {
            ticket_mine.DOAnchorPosX(-176, time);
            ticket_other.DOAnchorPosX(176, time).OnComplete(() => { StartCoroutine(StartQuiz()); });
        });
    }

    IEnumerator StartQuiz()
    {
        yield return new WaitForSeconds(time);
        splash.SetActive(false);
        GameSceneAnimations.instance.AllComponentsAnimations_IN(0.2f);
    }
}