using UnityEngine;
using UnityEngine.InputSystem;

namespace TownReview.Platform.Desktop
{
    // キーボード・マウス操作でのプレイヤー移動。
    // エディタでの動作確認用、および将来のWeb版（デスクトップブラウザ）操作の土台として使う。
    //
    // Eye（視点カメラ）は子オブジェクトから自動取得し、Move/Jumpの入力アクションは
    // Assets/_Project/Input/InputSystem_Actions.inputactions から生成したC#クラスを使う。
    // どちらもInspectorでの手動アサインを不要にするための設計（詳細はCLAUDE.mdの作業ログ参照）。
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("移動設定")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = -9.8f;
        [SerializeField] private float jumpPower = 10f;

        private CharacterController controller;
        private Camera eye;
        private InputSystem_Actions actions;
        private InputAction moveAction;
        private InputAction jumpAction;
        private Vector3 velocity;

        public Camera Eye => eye;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            eye = GetComponentInChildren<Camera>();
            if (eye == null)
            {
                Debug.LogWarning($"{nameof(PlayerController)}: 子オブジェクトにCameraが見つかりません。", this);
            }

            actions = new InputSystem_Actions();
            moveAction = actions.Player.Move;
            jumpAction = actions.Player.Jump;
        }

        private void OnEnable()
        {
            moveAction.Enable();
            jumpAction.Enable();
            jumpAction.performed += OnJump;
        }

        private void OnDisable()
        {
            jumpAction.performed -= OnJump;
            moveAction.Disable();
            jumpAction.Disable();
        }

        private void OnDestroy()
        {
            actions?.Dispose();
        }

        private void Update()
        {
            bool isGrounded = controller.isGrounded;
            if (isGrounded && velocity.y < 0f)
            {
                // 接地している間はわずかにマイナス値を保ち、isGroundedの判定を安定させる
                velocity.y = -2f;
            }

            Vector2 input = moveAction.ReadValue<Vector2>();
            Vector3 moveDirection = transform.TransformDirection(new Vector3(input.x, 0f, input.y)) * moveSpeed;

            velocity.y += gravity * Time.deltaTime;

            controller.Move((moveDirection + velocity) * Time.deltaTime);
        }

        private void OnJump(InputAction.CallbackContext context)
        {
            if (controller.isGrounded)
            {
                velocity.y += jumpPower;
            }
        }
    }
}
