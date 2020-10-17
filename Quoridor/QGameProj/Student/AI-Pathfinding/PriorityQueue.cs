using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

class PriorityQueue <T> // Min Heap
{
    private class Item
    {
        public T item { get; private set; }
        public float priority { get; private set; }

        public Item(T item, float priority)
        {
            this.item = item;
            this.priority = priority;
        }
    }

    public int Count => priorityQueue.Count;
    public int Parent(int pos) => (pos - 1) / 2;
    public int Left(int pos) => (2 * pos) + 1;
    public int Right(int pos) => (2 * pos) + 2;

    private List<Item> priorityQueue;

    public PriorityQueue()
    {
        priorityQueue = new List<Item>();
    }

    public void Enqueue(T item, float priority)
    {
        priorityQueue.Add(new Item(item, priority)); // Add to end of list
        MoveUp(Count - 1);
    }

    public T Dequeue()
    {
        if (Count <= 0) // No data available
            return default(T);

        Item root = priorityQueue.First();

        priorityQueue[0] = priorityQueue[Count - 1]; // Replace root with element at end
        priorityQueue.RemoveAt(Count - 1);           // Remove element at end

        MoveDown(0); // After root is removed, move it down

        return root.item;
    }

    private void MoveUp(int pos)
    {
        if (pos <= 0) // We have reached root
            return;

        int parent = Parent(pos);      
        if (priorityQueue[pos].priority < priorityQueue[parent].priority)
        {
            Swap(pos, parent);
            MoveUp(parent);
        }
    }

    private void MoveDown(int pos)
    {
        int left = Left(pos);
        int right = Right(pos);

        int smallest = pos;
        if (left < Count && priorityQueue[left].priority < priorityQueue[smallest].priority)
        {
            smallest = left;
        }
        if (right < Count && priorityQueue[right].priority < priorityQueue[smallest].priority)
        {
            smallest = right;
        }

        if (smallest != pos)
        {
            Swap(pos, smallest);
            MoveDown(smallest);
        }
    }

    private void Swap(int first, int second)
    {
        Item tmp = priorityQueue[first];
        priorityQueue[first] = priorityQueue[second];
        priorityQueue[second] = tmp;
    }

    public bool Contains(T item)
    {
        for (int i = 0; i < priorityQueue.Count; i++)
        {
            if (priorityQueue[i].item.Equals(item))
                return true;
        }
        return false;
    }
}
