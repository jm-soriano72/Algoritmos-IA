using Assets.Scripts.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Assets.Scripts.SampleMind
{
    public class OnLineEnemies : AbstractPathMind {

        public List<Nodo> listaAbierta = new List<Nodo>();
        public List<Nodo> listaCerrada = new List<Nodo>();

        private int costeAcumulado = 0;

        public List<Locomotion.MoveDirection> camino = new List<Locomotion.MoveDirection>();
        private int indiceCamino = 0;

        private Nodo nodoActual;
        // Lista de enemigos
        public List<GameObject> objetos = new List<GameObject>();

        public override void Repath()
        {
            
        }

        public override Locomotion.MoveDirection GetNextMove(BoardInfo boardInfo, CellInfo currentPos, CellInfo[] goals)
        {
            int numEnemigos = 0;
            for(int i=0; i<objetos.Count; i++)
            {
                if (objetos[i] != null) numEnemigos++;
            }
            if(numEnemigos > 0)
            {
                return BusquedaEnemigos(boardInfo, currentPos);
            }
            if (camino.Count==0)
            {
                AlgoritmoBusqueda(boardInfo, currentPos, goals);
            }
            Locomotion.MoveDirection nextMove = camino[indiceCamino];
            indiceCamino--;
            return nextMove;

        }

        public void AddEnemies(List<GameObject> enemigos)
        {
            objetos = enemigos;
        }

        public Locomotion.MoveDirection BusquedaEnemigos(BoardInfo boardInfo, CellInfo currentPos)
        {
            int heuristica = 1000;
            int indice = 0;
            for(int i=0; i<objetos.Count; i++)
            {
                if (objetos[i] == null) continue;
                GameObject enemy = objetos[i];
                int aux = CalcularDistanciaEnemigo(currentPos, enemy);

                if(aux<heuristica)
                {
                    heuristica = aux;
                    indice = i;
                }
            }
            Nodo nodoInicial = new Nodo(heuristica, costeAcumulado, EsMeta(currentPos, objetos[indice].GetComponent<EnemyBehaviour>().CurrentPosition()), currentPos, null);
            // Añadir el nodo a la lista abierta
            listaAbierta.Add(nodoInicial);

            // SE ORDENA LA LISTA
            listaAbierta.Sort(Comparador.CompareNodesByF);

            // SE EXPLORA EL PRIMER NODO DE LA LISTA
            Nodo primerNodo = listaAbierta[0];
            // Una vez expandido, se añade a la lista cerrada, se saca de la lista abierta y se comprueba 
            listaAbierta.Remove(primerNodo);

                // Función expandir - se obtienen las casillas caminables destino
                CellInfo[] casillasHijas = currentPos.WalkableNeighbours(boardInfo);

                // Se crea un nodo con cada uno de los vecinos
                for (int i = 0; i < casillasHijas.Length; i++)
                {
                    if (casillasHijas[i] == null) continue;
                    heuristica = CalcularHeuristica(casillasHijas[i], objetos[indice].GetComponent<EnemyBehaviour>().CurrentPosition());
                    Nodo nodoHijo = new Nodo(heuristica, primerNodo.g + 1, EsMeta(casillasHijas[i], objetos[indice].GetComponent<EnemyBehaviour>().CurrentPosition()), casillasHijas[i], primerNodo);

                    listaAbierta.Add(nodoHijo);
                }
                listaAbierta.Sort(Comparador.CompareNodesByF);
                primerNodo = listaAbierta[0];
            listaAbierta.Clear();

            if (primerNodo.fila == nodoInicial.fila)
            {
                if (primerNodo.columna > nodoInicial.columna)
                {
                    return (Locomotion.MoveDirection.Right);
                }
                else
                {
                    return (Locomotion.MoveDirection.Left);
                }
            }
            else
            {
                if (primerNodo.fila < nodoInicial.fila)
                {
                   return (Locomotion.MoveDirection.Down);
                }
                else
                {
                   return (Locomotion.MoveDirection.Up);
                }
            }
        }

        public int CalcularDistanciaEnemigo(CellInfo posicionActual, GameObject enemy)
        {
            int columna = posicionActual.ColumnId;
            int fila = posicionActual.RowId;

            int columnaMeta = (int) enemy.transform.position.x;
            int filaMeta = (int) enemy.transform.position.y;

            return (Mathf.Abs(columnaMeta - columna) + Mathf.Abs(filaMeta - fila));

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
}
