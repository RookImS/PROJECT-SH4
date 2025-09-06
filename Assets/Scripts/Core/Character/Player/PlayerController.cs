using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    // 애니메이션 파라미터
    private const string ANI_SPEED = "Speed";
    private const string ANI_ATTACK = "Attack";
    private const string ANI_STATE_PLAYER_ATTACK = "Player_Attack";
    private const int UPPER_BODY_LAYER = 1;

    private float _movementSpeed; // 이동 속도
    private float _rotationSpeed; // 회전 속도

    private float _attackRange;    // 공격 범위
    private float _attackDamage;   // 공격 데미지

    private CharacterBase _characterBase;
    private IStatProvider _statProvider;
    private Animator _animator; // 애니메이션 컨트롤러
    private Rigidbody _rigidBody;
    private InputSystem_Actions _inputActions; // 생성된 입력 액션 클래스
    private Vector2 _moveInput; // 입력받은 이동 방향 (Vector2)
    #endregion

    #region Unity Methods
    void Awake()
    {
        _characterBase = GetComponent<CharacterBase>();
        if (_characterBase != null)
            _statProvider = _characterBase; // CharacterBase는 IStatProvider 구현
        else
            _statProvider = GetComponent<IStatProvider>(); // 폴백

        _animator = GetComponent<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
    }

    void Start()
    {
        // 초기화
        _movementSpeed = _statProvider != null ? _statProvider[StatType.MovementSpeed] : 5f; // 기본 이동 속도
        _rotationSpeed = _statProvider != null ? _statProvider[StatType.RotationSpeed] : 10f; // 기본 회전 속도
        _attackDamage = _statProvider != null ? _statProvider[StatType.AttackPower] : 10f;
        _attackRange = _statProvider != null ? _statProvider[StatType.AttackRange] : 1.5f;

        // 입력 액션에 콜백 함수 연결
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;
        _inputActions.Player.Attack.performed += OnAttack;
    }

    private void Update()
    {
        float animatorSpeed = 0f;
        if (_characterBase != null)
            animatorSpeed = _moveInput.magnitude;
        _animator.SetFloat(ANI_SPEED, animatorSpeed);
    }

    void FixedUpdate() // 물리 업데이트는 FixedUpdate에서 처리
    {
        MovePlayer();
    }

    void OnEnable()
    {
        _inputActions.Enable(); // 스크립트 활성화 시 입력 액션 활성화
    }
    void OnDisable()
    {
        _inputActions.Disable(); // 스크립트 비활성화 시 입력 액션 비활성화
    }
    #endregion

    #region Input Callbacks
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        _moveInput = Vector2.zero;
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(UPPER_BODY_LAYER);

        if (stateInfo.IsName(ANI_STATE_PLAYER_ATTACK) || _animator.IsInTransition(UPPER_BODY_LAYER))
            return; // 이미 공격 애니메이션이 재생 중이거나 애니메이터가 전환 중이면 무시

        _animator.SetTrigger(ANI_ATTACK);
    }
    #endregion

    #region Core Logic
    /// <summary>
    /// 공격 애니메이션 이벤트에서 호출할 함수
    /// </summary>
    public void PerformHitCheck()
    {
        _characterBase.TryUseSkill(0);
    }

    /// <summary>
    /// 콜라이더 사용한 정면 공격 처리
    /// </summary>
    private void ColliderAttack()
    {
        Vector3 attackPoint = transform.position + transform.forward;

        Collider[] hitColliders = Physics.OverlapSphere(attackPoint, _attackRange);

        if (hitColliders == null)
            return;

        foreach (var hitCollider in hitColliders)
        {
            // 자기 자신은 공격 대상에서 제외
            if (hitCollider.gameObject == this.gameObject) continue;

            CharacterBase character = hitCollider.GetComponent<CharacterBase>();

            if (character != null) character.TakeDamage(_attackDamage);
            else Debug.Log($"{hitCollider.name} 에서 CharacterBase가 없습니다!!");
        }
    }

    private void MovePlayer()
    {
        Vector3 moveDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        // --- 회전 로직 ---
        if (moveDirection != Vector3.zero)
        {
            // 목표 방향을 바라보는 회전값(Quaternion)을 계산
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            // Rigidbody의 회전을 부드럽게 변경
            Quaternion newRotation = Quaternion.Slerp(_rigidBody.rotation, targetRotation, _rotationSpeed * Time.fixedDeltaTime);
            _rigidBody.MoveRotation(newRotation);
        }

        _rigidBody.MovePosition(_rigidBody.position + moveDirection * _movementSpeed * Time.fixedDeltaTime);
    }
    #endregion

    #region Test Code
    // 테스트 코드
    // 이 스크립트가 붙은 게임오브젝트를 선택했을 때, 기즈모 표시
    private void OnDrawGizmosSelected()
    {
        // 기즈모의 색상을 빨간색으로 설정
        Gizmos.color = Color.red;

        // 공격 판정이 일어나는 위치를 계산
        Vector3 attackPoint = transform.position + transform.forward;

        // 해당 위치에 _attackRange를 반지름으로 하는 와이어(선) 구체를 그립니다.
        Gizmos.DrawWireSphere(attackPoint, _attackRange);
    }
    #endregion
}
