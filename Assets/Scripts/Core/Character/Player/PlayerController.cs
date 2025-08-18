using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    private float _movementSpeed; // 이동 속도
    private float _rotationSpeed; // 회전 속도

    private float _attackRange;    // 공격 범위
    private float _attackDamage;   // 공격 데미지

    // CharacterBase에서 StatSystem을 가져와 이동 속도에 적용
    private IStatProvider _statProvider;
    private Animator _animator; // 애니메이션 컨트롤러 (필요시 사용)
    private Rigidbody _rigidBody;
    private InputSystem_Actions _inputActions; // 생성된 입력 액션 클래스
    private Vector2 _moveInput; // 입력받은 이동 방향 (Vector2)

    void Awake()
    {
        _statProvider = GetComponent<IStatProvider>();
        _animator = GetComponentInChildren<Animator>();
        _rigidBody = GetComponent<Rigidbody>();
        _inputActions = new InputSystem_Actions();
        // Move 액션에 대한 콜백 설정
        _inputActions.Player.Move.performed += OnMovePerformed;
        _inputActions.Player.Move.canceled += OnMoveCanceled;

        // Attack 액션에 대한 콜백 설정
        _inputActions.Player.Attack.performed += OnAttack;
    }

    void Start()
    {
        // 초기화
        _movementSpeed = _statProvider != null ? _statProvider.FinalStats.movementSpeed : 5f; // 기본 이동 속도
        _rotationSpeed = _statProvider != null ? _statProvider.FinalStats.rotationSpeed : 10f; // 기본 회전 속도

        _attackDamage = _statProvider != null ? _statProvider.FinalStats.attackPower : 10f;
        _attackRange = _statProvider != null ? _statProvider.FinalStats.attackRange : 1.5f;
    }

    private void Update()
    {
        float animatorSpeed = 0f;
        if (_statProvider != null)
            animatorSpeed = _statProvider.FinalStats.movementSpeed * _moveInput.magnitude;
        _animator.SetFloat("Speed", animatorSpeed);
    }

    void OnEnable()
    {
        _inputActions.Enable(); // 스크립트 활성화 시 입력 액션 활성화
    }
    void OnDisable()
    {
        _inputActions.Disable(); // 스크립트 비활성화 시 입력 액션 비활성화
    }
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
        _animator.SetTrigger("Attack");

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

    void FixedUpdate() // 물리 업데이트는 FixedUpdate에서 처리
    {
        MovePlayer();
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
        // --- 회전 로직 끝 ---

        _rigidBody.MovePosition(_rigidBody.position + moveDirection * _movementSpeed * Time.fixedDeltaTime);
    }

    // 이 스크립트가 붙은 게임오브젝트를 선택했을 때, 씬 뷰에만 기즈모를 그립니다.
    private void OnDrawGizmosSelected()
    {
        // 기즈모의 색상을 빨간색으로 설정
        Gizmos.color = Color.red;

        // 공격 판정이 일어나는 위치를 계산
        Vector3 attackPoint = transform.position + transform.forward;

        // 해당 위치에 _attackRange를 반지름으로 하는 와이어(선) 구체를 그립니다.
        Gizmos.DrawWireSphere(attackPoint, _attackRange);
    }
}
