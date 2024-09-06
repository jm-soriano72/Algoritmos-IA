using Assets.Scripts.DataStructures;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nodo
{
    public int fila;
    public int columna;
    public float g;
    public float h;
    public float f;
    public bool esMeta;
    public Nodo nodoPadre;
    public CellInfo informacionNodo;

    public Nodo(float h_in, float g_in, bool esMeta_in, CellInfo info, Nodo padre)
    {
        this.h = h_in;
        this.g = g_in;
        this.f = h + g;
        this.esMeta = esMeta_in;
        this.informacionNodo = info;
        this.fila = info.RowId;
        this.columna = info.ColumnId;
        this.nodoPadre = padre;
    }
}
