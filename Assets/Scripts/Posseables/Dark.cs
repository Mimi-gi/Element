using UnityEngine;
using Element.Interfaces;

public class Dark : MonoBehaviour, IPossable
{
    //こいつは
    public void TryPossess()
    {

    }
    [SerializeField] int _layer;
    public int Layer => _layer;

    public bool IsPossess { get; set; }

    [SerializeField] Transform _core;
    public Transform Core => _core;

    public void Death()
    {

    }
}