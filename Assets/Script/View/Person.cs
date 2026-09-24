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

            transform.LookAt(target);
        }
    }
    private Vector3 bassHeight;
    private bool isPlayerClosed;
    private TextMeshPro commentText;

    void Start()
    {
        bassHeight = transform.position;
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
