using UnityEngine;
using UnityEngine.UI;
public class AnswerToServer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SendAns(string answer)
    {
        Debug.Log("Button pressed with answer: " + answer);
        Client.Instance.Send("answer",answer);
    }
    public void SendStart()
    {
        Client.Instance.Send("start");
    }
}
