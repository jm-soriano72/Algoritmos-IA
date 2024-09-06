using Assets.Scripts.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class AStarMind : AbstractPathMind {

        public List<Nodo> listaAbierta = new List<Nodo>();
        public List<Nodo> listaCerrada = new List<Nodo>();

        private int costeAcumulado = 0;

        public List<Locomotion.MoveDirection> camino = new List<Locomotion.MoveDirection>();
        private int indiceCamino = 0;

        private Nodo nodoActual;

        public override void Repath()
        {
            
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            if (camino.Count==0)
            {
                AlgoritmoBusqueda(boardInfo, currentPos, goals);
            }
            Locomotion.MoveDirection nextMove = camino[indiceCamino];
            indiceCamino--;
            return nextMove;

        }


        public void AlgoritmoBusqueda(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            // PRIMERO SE FORMA UN NODO CON LA POSICIÓN ACTUAL
            // Calcular la heurística
            int heuristica = CalcularHeuristica(currentPos, goals[0]);
            // Calcular el coste
            // Se calcula si el nodo es meta o no  
            Nodo nodoInicial = new Nodo(heuristica, costeAcumulado, EsMeta(currentPos, goals[0]), currentPos, null);
            // Añadir el nodo a la lista abierta
            listaAbierta.Add(nodoInicial);

            // SE ORDENA LA LISTA
            listaAbierta.Sort(Comparador.CompareNodesByF);

            // SE EXPLORA EL PRIMER NODO DE LA LISTA
            Nodo primerNodo = listaAbierta[0];
            // Una vez expandido, se añade a la lista cerrada, se saca de la lista abierta y se comprueba 
            listaCerrada.Add(primerNodo);
            listaAbierta.Remove(primerNodo);

            while(!primerNodo.esMeta)
            {
                // Función expandir - se obtienen las casillas caminables destino
                CellInfo[] casillasHijas = currentPos.WalkableNeighbours(boardInfo);

                // Se crea un nodo con cada uno de los vecinos
                for (int i = 0; i < casillasHijas.Length; i++)
                {
                    if (casillasHijas[i] == null) continue;
                    heuristica = CalcularHeuristica(casillasHijas[i], goals[0]);
                    Nodo nodoHijo = new Nodo(heuristica, primerNodo.g+1, EsMeta(casillasHijas[i], goals[0]), casillasHijas[i], primerNodo);
                    // Comprobar si el nodo se encuentra en la lista cerrada
                    bool encontrado = false;
                    foreach(Nodo n in listaCerrada)
                    {
                        if(n.columna == nodoHijo.columna && n.fila == nodoHijo.fila)
                        {
                            encontrado = true;
                        }
                    }
                    if (encontrado) continue;
                    // Se comprueba si hay un ciclo
                    nodoActual = nodoHijo;
                    if (EsCiclo(nodoActual))
                    {
                        continue;
                    }

                    listaAbierta.Add(nodoHijo);
                }

                // SE ORDENA LA LISTA DE NUEVO
                listaAbierta.Sort(Comparador.CompareNodesByF);
                primerNodo = listaAbierta[0];
                currentPos = primerNodo.informacionNodo;
                listaCerrada.Add(primerNodo);
                listaAbierta.Remove(primerNodo);

            }

            if(primerNodo.esMeta)
            {
                CalcularCamino(primerNodo);
                return;
            }

        }

        // Se calcula la heurística de una casilla
        public int CalcularHeuristica(CellInfo posicionActual, CellInfo destino)
        {
            int columna = posicionActual.ColumnId;
            int fila = posicionActual.RowId;

            int columnaMeta = destino.ColumnId;
            int filaMeta = destino.RowId;

            // Distancia Manhattan
            return (Mathf.Abs(columnaMeta - columna) + Mathf.Abs(filaMeta - fila));
        }

        // Se calcula si el nodo es meta o no
        public bool EsMeta(CellInfo posicionActual, CellInfo meta)
        {
            if (posicionActual.RowId == meta.RowId && posicionActual.ColumnId == meta.ColumnId)
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        // Función recursiva que calcula si se produce un ciclo
        public bool EsCiclo(Nodo nodo)
        {
            if(nodo.nodoPadre!=null)
            {
                if(nodo.nodoPadre.fila==nodoActual.fila && nodo.nodoPadre.columna==nodoActual.columna)
                {
                    return true;
                }
                EsCiclo(nodo.nodoPadre);
            } 
            return false;
        }

        public void CalcularCamino(Nodo meta)
        {
            Nodo aux = meta;
            while(aux.nodoPadre!=null)
            {
                if(aux.fila == aux.nodoPadre.fila)
                {
                    if(aux.columna>aux.nodoPadre.columna)
                    {
                        camino.Add(Locomotion.MoveDirection.Right);
                    }
                    else
                    {
                        camino.Add(Locomotion.MoveDirection.Left);
                    }
                }
                if (aux.columna == aux.nodoPadre.columna)
                {
                    if(aux.fila<aux.nodoPadre.fila)
                    {
                        camino.Add(Locomotion.MoveDirection.Down);
                    }
                    else
                    {
                        camino.Add(Locomotion.MoveDirection.Up);
                    }
                }
                aux = aux.nodoPadre;
            }
            // Ahora mismo, en el camino tendremos los movimientos ordenados de meta a inicio, por lo que la lista se debe recorrer al revés
            indiceCamino = camino.Count - 1;
        }

    }
    public class Comparador
    {
        // Compara dos nodos de la lista, por su f
        public static int CompareNodesByF(Nodo a, Nodo b)
        {
            if(a.f < b.f)
            {
                return -1;
            }
            if(a.f == b.f)
            {
                if(a.h < b.h)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }
    }
}
