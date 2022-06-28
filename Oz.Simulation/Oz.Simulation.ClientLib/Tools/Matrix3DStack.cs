///---------------------------------------------------------------------------
//
// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Limited Permissive License.
// See http://www.microsoft.com/resources/sharedsource/licensingbasics/limitedpermissivelicense.mspx
// All other rights reserved.
//
// This file is part of the 3D Tools for Windows Presentation Foundation
// project.  For more information, see:
// 
// http://CodePlex.com/Wiki/View.aspx?ProjectName=3DTools
//
//---------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace Oz.Simulation.ClientLib.Tools;

/// <summary>
///     Matrix3DStack is a stack of Matrix3Ds.
/// </summary>
public class Matrix3DStack : IEnumerable<Matrix3D>, ICollection
{
    private readonly List<Matrix3D> _storage = new();

    public int Count => _storage.Count;

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator() =>
        ((IEnumerable<Matrix3D>)this).GetEnumerator();

    #endregion

    #region IEnumerable<Matrix3D> Members

    IEnumerator<Matrix3D> IEnumerable<Matrix3D>.GetEnumerator()
    {
        for (var i = _storage.Count - 1; i >= 0; i--)
        {
            yield return _storage[i];
        }
    }

    #endregion

    public Matrix3D Peek() =>
        _storage[_storage.Count - 1];

    public void Push(Matrix3D item) =>
        _storage.Add(item);

    public void Append(Matrix3D item)
    {
        if (Count > 0)
        {
            var top = Peek();
            top.Append(item);
            Push(top);
        }
        else
        {
            Push(item);
        }
    }

    public void Prepend(Matrix3D item)
    {
        if (Count > 0)
        {
            var top = Peek();
            top.Prepend(item);
            Push(top);
        }
        else
        {
            Push(item);
        }
    }

    public Matrix3D Pop()
    {
        var result = Peek();
        _storage.RemoveAt(_storage.Count - 1);

        return result;
    }

    private void Clear() =>
        _storage.Clear();

    private bool Contains(Matrix3D item) =>
        _storage.Contains(item);

    #region ICollection Members

    void ICollection.CopyTo(Array array, int index) =>
        ((ICollection)_storage).CopyTo(array, index);

    bool ICollection.IsSynchronized => ((ICollection)_storage).IsSynchronized;

    object ICollection.SyncRoot => ((ICollection)_storage).SyncRoot;

    #endregion
}