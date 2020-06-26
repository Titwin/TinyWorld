using UnityEngine;
using System.Collections;

public class QuadTreeTest : MonoBehaviour
{
	public class TestObject : IQuadTreeObject
	{
		private Rect m_bounds;
		public TestObject(Vector3 position)
		{
			m_bounds = new Rect(position.x,position.z,1,1);
		}

		public Rect bounds { 
			get => m_bounds; 
			set{ m_bounds = value; } }

		public int Layer => 0;

		int IQuadTreeObject.SpatialGroup { get; set; }

	}
	QuadTree<TestObject> quadTree;
	void OnEnable()
	{
		quadTree = new QuadTree<TestObject>(10, new Rect(-1000, -1000, 2000, 2000));
		for (int i = 0; i < 1000; i++)
		{
			TestObject newObject = new TestObject(new Vector3(Random.Range(-900, 900), 0, Random.Range(-900, 900)));
			quadTree.Insert(newObject);
		}
	}
	void OnDrawGizmos2()
	{
		if (quadTree != null)
		{
			quadTree.DrawDebug();
		}
	}
}