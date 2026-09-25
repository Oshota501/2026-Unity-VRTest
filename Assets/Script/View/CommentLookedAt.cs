using UnityEngine;

// コメント（TextMeshPro）を常に target の方へ向ける。
// Person が近づいたときに作るコメントには自動で付く。
// エディタで TextMeshPro のオブジェクトに Add Component して使うこともできる。
public class CommentLookedAt : MonoBehaviour
{
    [Tooltip("文字を向ける先（プレイヤーのカメラなど）。未設定なら Main Camera の方を向く。")]
    [SerializeField] private Transform target;

    public void Initialize(Transform target)
    {
        this.target = target;
    }

    void LateUpdate()
    {
        Transform lookTarget = target;
        if (lookTarget == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;

            lookTarget = mainCamera.transform;
        }

        // TextMeshPro は裏側から見ると文字が反転するため、ターゲットの方向を向いたあと180度回転させて正面を向ける
        transform.LookAt(lookTarget);
        transform.Rotate(0f, 180f, 0f);
    }
}
