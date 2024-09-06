using System.Collections;
using System.Collections.Generic;
using System.Xml.XPath;
using UnityEngine;

public class QState : MonoBehaviour
{

    public bool _nWalkable;
    public bool _sWalkable;
    public bool _eWalkable;
    public bool _wWalkable;

    public int _playerUp;
    public int _playerRight;

    public int _idState;

    public QState(bool n, bool s, bool e, bool w, int up, int right, int idState)
    {
        _nWalkable = n;
        _sWalkable = s;
        _eWalkable = e;
        _wWalkable = w;
        _playerUp = up;
        _playerRight = right;
        _idState = idState;
    }
}
