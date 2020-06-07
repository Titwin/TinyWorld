using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Quadtree by Just a Pixel (Danny Goodayle) - http://www.justapixel.co.uk
Copyright (c) 2015
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
//Any object that you insert into the tree must implement this interface
public interface IQuadTreeObject
{
    Rect bounds { get; }
    int Layer { get; }
    int SpatialGroup { get; set; }
}

public class QuadTree<T>
    where T : IQuadTreeObject
{
    public interface Query
    {
        // TODO implement to select specific filters
    }

    private int m_maxObjectCount;
    private List<T> m_storedObjects;
    private Rect m_bounds;
    private QuadTree<T>[] cells;
    private int m_divisions = 2;
    private int maxDepth = 3;
    private int m_depth;

    public QuadTree(int maxSize, Rect bounds, int divisions = 4, int depth = 0)
    {
        m_bounds = bounds;
        m_maxObjectCount = maxSize;
        cells = new QuadTree<T>[divisions*divisions];
        m_storedObjects = new List<T>(maxSize);
        m_divisions = divisions;
        m_depth = depth;
    }

    public void Insert(T objectToInsert)
    {

        if (cells[0] != null)
        {
            int iCell = GetCellToInsertObject(objectToInsert.bounds.center);
            if (iCell > -1)
            {
                cells[iCell].Insert(objectToInsert);
            }
            return;
        }
        m_storedObjects.Add(objectToInsert);
        //Objects exceed the maximum count
        if (m_depth < maxDepth && m_storedObjects.Count > m_maxObjectCount)
        {
            Expand();
        }
    }
    public void Remove(T objectToRemove, Vector2 position)
    {
        if (ContainsLocation(position))
        {
            m_storedObjects.Remove(objectToRemove);
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    //if (cells[i].ContainsLocation(position))
                    // {
                    cells[i].Remove(objectToRemove, position);
                    // }
                }
            }
            if (cells[0] != null && CountChild < m_maxObjectCount / 2)
            {
                Shrink();
            }
        }
    }

    void Expand()
    {
        //Split the quad into (divisions*divisions) sections
        if (cells[0] == null)
        {
            float subWidth = (m_bounds.width / (float)m_divisions);
            float subHeight = (m_bounds.height / (float)m_divisions);
            float x = m_bounds.x;
            float y = m_bounds.y;
            int idx = 0;
            for(int ix = 0; ix < m_divisions;++ix)
            {
                for (int iy = 0; iy < m_divisions;++iy)
                {
                    cells[idx] = new QuadTree<T>(m_maxObjectCount, new Rect(x + subWidth*ix, y + subHeight*iy, subWidth, subHeight), m_divisions, m_depth + 1);
                    ++idx;
                }
            }
        }
        //Reallocate this quads objects into its children
        int i = m_storedObjects.Count - 1;
        while (i >= 0)
        {
            T storedObj = m_storedObjects[i];
            int iCell = GetCellToInsertObject(storedObj.bounds.position);
            if (iCell > -1)
            {
                cells[iCell].Insert(storedObj);
            }
            m_storedObjects.RemoveAt(i);
            i--;
        }
    }
    void Shrink()
    {
        if (cells[0] != null)
        {
            RetrieveAllChild(ref m_storedObjects);
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i] = null;
            }
        }
    }
    public int Count
    {
        get
        {
            int size = m_storedObjects.Count + CountChild;
            return size;
        }
    }
    int CountChild
    {
        get
        {
            int size = 0;
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    size += cells[i].Count;
                }
            }
            return size;
        }
    }

    public void RetrieveAll(ref List<T> container)
    {
        container.AddRange(m_storedObjects);
        RetrieveAllChild(ref m_storedObjects);
    }
    void RetrieveAllChild(ref List<T> container)
    {
        if (cells[0] != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                cells[i].RetrieveAll(ref container);
            }
        }
    }
    public List<T> RetrieveObjectsInArea(Rect area)
    {
        if (rectOverlap(m_bounds, area))
        {
            List<T> returnedObjects = new List<T>();
            for (int i = 0; i < m_storedObjects.Count; i++)
            {
                if (ContainsRect(m_storedObjects[i].bounds))
                {
                    returnedObjects.Add(m_storedObjects[i]);
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    List<T> cellObjects = cells[i].RetrieveObjectsInArea(area);
                    if (cellObjects != null)
                    {
                        returnedObjects.AddRange(cellObjects);
                    }
                }
            }
            return returnedObjects;
        }
        return null;
    }
    public void RetrieveObjectsInAreaNonAloc(Rect area, LayerMask mask, ref T[] returnedObjects, ref int count)
    {
        if(m_depth == 0)
        {
            count = 0;
        }
        if (rectOverlap(m_bounds, area))
        {
            for (int i = 0; i < m_storedObjects.Count && returnedObjects.Length > count; i++)
            {
                if (ContainsRect(m_storedObjects[i].bounds) && LayerMaskExtensions.Contains(mask, m_storedObjects[i].Layer))
                {
                    returnedObjects[count++] = m_storedObjects[i];
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length && returnedObjects.Length > count; i++)
                {
                    cells[i].RetrieveObjectsInAreaNonAloc(area, mask, ref returnedObjects, ref count);
                }
            }
        }
    }
    public void RetrieveObjectsInAreaNonAloc(Rect area, ref T[] returnedObjects, ref int count)
    {
        if (rectOverlap(m_bounds, area))
        {
            for (int i = m_storedObjects.Count - 1; i >= 0 && returnedObjects.Length > count; --i)
            {
                if (m_storedObjects[i] != null)
                {
                    try
                    {
                        if (ContainsRect(m_storedObjects[i].bounds))
                        {
                            returnedObjects[count++] = m_storedObjects[i];
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning("Bad actor indexing, at index " + i + " : " + e);
                    }
                }
                else
                {
                    m_storedObjects.RemoveAt(i);
                }
            }
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length && returnedObjects.Length > count; i++)
                {
                    cells[i].RetrieveObjectsInAreaNonAloc(area, ref returnedObjects, ref count);
                }
            }
        }
    }

    // Clear quadtree
    public void Clear()
    {
        m_storedObjects.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i] != null)
            {
                cells[i].Clear();
                cells[i] = null;
            }
        }
    }
    public bool ContainsLocation(Vector2 location)
    {
        return m_bounds.Contains(location);
    }
    public bool ContainsRect(Rect rect)
    {
        return rectOverlap(m_bounds,rect);
    }
    private int GetCellToInsertObject(Vector2 location)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            if (cells[i].ContainsLocation(location))
            {
                return i;
            }
        }
        return -1;
    }
    public int GetCell(Vector2 location, int current = 0)
    {
        if (ContainsLocation(location))
        {
            if (cells[0] != null)
            {
                for (int i = 0; i < cells.Length; i++)
                {
                    if (cells[i].ContainsLocation(location))
                    {
                        return cells[i].GetCell(location, current * cells.Length + i);
                    }
                }
            }
            else
            {
                return current;
            }
        }
        return -1;
    }
    static bool valueInRange(float value, float min, float max)
    { return (value >= min) && (value <= max); }

    static bool rectOverlap(Rect A, Rect B)
    {
        bool xOverlap = valueInRange(A.x, B.x, B.x + B.width) ||
                        valueInRange(B.x, A.x, A.x + A.width);

        bool yOverlap = valueInRange(A.y, B.y, B.y + B.height) ||
                        valueInRange(B.y, A.y, A.y + A.height);

        return xOverlap && yOverlap;
    }

    void DrawRect(Rect rect)
    {
        Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.x, 0, rect.y + rect.height));
        Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.x + rect.width, 0, rect.y));
        Gizmos.DrawLine(new Vector3(rect.x + rect.width, 0, rect.y), new Vector3(rect.x + rect.width, 0, rect.y + rect.height));
        Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y + rect.height), new Vector3(rect.x + rect.width, 0, rect.y + rect.height));
    }
    public void DrawDebug()
    {
        //UnityEditor.Handles.Label(new Vector3(m_bounds.center.x, 0, m_bounds.center.y), Count.ToString());
       
        if (cells[0] != null)
        {
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] != null)
                {
                    cells[i].DrawDebug();
                }
            }
        }
        else
        {
            foreach (var node in m_storedObjects)
            {
                Gizmos.DrawLine(new Vector3(m_bounds.center.x, 0, m_bounds.center.y), new Vector3(node.bounds.center.x, 0, node.bounds.center.y));
            }
        }
        DrawRect(m_bounds);
    }
}