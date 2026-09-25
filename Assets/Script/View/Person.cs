using TMPro;
using UnityEngine;

// 街にいる人1人分。プレイヤーが近づくと少し浮き上がり、コメントを表示する。
//
// コメントの表示に使う TextMeshPro は次の順で決まる。
//   1. Comment Text に設定したもの
//   2. 子オブジェクトにある TextMeshPro（プレハブの Text など。位置・大きさ・色はエディタで調整する）
//   3. どちらもなければ「コメントを自動で作るときの設定」で作る
// 文字は最初は隠しておき、近づいたときだけ表示する。
public class Person : MonoBehaviour
{
    [Header("コメント")]
    [SerializeField, TextArea] private string comment = "hello";

    [Tooltip("コメントを表示する TextMeshPro。未設定なら子オブジェクトから探し、それもなければ自動で作る。")]
    [SerializeField] private TextMeshPro commentText;

    [Tooltip("コメントのフォント。日本語を表示するには、日本語を含むTMPフォントアセット（DotGothic16-Regular SDF など）を設定する。未設定なら TextMeshPro 側のフォントのまま。")]
    [SerializeField] private TMP_FontAsset commentFont;

    [Tooltip("コメントの文字の大きさ。Comment Text（プレハブの Text など）の Font Size もこの値で上書きする。0 なら TextMeshPro 側の大きさのまま。目安：2〜3")]
    [SerializeField] private float commentFontSize = 3f;

    [Tooltip("プレイヤーが近づいたときに浮き上がる高さ（m）")]
    [SerializeField] private float closedLiftHeight = 0.1f;

    [Header("コメントを自動で作るときの設定")]
    [Tooltip("人の原点（足元）からのコメントの位置")]
    [SerializeField] private Vector3 commentOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private Color commentColor = Color.white;
    [SerializeField] private int commentSortingOrder = 100;

    internal PlayerController player;

    private bool isPlayerClosed;

    void Awake()
    {
        if (commentText == null)
        {
            commentText = GetComponentInChildren<TextMeshPro>(true);
        }
        if (commentText == null)
        {
            commentText = CreateCommentText();
        }

        if (commentFont != null)
        {
            commentText.font = commentFont;
        }
        if (commentFontSize > 0f)
        {
            commentText.fontSize = commentFontSize;
        }

        SetCommentVisible(false);
    }

    // 近づいたときに表示するコメントを設定する（APIの住民の voice を入れるときに使う）
    public void SetComment(string text)
    {
        comment = text;
        if (isPlayerClosed)
        {
            commentText.text = comment;
        }
    }

    // 状態が変わったときだけ動かす（毎フレーム位置を書き換えないように）
    public virtual void ClosedPlayer()
    {
        if (isPlayerClosed) return;

        isPlayerClosed = true;
        transform.position += Vector3.up * closedLiftHeight;
        ShowComment();
    }

    public virtual void FarPlayer()
    {
        if (!isPlayerClosed) return;

        isPlayerClosed = false;
        transform.position -= Vector3.up * closedLiftHeight;
        SetCommentVisible(false);
    }

    private void ShowComment()
    {
        commentText.text = comment;

        // 文字をプレイヤーの目（カメラ）へ向ける。目がなければ CommentLookedAt が Main Camera の方を向く
        if (player != null && player.eye != null && commentText.TryGetComponent(out CommentLookedAt lookedAt))
        {
            lookedAt.Initialize(player.eye.transform);
        }

        SetCommentVisible(true);
    }

    private void SetCommentVisible(bool visible)
    {
        if (commentText.gameObject == gameObject)
        {
            // Person と同じオブジェクトに付いている場合は、人ごと消えないよう文字の表示だけを切り替える
            commentText.enabled = visible;
        }
        else
        {
            // 別のオブジェクトならまとめて止める（CommentLookedAt も止まり、毎フレームの処理が減る）
            commentText.gameObject.SetActive(visible);
        }
    }

    private TextMeshPro CreateCommentText()
    {
        // TextMeshPro を付けると Transform が RectTransform に置き換わるため、位置の設定より先に付ける
        var commentObject = new GameObject("Comment");
        var text = commentObject.AddComponent<TextMeshPro>();
        commentObject.transform.SetParent(transform, false);
        commentObject.transform.localPosition = commentOffset;

        text.color = commentColor;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.sortingOrder = commentSortingOrder;

        commentObject.AddComponent<CommentLookedAt>();
        return text;
    }
}
