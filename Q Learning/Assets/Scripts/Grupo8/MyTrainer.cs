using NavigationDJIA.Interfaces;
using NavigationDJIA.World;
using QMind;
using QMind.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Cache;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;


public class MyTrainer : IQMindTrainer
{
    public int CurrentEpisode { get; private set; }
    public int CurrentStep { get; private set; }
    public CellInfo AgentPosition { get; private set; }
    public CellInfo OtherPosition { get; private set; }
    public float Return { get; }
    public float ReturnAveraged { get; }
    public event EventHandler OnEpisodeStarted;
    public event EventHandler OnEpisodeFinished;

    private INavigationAlgorithm _navigationAlgorithm;
    private WorldInfo _worldInfo;

    // Tabla Q
    private QTable _QTable;
    // Parámetros del algoritmo
    private QMindTrainerParams _params;

    private int counter = 0;
    private int numEpisode = 0;
    private int accionAnterior = -1;


    public void DoStep(bool train)
    {
        CellInfo agentNextCell;
        CellInfo currentCell = AgentPosition;
        int accion = -1;
        
        do
        {
            if(EscogerNumeroAleatorio())
            {
                accion = AccionAleatoria();
            }
            else
            {
                accion = MejorAccion();
            }

            // Según el código obtenido para la acción, se asigna una nueva posición para el agente
            // 0 - Norte, 1 - Este, 2 - Sur, 3 - Oeste
            agentNextCell = QMind.Utils.MoveAgent(accion, AgentPosition, _worldInfo);
            if(!agentNextCell.Walkable)
            {
                // Penalizar esa acción gravemente, pero sin realizarla para no provocar un error
                float Q = DevolverQ(accion);
                float QNew = ActualizarQ(Q, 0, -10000);
                ActualizarTablaQ(accion, QNew);
            }

        } while(!agentNextCell.Walkable);
        
        // ACTUALIZACIÓN DE LA TABLA Q
        // 1 - Se obtiene la Q del estado actual y la acción escogida
        float currentQ = DevolverQ(accion);
        // 2 - Se obtiene la mejor Q del estado siguiente
        float bestNextQ = DevolverMaxQ(agentNextCell);
        // 3 - Se calcula la recompensa al realizar la acción
        int reward = GetReward(agentNextCell, accion);
        // 4 - Se calcula el nuevo valor de Q aplicando la fórmula
        float actualizedQ = ActualizarQ(currentQ, bestNextQ, reward);
        // 5 - Se actualiza la tabla con el nuevo valor de Q calculado
        ActualizarTablaQ(accion, actualizedQ);

        accionAnterior = accion;

        AgentPosition = agentNextCell;
        CellInfo otherCell = QMind.Utils.MoveOther(_navigationAlgorithm, OtherPosition, AgentPosition);
        OtherPosition = otherCell;

        CurrentStep = counter;
        // En el caso de que el player alcance al agente, o se llegue al máximo de pasos, se finaliza el episodio actual y se comienza uno nuevo
        if(OtherPosition == null || CurrentStep == _params.maxSteps || OtherPosition == AgentPosition)
        {
            OnEpisodeFinished?.Invoke(this, EventArgs.Empty);
            NuevoEpisodio();
        }
        else
        {
            counter += 1;
        }

    }

    private void NuevoEpisodio()
    {
        AgentPosition = _worldInfo.RandomCell();
        OtherPosition = _worldInfo.RandomCell();
        counter = 0;
        CurrentStep = counter;
        numEpisode++;
        CurrentEpisode = numEpisode;
        if(numEpisode%_params.episodesBetweenSaves==0)
        {
            GuardarTablaQ();
        }
        OnEpisodeStarted?.Invoke(this, EventArgs.Empty);
    }

    public void Initialize(QMindTrainerParams qMindTrainerParams, WorldInfo worldInfo, INavigationAlgorithm navigationAlgorithm)
    {
        Debug.Log("QMindTrainer: initialized");

        _navigationAlgorithm = Utils.InitializeNavigationAlgo(navigationAlgorithm, worldInfo);
        _worldInfo = worldInfo;

        AgentPosition = worldInfo.RandomCell();
        OtherPosition = worldInfo.RandomCell();
        OnEpisodeStarted?.Invoke(this, EventArgs.Empty);

        // Almacenas los parámetros
        _params = qMindTrainerParams;
        // Creas e inicializas la tabla Q
        _QTable = new QTable(worldInfo);
        _QTable.InicializarTabla();
       
    }

    private bool EscogerNumeroAleatorio()
    {
        float azar = UnityEngine.Random.Range(0.0f, 1.0f);
        if(azar <= _params.epsilon)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private int AccionAleatoria()
    {
        int accion = UnityEngine.Random.Range(0, 4);
        return accion;
    }

    private int MejorAccion()
    {
        // Para escoger la mejor acción, primero debemos averiguar el estado en el que nos encontramos actualmente
        // De esta forma, se escoge la acción que aporte más Q

        int posX = AgentPosition.x;
        int posY = AgentPosition.y;

        CellInfo north = QMind.Utils.MoveAgent(0, AgentPosition, _worldInfo);
        bool n = north.Walkable;

        CellInfo south = QMind.Utils.MoveAgent(2, AgentPosition, _worldInfo);
        bool s = south.Walkable;

        CellInfo east = QMind.Utils.MoveAgent(1, AgentPosition, _worldInfo);
        bool e = east.Walkable;

        CellInfo west = QMind.Utils.MoveAgent(3, AgentPosition, _worldInfo);
        bool w = west.Walkable;

        int up = 0, right = 0;
        if(OtherPosition.x > AgentPosition.x)
        {
            right = 0;
        }
        if(OtherPosition.x < AgentPosition.x)
        {
            right = 1;
        }
        if(OtherPosition.x == AgentPosition.x)
        {
            right = 2;
        }

        if (OtherPosition.y > AgentPosition.y)
        {
            up = 0;
        }
        if (OtherPosition.y < AgentPosition.y)
        {
            up = 1;
        }
        if (OtherPosition.y == AgentPosition.y)
        {
            up = 2;
        }

        return _QTable.DevolverMejorAccion(n,s,e,w,up,right);

    }

    private float DevolverQ (int accion)
    {
        int posX = AgentPosition.x;
        int posY = AgentPosition.y;

        CellInfo north = QMind.Utils.MoveAgent(0, AgentPosition, _worldInfo);
        bool n = north.Walkable;

        CellInfo south = QMind.Utils.MoveAgent(2, AgentPosition, _worldInfo);
        bool s = south.Walkable;

        CellInfo east = QMind.Utils.MoveAgent(1, AgentPosition, _worldInfo);
        bool e = east.Walkable;

        CellInfo west = QMind.Utils.MoveAgent(3, AgentPosition, _worldInfo);
        bool w = west.Walkable;

        int up = 0, right = 0;
        if (OtherPosition.x > AgentPosition.x)
        {
            right = 0;
        }
        if (OtherPosition.x < AgentPosition.x)
        {
            right = 1;
        }
        if (OtherPosition.x == AgentPosition.x)
        {
            right = 2;
        }

        if (OtherPosition.y > AgentPosition.y)
        {
            up = 0;
        }
        if (OtherPosition.y < AgentPosition.y)
        {
            up = 1;
        }
        if (OtherPosition.y == AgentPosition.y)
        {
            up = 2;
        }

        return _QTable.DevolverQ(accion, n,s,e,w,up,right);
    }

    private float DevolverMaxQ(CellInfo nextCell)
    {
        int posX = AgentPosition.x;
        int posY = AgentPosition.y;

        CellInfo north = QMind.Utils.MoveAgent(0, AgentPosition, _worldInfo);
        bool n = north.Walkable;

        CellInfo south = QMind.Utils.MoveAgent(2, AgentPosition, _worldInfo);
        bool s = south.Walkable;

        CellInfo east = QMind.Utils.MoveAgent(1, AgentPosition, _worldInfo);
        bool e = east.Walkable;

        CellInfo west = QMind.Utils.MoveAgent(3, AgentPosition, _worldInfo);
        bool w = west.Walkable;

        int up = 0, right = 0;
        if (OtherPosition.x > AgentPosition.x)
        {
            right = 0;
        }
        if (OtherPosition.x < AgentPosition.x)
        {
            right = 1;
        }
        if (OtherPosition.x == AgentPosition.x)
        {
            right = 2;
        }

        if (OtherPosition.y > AgentPosition.y)
        {
            up = 0;
        }
        if (OtherPosition.y < AgentPosition.y)
        {
            up = 1;
        }
        if (OtherPosition.y == AgentPosition.y)
        {
            up = 2;
        }

        return _QTable.DevolverMejorQ(n, s, e, w, up, right);
    }

    private int GetReward(CellInfo nextCell, int accion)
    {
        int distanciaRealInicial = Mathf.Abs(AgentPosition.x - OtherPosition.x) + Mathf.Abs(AgentPosition.y - OtherPosition.y);
        int distanciaRealFinal = Mathf.Abs(nextCell.x - OtherPosition.x) + Mathf.Abs(nextCell.y - OtherPosition.y);

        int recompensa = 0;
        if(nextCell.x == OtherPosition.x && nextCell.y == OtherPosition.y) { return -100; }
        if(distanciaRealFinal > distanciaRealInicial)
        {
            recompensa += 100;
        }
        else
        {
            if(distanciaRealFinal<=2)
            {
                recompensa -= 100;
            }
           recompensa += -10;
            
        }
        if((nextCell.x == 0 && nextCell.y == 19) ||
                (nextCell.x == 0 && nextCell.y == 0) ||
                (nextCell.x == 19 && nextCell.y == 0) ||
                (nextCell.x == 19 && nextCell.y == 19))
        {
            recompensa -= 1000;
        }
               
        return recompensa;
           
    }

    private float ActualizarQ(float currentQ, float maxNextQ, int reward)
    {
        float actualizedQ = (1 - _params.alpha) * currentQ + _params.alpha * (reward + _params.gamma * maxNextQ);
        return actualizedQ;
    }

    private void ActualizarTablaQ(int accion, float actualizedQ) {

        int posX = AgentPosition.x;
        int posY = AgentPosition.y;

        CellInfo north = QMind.Utils.MoveAgent(0, AgentPosition, _worldInfo);
        bool n = north.Walkable;

        CellInfo south = QMind.Utils.MoveAgent(2, AgentPosition, _worldInfo);
        bool s = south.Walkable;

        CellInfo east = QMind.Utils.MoveAgent(1, AgentPosition, _worldInfo);
        bool e = east.Walkable;

        CellInfo west = QMind.Utils.MoveAgent(3, AgentPosition, _worldInfo);
        bool w = west.Walkable;

        int up = 0, right = 0;
        if (OtherPosition.x > AgentPosition.x)
        {
            right = 0;
        }
        if (OtherPosition.x < AgentPosition.x)
        {
            right = 1;
        }
        if (OtherPosition.x == AgentPosition.x)
        {
            right = 2;
        }

        if (OtherPosition.y > AgentPosition.y)
        {
            up = 0;
        }
        if (OtherPosition.y < AgentPosition.y)
        {
            up = 1;
        }
        if (OtherPosition.y == AgentPosition.y)
        {
            up = 2;
        }

        _QTable.ActualizarQ(accion, n, s, e, w, up, right, actualizedQ);
    }

    private void GuardarTablaQ()
    {
        File.WriteAllLines(@"Assets/Scripts/Grupo8/TablaQ.csv",
            ToCsv(_QTable._tablaQ));
    }

    private static IEnumerable<String> ToCsv<T>(T[,] data, string separator = ";")
    {
        for (int i = 0; i < data.GetLength(0); ++i)
            yield return string.Join(separator, Enumerable
              .Range(0, data.GetLength(1))
              .Select(j => data[i, j]));
    }

}

