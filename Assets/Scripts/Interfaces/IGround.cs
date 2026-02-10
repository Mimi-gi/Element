using UnityEngine;

namespace Element.Interfaces
{
    public interface IGround
    {
        //接地判定として使う
    }

    public interface IGrounded
    {
        //継承してたら、基本的に設置しなきゃいけない
        bool IsGrounded { get; }
        void ConfigureYVelocity(Rigidbody2D rb);
    }
}