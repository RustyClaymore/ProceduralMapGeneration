using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree {

    public int leavesCount = 0;
    public List<Leaf> leaves = new List<Leaf>();

	public Tree(int leavesCount)
    {
        this.leavesCount = leavesCount;

        for (int i = 0; i < leavesCount; i++)
        {
            int randX = Random.Range(-450, 450);
            int y = 150;
            int randZ = Random.Range(-450, 450);

            leaves.Add(new Leaf(randX, y, randZ));
        }
    }
}
