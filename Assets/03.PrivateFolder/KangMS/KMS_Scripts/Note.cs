using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
public abstract class Note : MonoBehaviour
{
    public float speed = 5f;
    public float scoreValue = 100;
    public Vector3 endPoint;
    public bool _isHit = false;
    public static bool isBoss = false;
    
    public virtual void Initialize(Vector3 endPoint, float speed, float scoreValue)
    {
        this.endPoint = endPoint;
        this.speed = speed;
        this.scoreValue = scoreValue;
        double startDspTime = AudioSettings.dspTime;
        double travelDuration = Vector3.Distance(transform.position, endPoint) / speed;
        double endDspTime = startDspTime + travelDuration;
        StartCoroutine(MoveToLeft(startDspTime, endDspTime));
    }
    /// <summary>
    /// ?์๊ณ??์??_endPoint๋ฅ??ฅํ??? ์๊ฐ?๋ก ?ค์ .
    /// </summary>
    protected virtual IEnumerator MoveToLeft(double startDspTime, double endDspTime)
    {
        Vector3 startPosition = transform.position;
        Vector3 direction = (endPoint - startPosition).normalized;
        float totalDistance = Vector3.Distance(startPosition, endPoint);
        Debug.Log($"์ถ๋ฐ ?๊ฐ : {AudioSettings.dspTime} , ?์ฐฉ?์ ?๊ฐ : {(totalDistance / speed) + AudioSettings.dspTime}");

        while (!_isHit)
        {
            double currentDspTime = AudioSettings.dspTime;
            // ?จ์? ?๊ฐ??๋น๋??์ฌ ๋ง??๋ ???ผ์  ๊ฑฐ๋ฆฌ๋งํผ ?ด๋
            double elapsedTime = currentDspTime - startDspTime;
            float coveredDistance = Mathf.Min((float)(elapsedTime * speed), totalDistance);
            transform.position = startPosition + direction * coveredDistance;
            if (Vector3.Distance(transform.position, endPoint) <= 0.001f)
            {
                Debug.Log($"?ธํธ๊ฐ ๋ชฉํ ์ง?์ ?์ฐฉ?? ?์ฐฉ ?๊ฐ: {currentDspTime}");
                //Destroy(gameObject);
                yield break;
            }
            // ?์ค?ธ์ฉ ๋ก๊ทธ ์ถ๋ ฅ
            //Debug.Log($"?ธํธ ?ด๋ ์ค?- ?์ฌ dspTime: {currentDspTime}, ๋ชฉํ ?๊ฐ: {endDspTime}");
            yield return null;
        }
    }
    /// <summary>
    /// ๊ณตํต???ผ๊ฒฉ ?์ ??????์ ์ฒ๋ฆฌ
    /// </summary>
    protected virtual void CalculateScore(E_NoteDecision decision)
    {
        if (isBoss)
        {
            scoreValue *= (float)decision * 2;
        }
        else
        {
            scoreValue *= (float)decision;
        }
        Debug.Log($"Hit??๊ฒฐ๊ณผ : {decision}, ?์ : {scoreValue}");
    }
    /// <summary>
    /// ๋ฒํผ ?๋ ฅ???ฐ๋ฅธ ?์  ์ฒ๋ฆฌ
    /// </summary>
    public abstract void OnHit(E_NoteDecision decision);
    /// <summary>
    // ?ดํ??์ฒ๋ฆฌ (? ๋๋ฉ์ด???๋ ?ํฐ??
    /// </summary>
    protected void ShowEffect()
    {
        Debug.Log("?ดํ???์");
    }
}







