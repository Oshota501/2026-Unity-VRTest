using TMPro;
using UnityEngine;

public class Person : MonoBehaviour
{
    [SerializeField, TextArea] private string comment = "hello";
    [SerializeField] private Vector3 commentOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float commentFontSize = 3f;
    [SerializeField] private Color commentColor = Color.white;
    [SerializeField] private int commentSortingOrder = 100;

    internal PlayerController player;

    protected class CommentLookedAt : MonoBehaviour
    {
        private Transform target;

        public void Initialize(Transform target)
        {
            this.target = target;
        }

        void LateUpdate()
        {
            if (target == null) return;

            // TextMeshPro は裏側から見ると文字が反転するため、ターゲットの方向を向いたあと180度回転させて正面を向ける
            transform.LookAt(target);
            transform.Rotate(0f, 180f, 0f);
        }
    }
    private Vector3 bassHeight;
    private bool isPlayerClosed;
    private TextMeshPro commentText;

    void Start()
    {
        bassHeight = transform.position;
    }

    // 表示するコメントを外から設定する（APIから取得した住民のコメントを入れるときに使う）
    public void SetComment(string text)
    {
        comment = text;
        if (commentText != null)
        {
            commentText.text = text;
        }
    }
    public virtual void ClosedPlayer()
    {
        transform.position = bassHeight + new Vector3(0f, 0.1f, 0f);
        if (isPlayerClosed) return;

        isPlayerClosed = true;
        ShowComment();
    }
    public virtual void FarPlayer()
    {
        transform.position = bassHeight;
        if (!isPlayerClosed) return;

        isPlayerClosed = false;
        HideComment();
    }

    private void ShowComment()
    {
        GameObject commentObject = new GameObject("Comment");
        commentObject.transform.SetParent(transform, false);
        commentObject.transform.localPosition = commentOffset;

        commentText = commentObject.AddComponent<TextMeshPro>();
        commentText.text = comment;
        commentText.fontSize = commentFontSize;
        commentText.color = commentColor;
        commentText.alignment = TextAlignmentOptions.Center;
        commentText.textWrappingMode = TextWrappingModes.NoWrap;
        commentText.sortingOrder = commentSortingOrder;

        commentObject.AddComponent<CommentLookedAt>().Initialize(player.eye.transform);
    }

    private void HideComment()
    {
        if (commentText == null) return;

        Destroy(commentText.gameObject);
        commentText = null;
    }
}
