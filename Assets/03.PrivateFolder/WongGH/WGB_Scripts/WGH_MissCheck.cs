using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WGH_MissCheck : MonoBehaviour
{
    [SerializeField] WGH_PlayerController _player;
    [SerializeField] WGH_JudgeCircle _jucgeCircle;
    WGH_TestNodeClass _testNote;
    void Start()
    {
        _player = FindAnyObjectByType<WGH_PlayerController>();
        _jucgeCircle = FindAnyObjectByType<WGH_JudgeCircle>();
        _testNote = FindAnyObjectByType<WGH_TestNodeClass>();
    }

    void Update()
    {
        Debug.Log($"노트와 플레이어 거리{Vector2.Distance(_player.transform.position, _testNote.transform.position)}");
        Debug.Log(Vector2.Distance(_player.transform.position, _jucgeCircle._testLeftPrefab.transform.position));
        if(!_jucgeCircle._left &&
            !_jucgeCircle._right &&
            Vector2.Distance(_player.transform.position, _testNote.transform.position) < Vector2.Distance(_player.transform.position, _jucgeCircle._testLeftPrefab.transform.position))
        {
            Debug.Log("Miss");
        }
    }
}
