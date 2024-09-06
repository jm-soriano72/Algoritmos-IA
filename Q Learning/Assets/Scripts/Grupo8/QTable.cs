using NavigationDJIA.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTable : MonoBehaviour
{
    // Número de filas y de columnas
    public int _numRows;
    public int _numCols;
    // Tabla con los valores Q
    public float[,] _tablaQ;
    // Lista con los estados
    public QState[] _listaEstados;

    public QTable (WorldInfo world){
        // En las filas se almacena el número de acciones posibles a realizar (N,S,E,O)
        this._numRows = 4;
        // En las columnas se almacenan todos los estados posibles.
        // Para los estados, se han almacenan los vecinos caminables y la posición del enemigo respecto al jugador ( opuestas o iguales )
        this._numCols = 16*9;
        // Inicialización de la tabla Q
        this._tablaQ = new float[this._numRows, this._numCols];
        this._listaEstados = new QState[_numCols];
    }
    
    public void InicializarTabla()
    {
        for(int i=0; i<_numRows; i++)
        {
            for (int j = 0; j < _numCols; j++) 
            {
                _tablaQ[i, j] = 0;          
            }
        }

        InicializarEstados();
    }

    public void InicializarEstados()
    {
        int indice = 0;
        bool n, s, e, w;
        for(int i= 0; i<16; i++)
        {
            for(int j=0; j<3; j++)
            {
                for(int k=0; k<3; k++)
                {
                    if(i/8%2==0)
                    {
                        n = false;
                    }
                    else
                    {
                        n = true;
                    }

                    if(i/4%2==0)
                    {
                        s = false;
                    }
                    else
                    {
                        s = true;
                    }

                    if(i/2%2==0)
                    {
                        e = false;
                    }
                    else
                    {
                        e = true;
                    }
                    if(i%2==0)
                    {
                        w = false;
                    }
                    else
                    {
                        w = true;
                    }

                    _listaEstados[indice] = new QState(n,s,e,w,j,k,indice); 
                    indice++;
                }
            }
        }

    }

    public float DevolverQ(int accion, bool n, bool s, bool e, bool w, int up, int right)
    {
        int indice = 0;
        // Primero se recorre la lista de estados, para identificar de cuál se trata, en base a las posiciones pasadas como parámetro
        for(int i=0; i<_listaEstados.Length; i++)
        {
            if (n == _listaEstados[i]._nWalkable &&
                s == _listaEstados[i]._sWalkable &&
                w == _listaEstados[i]._wWalkable &&
                e == _listaEstados[i]._eWalkable &&
                up == _listaEstados[i]._playerUp &&
                right == _listaEstados[i]._playerRight)
            {
                // Se guarda el índice del estado
                indice = _listaEstados[i]._idState;
            }
        }
        return _tablaQ[accion, indice];
    }

    public int DevolverMejorAccion(bool n, bool s, bool e, bool w, int up, int right)
    {
        int indice = 0;
        // Primero se recorre la lista de estados, para identificar de cuál se trata, en base a las posiciones pasadas como parámetro
        for (int i = 0; i < _listaEstados.Length; i++)
        {
            if (n == _listaEstados[i]._nWalkable &&
                s == _listaEstados[i]._sWalkable &&
                w == _listaEstados[i]._wWalkable &&
                e == _listaEstados[i]._eWalkable &&
                up == _listaEstados[i]._playerUp &&
                right == _listaEstados[i]._playerRight)
            {
                // Se guarda el índice del estado
                indice = _listaEstados[i]._idState;
            }
        }

        int mejorAccion = 0;
        float mejorQ = -1000f;

        for(int i=0; i< _numRows; i++)
        {
            if (_tablaQ[i,indice]>mejorQ)
            {
                mejorAccion = i;
                mejorQ = _tablaQ[i, indice];
            }
        }

        return mejorAccion;
    }

    public float DevolverMejorQ(bool n, bool s, bool e, bool w, int up, int right)
    {
        int indice = 0;
        // Primero se recorre la lista de estados, para identificar de cuál se trata, en base a las posiciones pasadas como parámetro
        for (int i = 0; i < _listaEstados.Length; i++)
        {
            if (n == _listaEstados[i]._nWalkable &&
                s == _listaEstados[i]._sWalkable &&
                w == _listaEstados[i]._wWalkable &&
                e == _listaEstados[i]._eWalkable &&
                up == _listaEstados[i]._playerUp &&
                right == _listaEstados[i]._playerRight)
            {
                // Se guarda el índice del estado
                indice = _listaEstados[i]._idState;
            }
        }

        float mejorQ = -1000f;

        for (int i = 0; i < _numRows; i++)
        {
            if (_tablaQ[i, indice] >= mejorQ)
            {
                mejorQ = _tablaQ[i, indice];
            }
        }

        return mejorQ;
    }

    public void ActualizarQ(int accion, bool n, bool s, bool e, bool w, int up, int right, float actualizedQ)
    {
        int indice = 0;
        // Primero se recorre la lista de estados, para identificar de cuál se trata, en base a las posiciones pasadas como parámetro
        for (int i = 0; i < _listaEstados.Length; i++)
        {
            if (n == _listaEstados[i]._nWalkable &&
                s == _listaEstados[i]._sWalkable &&
                w == _listaEstados[i]._wWalkable &&
                e == _listaEstados[i]._eWalkable &&
                up == _listaEstados[i]._playerUp &&
                right == _listaEstados[i]._playerRight)
            {
                // Se guarda el índice del estado
                indice = _listaEstados[i]._idState;
            }
        }

        // Actualización de la tabla
        _tablaQ[accion,indice] = actualizedQ;
    }
}
