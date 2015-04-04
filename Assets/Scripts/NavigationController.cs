using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NavigationController : MonoBehaviour {
	public SelectionController selection;

	private bool pathForSubject = false;
	private List<Vector3> path = null;
	private UnitController subject;

	enum NodeStates {inactive, open, closed, start, end};

	void Start () {}

	void Update () {}

	public void registerClick(Vector3 click){
		// Move using A*.
		subject = selection.getSelected();
		if(subject != null){
			subject.setPath(getPath (subject.transform.position, click));
		}
	}

	public bool pathIsInvalid(List<Vector3> returnPath){
		return returnPath != null && returnPath.Count == 2 && returnPath[0] == Vector3.zero && returnPath[1] == Vector3.zero;
	}
	
	private bool isObstructed(Node start, Node end){
		float subjectRadius = 3f;
		float distance = Vector3.Distance (start.location,end.location);
		// We can't just draw a line from center to center because we could cut a corner into a wall.
		// Take the subject size into account and draw a line from top/bottom/left/right edge.
		Vector3[] corners = new Vector3[]{start.location,start.location,start.location,start.location,end.location,end.location,end.location,end.location};
		for(int i=0;i<8;i+=4){
			corners[i].x -= subjectRadius;
			corners[i+1].x += subjectRadius;
			corners[i+2].z -= subjectRadius;
			corners[i+3].z += subjectRadius;
		}
		for(int i=0;i<4;i++){
			if(Physics.Raycast (corners[i], corners[i+4]-corners[i], distance, 1 << LayerMask.NameToLayer ("Default"))){
				//Debug.DrawLine(corners[i], corners[i+4], Color.red);
				return true;
			}
		}
		return false;
	}
	
	private float getScore(Node start, Node node, Node end){
		return Vector3.Distance (start.location,node.location) + Vector3.Distance (node.location,end.location);
	}

	public List<Vector3> quickScanPath(Vector3 start, Vector3 end){
		Node startNode = new Node(start, int.MinValue, NodeStates.start);
		Node endNode = new Node(end, int.MaxValue, NodeStates.end);
		
		// If it's a trivial distance, just return null.
		if (Vector3.Distance (start,end) < 1.5f){
			return null;
		}
		// If we can get there without hitting a wall, just go there!
		
		if (!isObstructed(startNode,endNode)){
			//Debug.DrawLine(start,end, Color.green);
			return new List<Vector3> {start, end};
		} else {
			//Debug.DrawLine(start,end, Color.red);
			// Encoding for solution not found.
			return new List<Vector3> {Vector3.zero, Vector3.zero};
		}
	}

	public List<Vector3> getPath(Vector3 start, Vector3 end){
		Node startNode = new Node(start, int.MinValue, NodeStates.start);
		Node endNode = new Node(end, int.MaxValue, NodeStates.end);

		List<Vector3> returnPath = quickScanPath(start, end);
		if(!pathIsInvalid(returnPath)){
			return returnPath;
		}
		
		// Time to actually run A*... Start by creating a digital representation of every nav_node.
		GameObject[] navNodes = GameObject.FindGameObjectsWithTag("nav_node");
		List<Node> nodes = new List<Node>();
		for(int i=0; i<navNodes.Length; i++)
		{
			Node currNode = new Node(navNodes[i].transform.position, i);
			nodes.Add(currNode);
		}
		
		// Initialize all of our virtual nodes.
		for(int i=0;i<nodes.Count;i++){
			for(int j=i+1; j<nodes.Count; j++){
				// Check if the path between the nodes is obstructed. If not, add it as a connection!
				if(!isObstructed(nodes[i],nodes[j])){
					nodes[i].addConnection (nodes[j]);
					nodes[j].addConnection (nodes[i]);
					//Debug.DrawLine (nodes[i].location,nodes[j].location,Color.magenta);
				}
			}
			
			// Remove useless nodes.
			if(nodes[i].connections.Count == 0){
				nodes.Remove (nodes[i--]);
				continue;
			}
			// Check if the node connects to the end target. If so, add it as a connection!
			
			if(!isObstructed(nodes[i],endNode)){
				nodes[i].addConnection (endNode);
				//Debug.DrawLine (nodes[i].location,endNode.location,Color.green);
			}
			
			// Check which nodes connect to the starting point.
			if(!isObstructed(startNode,nodes[i])){
				nodes[i].previous = startNode;
				nodes[i].state = NodeStates.open;
				nodes[i].score = getScore(startNode,nodes[i],endNode);
				//Debug.Log (nodes[i].label+" connects to start with score: "+nodes[i].score+". Opening "+nodes[i].label+".");
				//Debug.DrawLine (nodes[i].location,startNode.location,Color.white);
			}
		}
		bool completedSearch = false, foundAPath = false;
		
		while(!completedSearch){
			completedSearch = true; // If we find evidence to the contrary, we will set this back to 0.
			List<Node> foundNodes = new List<Node>();
			// Go through all open nodes and find their connections before closing them.
			for(int i=0;i<nodes.Count;i++){
				if(nodes[i].state == NodeStates.open){
					completedSearch = false;
					//Debug.Log ("Evaluating "+nodes[i].label+".");
					foreach (Node connection in nodes[i].connections){
						if(connection.state == NodeStates.end){
							//Debug.Log (nodes[i].label+" connects to End!");
							foundAPath = true;
							endNode.addConnection (nodes[i]);
						} else if(connection.state == NodeStates.inactive){
							connection.addPossiblePrev(nodes[i]);
							foundNodes.Add(connection);
							connection.score = getScore (startNode,connection,endNode); // Should the first param be nodes[i] or startNode? Come back to this.
							//Debug.Log (nodes[i].label+" connects to "+connection.label+" with score: "+connection.score+". Opening "+connection.label+".");
						}
					}
					nodes[i].state = NodeStates.closed;
					//Debug.Log ("Done evaluating "+nodes[i].label+".");
				}
			}
			for(int i=0;i<foundNodes.Count;i++){
				foundNodes[i].state = NodeStates.open;
				foundNodes[i].getBestPrevious();
			}
		}
		
		if (foundAPath){
			List<Node> shortestRoute = null;
			float lowestScore = int.MaxValue;
			foreach (Node node in endNode.connections){
				//Debug.Log ("Current best path (reversed):");
				//printList (shortestRoute);
				//Debug.Log ("Total score: "+lowestScore);
				// Trace back every path and keep the lowest scoring one.
				float score = 0;
				Node ptr = node;
				List<Node> prospectivePath = new List<Node>{endNode};
				
				while(ptr != startNode){
					score += node.score;
					prospectivePath.Add (ptr);
					ptr = ptr.previous;
				}
				prospectivePath.Add (ptr);
				
				if(score < lowestScore){
					lowestScore = score;
					shortestRoute = prospectivePath;
				}
			}
			// Turn it around so it's start to finish.
			List<Vector3> path = new List<Vector3>();
			//Debug.Log ("Best path found:");
			for(int i=shortestRoute.Count-1;i>=0;i--){
				/**if (shortestRoute[i].label==int.MinValue)
					Debug.Log ("Start");
				else if(shortestRoute[i].label==int.MaxValue)
					Debug.Log ("End");
				else
					Debug.Log(shortestRoute[i].label); **/
				path.Add (shortestRoute[i].location);
			}
			return path;
		} else {
			return null;
		}
	}
		
	private class Node {
		// Declare all of the node fields.
		public int label; //{ get; set; }
		public Node previous; //{ get; set; }
		public List<Node> connections; //{ get; set; }
		public List<Node> possiblePrevious; //{ get; set; }
		public Vector3 location; //{ get; set; }
		public NodeStates state; //{ get; set; }
		public float score; //{ get; set; }
		
		public Node(Vector3 location, int label, NodeStates state = NodeStates.inactive){
			this.location = location;
			this.label = label;
			this.state = state;
			this.connections = new List<Node>();
			this.possiblePrevious = new List<Node>();
		}
		
		public void addConnection(Node other){
			connections.Add (other);
		}
		public void addPossiblePrev(Node other){
			possiblePrevious.Add (other);
		}
		
		public void getBestPrevious(){
			float lowScore = int.MaxValue;
			Node returnNode = null;
			
			foreach(Node node in possiblePrevious){
				if(node.score < lowScore){
					lowScore = node.score;
					returnNode = node;
				}
			}
			
			previous = returnNode;
		}
	}
}

/** These are likely being retired. Keeping the code for now just in case.
if (Input.GetMouseButtonDown (0)) {
	if(Input.GetKey (KeyCode.LeftShift)){
		// Tell the subject to seek this point when shift is held.
		subject.setTarget(getClickPoint());
		path = null;
	} else if(Input.GetKey (KeyCode.LeftControl)){
		Vector3 target = getClickPoint();
		// Demonstrate A* with Debug Lines. Set start position, end position, then find the path and draw it!
		if(startPos == Vector3.zero || endPos != Vector3.zero){
			startPos = target;
			endPos = Vector3.zero;
		} else {
			endPos = target;
			// Run A* for startPos and endPos
			pathForSubject = false;
			path = getPath(startPos, endPos);
			startPos = Vector3.zero;
			endPos = Vector3.zero;
		}
	} else {
		//A* goes here.
	}
}
public Vector3 getClickPoint () {
	Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
	RaycastHit hit;
	if (Physics.Raycast (ray, out hit, int.MaxValue, 1 << LayerMask.NameToLayer ("Water")))
		return hit.point;
	return Vector3.zero;
}	
void drawPath(List<Vector3> path){
	for(int i=1; i<path.Count; i++){
		Debug.DrawLine (path[i-1],path[i],Color.magenta);
	}
}
private void printList(List<Node> list){
	if (list == null){
		Debug.Log("<Empty list>");
	} else {
		foreach (Node node in list){
			if (node.label==int.MinValue)
				Debug.Log ("Start");
			else if(node.label==int.MaxValue)
				Debug.Log ("End");
			else
				Debug.Log(node.label);
		}
	}
}
**/
