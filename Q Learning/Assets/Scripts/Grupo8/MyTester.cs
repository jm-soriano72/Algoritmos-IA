using NavigationDJIA.World;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MyTester : IQMind
{
    private WorldInfo _worldInfo;
    // Parámetros para cargar la tabla Q
    // Número de filas y de columnas
    private int _numRows = 4;
    private int _numCols = 16*9;
    // Tabla con los valores Q
    private float[,] _tablaQ;
    // Lista con los estados
    private QState[] _listaEstados = new QState[16*9];

    public void Initialize(WorldInfo worldInfo)
    {
        _worldInfo = worldInfo;
        InicializarEstados();
        LoadQTable();
    }

    public CellInfo GetNextStep(CellInfo currentPosition, CellInfo otherPosition)
    {
        QState state = CalculateState(currentPosition, otherPosition);
        CellInfo agentCell = null;
        
            int action = GetAction(state);
            agentCell = QMind.Utils.MoveAgent(action, currentPosition, _worldInfo);
            Debug.Log("Action = " + action);
        if(!agentCell.Walkable)
        {
            CalculateState(currentPosition, otherPosition);
        }
        Debug.Log(currentPosition.x.ToString() + "" + currentPosition.y.ToString());    

        return agentCell;
    }

    private void InicializarEstados()
    {
        int indice = 0;
        bool n, s, e, w;
        for (int i = 0; i < 16; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (i / 8 % 2 == 0)
                    {
                        n = false;
                    }
                    else
                    {
                        n = true;
                    }

                    if (i / 4 % 2 == 0)
                    {
                        s = false;
                    }
                    else
                    {
                        s = true;
                    }

                    if (i / 2 % 2 == 0)
                    {
                        e = false;
                    }
                    else
                    {
                        e = true;
                    }
                    if (i % 2 == 0)
                    {
                        w = false;
                    }
                    else
                    {
                        w = true;
                    }

                    _listaEstados[indice] = new QState(n, s, e, w, j, k, indice);
                    indice++;
                }
            }
        }
    }

    private int GetAction(QState state)
    {
        // Usar tabla Q aprendida para seleccionar la mejor acción.
        int mejorAccion = 0;
        float mejorQ = -1000f;

        for (int i = 0; i < _numRows; i++)
        {
            if (_tablaQ[i, state._idState] >= mejorQ)
            {
                mejorAccion = i;
                mejorQ = _tablaQ[i, state._idState];
            }
        }

        return mejorAccion;
    }

    // Función para cargar los datos de la tabla Q conseguida tras el entrenamiento
    private void LoadQTable()
    {
        string filePath = @"Assets/Scripts/Grupo8/TablaQ.csv";
        StreamReader reader;
        if(File.Exists(filePath)) {
            reader = new StreamReader(File.OpenRead(filePath));
            _tablaQ = new float[_numRows, _numCols];
            int contador = 0;
            while(!reader.EndOfStream && contador<_numRows)
            {
                var line = reader.ReadLine();
                var values = line.Split(';');
                for(int i=0; i<values.Length; i++)
                {
                    _tablaQ[contador, i] = (float)Convert.ToDouble(values[i]);
                }
                contador++;
            }
        }
    }

    private QState CalculateState(CellInfo currentPosition, CellInfo otherPosition)
    {
        int posX = currentPosition.x;
        int posY = currentPosition.y;

        CellInfo north = QMind.Utils.MoveAgent(0, currentPosition, _worldInfo);
        bool n = north.Walkable;

        CellInfo south = QMind.Utils.MoveAgent(2, currentPosition, _worldInfo);
        bool s = south.Walkable;

        CellInfo east = QMind.Utils.MoveAgent(1, currentPosition, _worldInfo);
        bool e = east.Walkable;

        CellInfo west = QMind.Utils.MoveAgent(3, currentPosition, _worldInfo);
        bool w = west.Walkable;

        int up = 0, right = 0;
        if (otherPosition.x > currentPosition.x)
        {
            right = 0;
        }
        if (otherPosition.x < currentPosition.x)
        {
            right = 1;
        }
        if (otherPosition.x == currentPosition.x)
        {
            right = 2;
        }

        if (otherPosition.y > currentPosition.y)
        {
            up = 0;
        }
        if (otherPosition.y < currentPosition.y)
        {
            up = 1;
        }
        if (otherPosition.y == currentPosition.y)
        {
            up = 2;
        }


        for (int i = 0; i < _listaEstados.Length; i++)
        {
            if (n == _listaEstados[i]._nWalkable &&
                s == _listaEstados[i]._sWalkable &&
                w == _listaEstados[i]._wWalkable &&
                e == _listaEstados[i]._eWalkable &&
                up == _listaEstados[i]._playerUp &&
                right == _listaEstados[i]._playerRight)
            {
                // Se devuelve el estado
                return _listaEstados[i];
            }
        }
        return null;
    }
}
