namespace TownReview.Core.Interaction
{
    // 触れられるものが実装するインターフェース。
    // Platform層がそれぞれの方法（VRコントローラー、タップ、クリックなど）でInteract()を呼び出す。
    public interface IInteractable
    {
        void Interact();
    }
}
