using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WGH_AreaJudge : MonoBehaviour
{
    [SerializeField] private float _greatDistance;
    [SerializeField] private float _perfectDistance;
    public int Combo { get; private set; }
    private int _perfectCount;
    private int _greatCount;

    private Vector3 _checkTopPos;
    private Vector3 _checkMiddlePos;
    private Vector3 _checkBottomPos;
    private Vector3 _curPos;

    public Note Note { get; private set; }
    [SerializeField] private WGH_PlayerController _playerController = null;
    private WGH_FloatJudgeResult _floatResult = null;
    public WGH_FloatCombo _FloatCombo { get; private set; }

    private KeyCode _inputKey;
    private bool _isInputProcessing;                                    // 키를 입력 받았는가를 확인할 bool 변수
    private bool _isInputedDoubleKey;                                   // 동시 입력 처리를 할 2번째 키를 입력받았는가를 확인할 bool 변수
    [SerializeField, Range(0.01f, 0.5f)] private float _judgeTime;

    [Header("테스트용 임시 프리팹")]
    [SerializeField] private GameObject _great;
    [SerializeField] private GameObject _perfect;

    private bool _isSendedScore = false;

    private void Start()
    {
        _checkTopPos = NoteDirector.Instance.GetCheckPoses(E_SpawnerPosY.TOP);
        _checkMiddlePos = NoteDirector.Instance.GetCheckPoses(E_SpawnerPosY.MIDDLE);
        _checkBottomPos = NoteDirector.Instance.GetCheckPoses(E_SpawnerPosY.BOTTOM);
        _playerController = FindAnyObjectByType<WGH_PlayerController>();
        _floatResult = GetComponent<WGH_FloatJudgeResult>();
        _FloatCombo = GetComponent<WGH_FloatCombo>();
        _FloatCombo.SpawnCombo(Combo);
        _isSendedScore = false;

        EventManager.Instance.AddAction(E_Event.BOSSDEAD, GetBossScore, this);
        EventManager.Instance.AddAction(E_Event.STAGE_END, SentCount, this);
    }

    private void Update()
    {
        if (_isInputProcessing == false && _playerController.IsCanMove&& !_playerController.IsDied && !_playerController.IsContact)
        {
            if (Input.GetKeyDown(KeyCode.F))
                _inputKey = KeyCode.F;
            else if (Input.GetKeyDown(KeyCode.J))
                _inputKey = KeyCode.J;
            else
                return;

            StartCoroutine(StartInputCheck(_inputKey));
        }
    }
    public int CheckCurCombo()
    {
        return Combo;
    }

    public void SentCount()
    {
        DataManager.Instance.SetPerfectCount(_perfectCount);
        DataManager.Instance.SetGreatCount(_greatCount);
    }
    public void AddCombo()
    {
        Combo++;
        // 여기서 콤보 증가가 발생할때마다 오브젝트풀을 통해 생성 or 스프라이트 변경
    }
    public void AddPerfectCount()
    {
        _perfectCount++;
    }
    /// <summary>
    /// 콤보 리셋
    /// </summary>
    public void SetComboReset()
    { Combo = 0; }
    /// <summary>
    /// 노트판정 메서드
    /// </summary>
    public void CheckNote(Vector3 checkPos, E_Boutton button)
    {
        this._curPos = checkPos;
        Vector2 aPoint = new Vector2(_curPos.x - _greatDistance / 2, _curPos.y - _greatDistance / 4);
        Vector2 bPoint = new Vector2(_curPos.x + _greatDistance / 2, _curPos.y + _greatDistance / 4);
        Collider2D[] hits = Physics2D.OverlapAreaAll(aPoint, bPoint);
        Debug.DrawLine(aPoint, bPoint, Color.blue, 0.5f);

        if (hits.Length == 0)
            Note = null;
        foreach (Collider2D hit in hits)
        {
            if (hit.TryGetComponent(out Note note) && !hit.TryGetComponent(out ObstacleNote obstacle))
            {
                Note = note;
                float _distance = Vector2.Distance(_curPos, hit.transform.position);
                Debug.DrawLine(aPoint + new Vector2(0, _greatDistance / 4), bPoint - new Vector2(0, _greatDistance / 4), Color.blue, 0.5f);

                if (_distance <= _perfectDistance)
                {
                    AddCombo();
                    _perfectCount++;
                    _FloatCombo.SpawnCombo(Combo);
                    Note.OnHit(E_NoteDecision.Perfect, button);
                    _floatResult.SpawnResult(E_NoteDecision.Perfect, hit.transform.position + new Vector3(0, 2, 0));  // PERFECT 프리팹 띄우기
                    if (hit.TryGetComponent(out ScoreNote score))                                                     // 스코어 노트 퍼펙트 점수 처리
                    {
                        CalculateScoreNote(E_NoteDecision.Perfect);
                    }
                    else if (hit.TryGetComponent(out MonsterNote monster))                                            // 스코어 노트 그레이트 점수 처리
                    {
                        CalculateScoreMonster(E_NoteDecision.Perfect);
                    }
                }
                else if (_distance <= _greatDistance + 0.2f)
                {
                    AddCombo();
                    _greatCount++;
                    _FloatCombo.SpawnCombo(Combo);
                    Note.OnHit(E_NoteDecision.Great, button);
                    _floatResult.SpawnResult(E_NoteDecision.Great, hit.transform.position + new Vector3(0, 2, 0));   // GREAT 프리팹 띄우기
                    if (hit.TryGetComponent(out ScoreNote score))                                                    // 몬스터 노트 퍼펙트 점수 처리
                    {
                        CalculateScoreNote(E_NoteDecision.Great);
                    }
                    else if (hit.TryGetComponent(out MonsterNote Monster))                                           // 몬스터 노트 그레이트 점수 처리
                    {
                        CalculateScoreMonster(E_NoteDecision.Great);
                    }
                }
            }
        }
    }
    /// <summary>
    /// 점수노트 점수 처리 메서드
    /// </summary>
    private void CalculateScoreNote(E_NoteDecision result)
    {
        int score = 0;
        if (result == E_NoteDecision.Perfect && Note.isBoss)
        {
            score = Mathf.RoundToInt((10 + 10) * 2 * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Great && Note.isBoss)
        {
            score = Mathf.RoundToInt((10 + 10) * 1 * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Perfect)
        {
            score = Mathf.RoundToInt((10 + 10) * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Great)
        {
            score = Mathf.RoundToInt((10 + 10) * 1 * ((Combo * 0.01f) + 1));
        }
        DataManager.Instance.AddScore(score);
    }
    /// <summary>
    /// 몬스터노트 점수 처리 메서드
    /// </summary>
    private void CalculateScoreMonster(E_NoteDecision result)
    {
        int score = 0;
        if (result == E_NoteDecision.Perfect && Note.isBoss)
        {
            score = Mathf.RoundToInt((10 + 20) * 2 * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Great && Note.isBoss)
        {
            score = Mathf.RoundToInt((10 + 20) * 1 * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Perfect)
        {
            score = Mathf.RoundToInt((10 + 20) * 2 * ((Combo * 0.01f) + 1));
        }
        else if (result == E_NoteDecision.Great)
        {
            score = Mathf.RoundToInt((10 + 20) * 1 * ((Combo * 0.01f) + 1));
        }
        DataManager.Instance.AddScore(score);
    }
    /// <summary>
    /// 보스 처치시 획득 점수
    /// </summary>
    private void GetBossScore()
    {
        if (_isSendedScore == true)
            return;

        _isSendedScore = true;

        int score = 0;
        score = DataManager.Instance.Boss.Score + _playerController.GetHpScore();
        DataManager.Instance.AddScore(score);
        DataManager.Instance.ClearBossData();

        Debug.Log(DataManager.Instance.CurScore);
    }
    // 첫 입력 코루틴
    IEnumerator StartInputCheck(KeyCode key)
    {
        _isInputProcessing = true;
        _isInputedDoubleKey = false;

        KeyCode nextKey = KeyCode.None;                                          // 2번째 키를 받아둘 KeyCode
        Action nextAction = null;                                                // 경우에 따라 기능을 달리하기 위한 델리게이트

        // F를 눌렀을 경우 동시입력을 위해 필요한 키를 J로 정하는 조건문
        if (key == KeyCode.F)
        {
            // 상단 제거
            CheckNote(_checkTopPos, E_Boutton.F_BOUTTON);                        // F가 입력될경우 동시입력 여부에 관계없이 진행할 함수

            nextKey = KeyCode.J;                                                 // 다음으로 받으면 동시입력이 진행될 키 지정
            nextAction = () => CheckNote(_checkBottomPos, E_Boutton.J_BOUTTON);  // F가 입력되고나서 J가 입력될 경우 사용할 함수 델리게이트에 할당
        }
        // J를 눌렀을 경우 동시입력을 위해 필요한 키를 F로 정하는 조건문
        else if (key == KeyCode.J)
        {
            // 하단 제거
            CheckNote(_checkBottomPos, E_Boutton.J_BOUTTON);                     // J가 입력될 경우 동시입력 여부에 관계없이 진행할 함수
            nextKey = KeyCode.F;                                                 // 다음으로 받으면 동시입력이 진행될 키 지정
            nextAction = () => CheckNote(_checkTopPos, E_Boutton.F_BOUTTON);     // J가 입력되고 나서 F가 입력될 경우 사용할 함수 델리게이트에 할당
        }

        // 이중 코루틴 시작
        yield return StartCoroutine(InputAfterKey(nextKey, nextAction));

        // 즉시 동작하지 않고 텀을 두고 진행해야 할 동작이 있으면 아래에 지정

        // 동시 입력이 확인 되었으면 작동할 조건문
        if (_isInputedDoubleKey == true)
        {
            // 동시공격 애니메이션
            _playerController.SetAnim("MiddleAttack");
        }
        else
        {
            if (key == KeyCode.F)
            {
                // 판정했을 때 노트가 없을 경우 && 땅에 있는 상태일 경우 "일반 점프" 애니메이션
                if (Note == null /*&& !_playerController.IsAir*/)
                {
                    _playerController.IsAirControl(true);                        // 플레이어 체공상태 여부 true
                    _playerController.JumpMove();
                    _playerController.SetAnim("Jump");
                }
                // 판정했을 때 노트가 있을 경우 && 땅에 있는 상태일 경우 "점프 공격" 애니메이션
                else if (Note != null/* && !_playerController.IsAir*/)
                {
                    _playerController.IsAirControl(true);                        // 플레이어 체공상태 여부 true
                    _playerController.JumpMove();
                    _playerController.SetAnim("JumpAttack");
                }
            }
            else if (_playerController.IsAir && key == KeyCode.J)
            {
                _playerController.SetAnim("FallAttack");
                _playerController.transform.position = _playerController.StartPos;
            }
            else if (key == KeyCode.J)
            {
                if (!_playerController.IsAir)
                    _playerController.SetAnim("GroundAttack");
            }
        }
        _isInputProcessing = false;
    }

    // 두번째 입력 코루틴
    IEnumerator InputAfterKey(KeyCode key, Action nextAction)
    {
        float _timer = 0f;                                      // 경과 시간을 받을 타이머

        while (_timer < _judgeTime)                             // 지정한 시간을 넘을 경우 코루틴 종료
        {
            if (Input.GetKeyDown(key))
            {
                nextAction?.Invoke();                           // 할당 해두었던 델리게이트 함수 실행
                _isInputedDoubleKey = true;                     // 동시입력 확인 bool true
                break;
            }
            _timer += Time.deltaTime;
            yield return null;
        }
    }
}
