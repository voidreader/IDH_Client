//
//  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR
//  PURPOSE. IT CAN BE DISTRIBUTED FREE OF CHARGE AS LONG AS THIS HEADER 
//  REMAINS UNCHANGED.
//
//  Email:  gustavo_franco@hotmail.com
//
//  Copyright (C) 2006 Franco, Gustavo 
//
// EDIT 2010 by Christoph Husse: Update() method didn't work correctly. Also
// each item is now carrying an index, so that updating can be performed
// efficiently.
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;


public class PriorityQueue<T>
{
	protected List<T> InnerList = new List<T>();
	protected IComparer<T> mComparer;

	public List<T> GetList
	{
		get { return InnerList; }
	}
		
	public PriorityQueue()
	{
		mComparer = Comparer<T>.Default;
	}
		
	public PriorityQueue(IComparer<T> comparer)
	{
		mComparer = comparer;
	}
		
	protected void SwitchElements(int i, int j)
	{
		T h = InnerList[i];
		InnerList[i] = InnerList[j];
		InnerList[j] = h;
	}
		
	protected virtual int OnCompare(int i, int j)
	{
		return mComparer.Compare(InnerList[i], InnerList[j]);
	}

	/// <summary>
	/// Sort ALL
	/// </summary>
	public void Srot()
	{
		GetList.Sort(mComparer);
	}

	public int Push(T item)
	{
		int child = InnerList.Count, parrent;
		InnerList.Add(item); // E[p] = O
			
		do
		{
			if (child == 0)
				break;
			parrent = (child - 1) / 2;
			if (OnCompare(child, parrent) < 0)
			{
				SwitchElements(child, parrent);
				child = parrent;
			}
			else
				break;
		} while (true);
		return child;
	}


	public int PushRange(T[] _items)
	{
		int child = -1;
		for (int i = 0; i < _items.Length; i++)
			child = Push(_items[i]);
		return child;
	}
		
	/// <summary>
	/// Get the smallest object and remove it.
	/// </summary>
	/// <returns>The smallest object</returns>
	public T Pop()
	{
		if( InnerList.Count == 0 )
			return default(T);

		T result = InnerList[0];
		int p = 0, p1, p2, pn;
			
		InnerList[0] = InnerList[InnerList.Count - 1];
		InnerList.RemoveAt(InnerList.Count - 1);
			
		do
		{
			pn = p;
			p1 = 2 * p + 1;
			p2 = 2 * p + 2;
			if (InnerList.Count > p1 && OnCompare(p, p1) > 0) // links kleiner
				p = p1;
			if (InnerList.Count > p2 && OnCompare(p, p2) > 0) // rechts noch kleiner
				p = p2;
				
			if (p == pn)
				break;
			SwitchElements(p, pn);
		} while (true);
			
		return result;
	}

	/// <summary>
	/// Get the smallest object without removing it.
	/// </summary>
	/// <returns>The smallest object</returns>
	public T Peek()
	{
		if (InnerList.Count > 0)
			return InnerList[0];
		return default(T);
	}
		
	public void Clear()
	{
		InnerList.Clear();
	}
		
	public int Count
	{
		get { return InnerList.Count; }
	}
}
