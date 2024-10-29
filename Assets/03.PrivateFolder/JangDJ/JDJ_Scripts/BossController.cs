using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private BossStat _stat = new BossStat();
    [SerializeField] private BossMovement _movement = null;

    private E_SpawnerPosY _curPos = E_SpawnerPosY.NONE;
    private E_SpawnerPosY _nextPos = E_SpawnerPosY.NONE;

    public IState CurrentState { get; private set; }
    public BossIntoField IntoField;
    public BossIdle IdleState;
    public BossAttack AttackState;
    public BossMove MoveState;
    public BossRush RushState;
    public BossRushReady RushReadyState;
    public BossClosedPlayer ClosedPlayerState;
    public BossDead DeadState;
    public BossRecover RecoverState;

    public BossStat Stat => _stat;

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        InitStates();
        SetInitState(IntoField);
    }

    private void InitStates()
    {
        IntoField = new BossIntoField(this);
        IdleState = new BossIdle(this);
        AttackState = new BossAttack(this);
        MoveState = new BossMove(this);
        RushState = new BossRush(this);
        RushReadyState = new BossRushReady(this);
        ClosedPlayerState = new BossClosedPlayer(this,5);
        DeadState = new BossDead(this);
        RecoverState = new BossRecover(this);
    }

    private void SetInitState(IState state)
    {
        this.CurrentState = state;
        this.CurrentState.Enter();
    }

    public void SetState(IState nextState)
    {
        CurrentState.Exit();
        CurrentState = nextState;
        CurrentState.Enter();
    }

    public void Dead()
    {
        Destroy(this.gameObject); // �ӽ�
    }

    private void Update()
    {
        if (CurrentState != null)
            CurrentState.Update();

        if(Input.GetKeyDown(KeyCode.A))
        {
            SetState(RushReadyState);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SetState(RecoverState);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            SetState(DeadState);
        }
    }
}
