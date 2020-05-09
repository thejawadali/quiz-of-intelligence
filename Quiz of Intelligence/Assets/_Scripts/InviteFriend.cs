using System;
using UnityEngine;
using UnityEngine.UI;

public class InviteFriend : MonoBehaviour
{
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            ChallengeFriend.instance.InviteFriend(gameObject.name);
        });
    }
}