using UnityEngine;

namespace Element.Interfaces
{
    public interface IHorizontalMove
    {
        void XMove(float direction);
    }

    public interface IVerticalMove
    {
        void YMove(float direction);
    }
}