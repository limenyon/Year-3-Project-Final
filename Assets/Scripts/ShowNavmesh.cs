using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
public class ShowNavmesh : MonoBehaviour
{
	public bool showNavToggle = true;
	public Material myMaterial;
	public Material triangleMat;
	public List<TriangleData> triangleList = new List<TriangleData>();
	public AdjacencyMatrix adjacencyMatrix;
	public NavMeshTriangulation meshData;
	public List<int> indices;
	public List<Vector3> vertices;
	public List<GameObject> gameObjects;
	public int timer = 0;
	public GameObject thisGO;

	void Start()
	{
		gameObjects = new List<GameObject>();
		meshData = NavMesh.CalculateTriangulation();
		indices = new List<int>();
		vertices = new List<Vector3>();
		FixUnitysTriangles();
		CreateTriangles();
		CreateMeshes();
		CreateLines();
		adjacencyMatrix = CreateAdjacencyMatrix();
	}
    private void Update()
    {
		if(showNavToggle == true)
        {
			if (Input.GetMouseButtonDown(0))
			{
				DeleteMeshes();
				gameObjects = new List<GameObject>();
				//gets the navmesh triangle data
				meshData = NavMesh.CalculateTriangulation();
				indices = new List<int>();
				vertices = new List<Vector3>();
				FixUnitysTriangles();
				CreateTriangles();
				CreateMeshes();
				CreateLines();
				adjacencyMatrix = CreateAdjacencyMatrix();
			}
        }
        else
        {
			if (Input.GetMouseButtonDown(0))
            {
				//Gets the navmesh triangle data
				meshData = NavMesh.CalculateTriangulation();
				indices = new List<int>();
				vertices = new List<Vector3>();
				FixUnitysTriangles();
				adjacencyMatrix = CreateAdjacencyMatrix();
            }
		}
	}
	//A function that restores the connected nature of a regular navmesh
    void FixUnitysTriangles()
	{
		int i = 0;
		//Get every single node from the indices array and add it to a list to process easier
		foreach (int index in meshData.indices)
		{
			indices.Add(meshData.indices[i]);
			i++;
		}
		i = 0;
		//Get every single node from the vertices array and add it to a list to process easier
		foreach (Vector3 vertex in meshData.vertices)
		{
			vertices.Add(meshData.vertices[i]);
			i++;
		}
		//create a list of vertices that are duplicates
		List<int> verticesToRemove = new List<int>();
		//the indices array needs to point at the non-duplicate version of the duplicate node
		List<int> numbersToRemoveWith = new List<int>();
		//do this to every vertex
		for (int j = 0; j < vertices.Count; j++)
		{
			//a bit of optimisation, only look up until the current vertex that is being checked for duplication
			//since we do not care if this vertex is a duplicate of something after it, stop when we reach this vertex
			for (int k = 0; k < j; k++)
			{
				//See if there is any distance between the nodes, Mathf.Epsilon is used to negate float innacuracy
				if (Vector3.Distance(vertices[j], vertices[k]) < Mathf.Epsilon)
				{
					//we add the vertices to a new array to remove later as doing it now would disrupt the entire list
					verticesToRemove.Add(j);
					//add the replacement for the duplicate node
					numbersToRemoveWith.Add(k);
					//breaking out because if the node is a duplicate there is no reason to keep checking further
					break;
				}
			}
		}
		//simply replace indices with ones we set to be their replacements
		//because we want the indices to also point at the right vertices
		for (int x = 0; x < verticesToRemove.Count; x++)
		{
			for (int y = 0; y < indices.Count; y++)
			{
				if (indices[y] == verticesToRemove[x])
				{
					indices[y] = numbersToRemoveWith[x];
				}
			}
		}
		//finally if the vertex has been replaced we simply replace all points with max value
		//to never use it by accident
		//removing nodes would destroy the integrity of the indices array and be counterintuitive
		for (int z = verticesToRemove.Count; z > 0; z--)
		{
			Vector3 temp = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
			vertices[verticesToRemove[z - 1]] = temp;
		}
	}
	void CreateTriangles()
	{
		triangleList.Clear();
		for (int i = 0; i < meshData.indices.Length; i += 3)
		{
			//apply new material
			Material tempMaterial = new Material(triangleMat);
			//Set the vertices of the triangle by getting the position of indices[i] from vertices
			TriangleData triangle = new TriangleData(vertices[indices[i]], vertices[indices[i + 1]], vertices[indices[i + 2]], tempMaterial);
			//add triangle to list for management
			triangleList.Add(triangle);
		}
	}
	void CreateLines()
	{
		for(int i = 0; i < triangleList.Count; i++)
		{
			GameObject triangleObject = gameObjects[i];
			triangleObject.AddComponent<LineRenderer>();
			triangleObject.GetComponent<LineRenderer>().positionCount = 3;
			triangleObject.GetComponent<LineRenderer>().SetPositions(triangleList[i].GetVertices());
			triangleObject.GetComponent<LineRenderer>().endWidth = 0.05f;
			triangleObject.GetComponent<LineRenderer>().startWidth = 0.05f;
			triangleObject.GetComponent<LineRenderer>().loop = true;
			triangleObject.GetComponent<LineRenderer>().material = myMaterial;
		}
	}
	void CreateMeshes()
	{
		foreach (TriangleData triangle in triangleList)
		{
			GameObject triangleObject = new GameObject();
			triangleObject.AddComponent<MeshRenderer>();
			triangleObject.AddComponent<MeshFilter>();
			Mesh triangleMesh = new Mesh();
			triangleObject.GetComponent<MeshFilter>().mesh = triangleMesh;
			triangleMesh.Clear();
			triangleMesh.vertices = triangle.GetVertices();
			triangleMesh.triangles = triangle.GetIndices();
			triangleObject.GetComponent<MeshRenderer>().material = triangle.GetMaterial();
			triangleObject.transform.parent = transform;
			gameObjects.Add(triangleObject);
		}
	}

	private void DeleteMeshes()
	{
		triangleList.Clear();
		for (int i = gameObjects.Count; i > 0; i--)
		{
			Destroy(gameObjects[i - 1]);
			gameObjects.Remove(gameObjects[i - 1]);
		}
	}
	//used to create the entire adjacency matrix
	private AdjacencyMatrix CreateAdjacencyMatrix()
	{
		//create new list of data structure Adjacency Matrix
		AdjacencyMatrix adjacencyMatrix = new AdjacencyMatrix(meshData.vertices.Length);
		//since we take 3 vectors to create each triangle we move 3 spaces with each triangle made
		for (int i = 0; i < meshData.indices.Length; i += 3)
		{
			//grab three vertices used for a triangle
			int index1 = indices[i];
			int index2 = indices[i + 1];
			int index3 = indices[i + 2];
			//place them inside of vector3s for the purpose of finding the distance between them
			Vector3 vertex1 = vertices[index1];
			Vector3 vertex2 = vertices[index2];
			Vector3 vertex3 = vertices[index3];
			//Use unity's native distance between vectors finding function
			float distance1 = Vector3.Distance(vertex1, vertex2);
			float distance2 = Vector3.Distance(vertex2, vertex3);
			float distance3 = Vector3.Distance(vertex1, vertex3);
			//add the distance to the adjacency matrix at the position [vertex1, vertex2]
			//as there are three connections in a triangle this has to be done three times for each triangle
			adjacencyMatrix.SetAdjacency(index1, index2, distance1);
			adjacencyMatrix.SetAdjacency(index2, index3, distance2);
			adjacencyMatrix.SetAdjacency(index1, index3, distance3);
		}
		//return the matrix
		return adjacencyMatrix;
	}
	public void Toggle(bool toggle)
    {
		showNavToggle = !showNavToggle;
		thisGO.SetActive(showNavToggle);
    }
}