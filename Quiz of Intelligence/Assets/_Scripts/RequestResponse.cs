using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequestResponse : MonoBehaviour
{
    public void AcceptRequest()
    {
        ChallengeFriend.instance.AcceptRequest();
    }

    public void RejectRequest()
    {
        ChallengeFriend.instance.RejectRequest();
    }

    public void CancelRequest()
    {
        ChallengeFriend.instance.CancelRequest();
    }
}