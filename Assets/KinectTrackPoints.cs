using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

/**
 * Provides a wrapper around the kinect layers, and also another test layer.
 * 
 * @author John O'Meara
**/
public class KinectTrackPoints : MonoBehaviour
{
	public enum NodeType
	{
		LeftHand = 0,//1,//0,
		//Type_First = LeftHand,
		RightHand,//=32,//1,
		Head,//=2,
		//Type_Last = Head
	}
	
	public bool bForceZToZero = true;
	public bool bInvertXAxis = false;
	public bool Debug_EnableStopOnStaleTelemetry = true;
	public float stale_telemetryWait = 1.0f;
	
	
	private static Dictionary <NodeType, Vector3> nodes = new Dictionary<NodeType,Vector3>();
	
	public static void activateKinectMode()
	{
		if (singleton != null)
		{
			singleton._activateKinect();
		}
		else
		{
			Debug.Log("activateKinectMode: null singleton -- did you add the KinectTrackPoints script to a gameentity in the scene?");
		}
	}
	
	public static bool activateRecorder(String filename, bool bOverwrite)
	{
		if (singleton == null)
		{
			Debug.Log("activateRecorder: null singleton -- did you add the KinectTrackPoints script to a gameentity in the scene?");
			return false;
		}
		return singleton._record(filename, bOverwrite);
	}
	
	public static bool activatePlayback(String filename)
	{
		if (singleton == null)
		{
			Debug.Log("activatePlayback: null singleton -- did you add the KinectTrackPoints script to a gameentity in the scene?");
			return false;
		}
		return singleton._playback(filename);
	}
	
	public static void setNode_Live(NodeType key, Vector3 value)
	{
		//should be used by custom tracker implementations.
		if ((singleton == null)||(singleton.mode != RunMode.ModePlayback))
		{
			setNode(key, value);
			if (singleton)
			{
				singleton.onLiveTelemetry();
			}
		}
	}
	public static void setNode(NodeType key, Vector3 value)
	{
		if (singleton != null)
		{
			if (singleton.bForceZToZero)
			{
				value = new Vector3(value.x, value.y, 0);//enforce z = 0
			}
			if (singleton.bInvertXAxis)
			{
				//value = new Vector3(1 - value.x, value.y, 0);
				value = new Vector3(-value.x, value.y, 0);
			}
		}
		nodes[key] = value;
	}
	
	public static Vector3 getNode(NodeType key)
	{
		if (nodes.ContainsKey(key))
		{
			return nodes[key];
		}
		return new Vector3(0, 0, 0);
	}
	
	///////////////////////////////////////////////////////////////////////////////////////////
	/*
	public static void setLeftHand(Vector3 newLeftHand)
	{
		setNode (NodeType.LeftHand, newLeftHand);
	}
	
	public static void setRightHand(Vector3 newRightHand)
	{
		setNode (NodeType.RightHand, newRightHand);
	}
	
	public static void setHead(Vector3 newHead)
	{
		setNode (NodeType.Head, newHead);
	}
	
	public static Vector3 getLeftHand()
	{
		return getNode(NodeType.LeftHand);
	}
	
	public static Vector3 getRightHand()
	{
		return getNode(NodeType.RightHand);
	}
	
	public static Vector3 getHead()
	{
		return getNode(NodeType.Head);
	}
	*/
	
	////////////////////////////////////////////////////////////
	
	private static Vector3 readVector3(BinaryReader istream)
	{
		float x, y, z;
		x = istream.ReadSingle();
		y = istream.ReadSingle();
		z = istream.ReadSingle();
		return new Vector3(x, y, z);
	}
	
	private static void writeVector3(BinaryWriter ostream, Vector3 value)
	{
		float x, y, z; //ensure we use the right ostream.write overload
		x = value.x;
		y = value.y;
		z = value.z;
		ostream.Write(x);
		ostream.Write(y);
		ostream.Write(z);
	}
	
	struct SampleEntry
	{
		public float time;
		public Dictionary<NodeType, Vector3> map;
	};
	
	//private static Dictionary<NodeType, Vector3> readSample(StreamReader _istream)
	//private static Dictionary<NodeType, Vector3> readSample(BinaryReader istream)
	//private static Dictionary<NodeType, Vector3> readSample(BinaryReader istream)
	private static Dictionary<NodeType, Vector3> readNodeTypeVector3Dictionary(BinaryReader istream)
	{
		Dictionary<NodeType, Vector3> ret = null;
		//if (istream.BaseStream.Position < istream.BaseStream.Length)//check for EOF
		{
			ret = new Dictionary<NodeType, Vector3>();
			//BinaryReader istream = new BinaryReader(_istream.BaseStream);//DERPY HACK
			int numKeys = istream.ReadInt32();
			//String[] keys = new String[numKeys];
			//NodeType[] keys = new NodeType[numKeys];
			//Vector3[] values = new 
			int key;
			Vector3 value;
			for (int i = 0; i < numKeys; i++)
			{
				key = istream.ReadInt32();
				value = readVector3(istream);
				
				//Debug.Log("Read key/value pair:"+key+" ==> "+value);
				//TODO: perhaps double-check that key is a valid nodetype...?
				ret[(NodeType)key] = value;
			}
		}
		
		return ret;
	}
	
	//private static void writeSample(StreamWriter _ostream, Dictionary<NodeType, Vector3> map)
	//private static void writeSample(BinaryWriter _ostream, Dictionary<NodeType, Vector3> map)
	//private static void writeNodeTypeVector3Dictionary(BinaryWriter _ostream, Dictionary<NodeType, Vector3> map)
	private static void writeNodeTypeVector3Dictionary(BinaryWriter ostream, Dictionary<NodeType, Vector3> map)
	{
		if (map == null) //bad sample.
		{
			Debug.Log("writeNodeTypeVector3Dictionary: Tried to write a null sample!");
			return;
		}
		map = new Dictionary<NodeType, Vector3>(map); //HACK -- make a defensive copy
		
		int numKeys;
		int key;
		Vector3 value;
		
		numKeys = map.Count;
		//_ostream.Write(numKeys);
		ostream.Write(numKeys);
		//Debug.Log("numKeys=" + numKeys);
		
		//Debug.Log("map = "+map);
		/*if (numKeys > 0)
		{
			Debug.Log("left=" + map[NodeType.LeftHand]);
			Debug.Log("right=" + map[NodeType.RightHand]);
			Debug.Log("head=" + map[NodeType.Head]);
		}
		*/
		//Dictionary<NodeType, Vector3>.KeyCollection.Enumerator it = map.Keys.GetEnumerator();
		//for (int i = 0; i < numKeys; i++)
		Dictionary<NodeType, Vector3>.KeyCollection.Enumerator it = map.Keys.GetEnumerator();
		//for (var rawKey in map.Keys)
		//it.MoveNext(); //start iterator?
		for (int i = 0; i < numKeys; i++)
		{
			it.MoveNext(); //start iterator?
			
			NodeType rawKey = it.Current;
			key = (int)rawKey;
			/*
			bool bMovedOkay;
			bMovedOkay = it.MoveNext();
			if (!bMovedOkay)
			{
				Debug.Log("didn't move okay!");
			}
			*/
			//Debug.Log("rawKey=" + rawKey);
			value = map[rawKey];
			//Debug.Log("Write key/value pair:" + key + " ==> " + value);
			//Debug.Log("Write key/value pair:" + rawKey + " ==> " + value);
			//Debug.Break();//HACK
			//Debug.DebugBreak();
			
			//_ostream.Write(key);
			ostream.Write(key);
			writeVector3(ostream, value);
		}
		//it.Dispose();
		/*
		bool bMovedOkay2 = it.MoveNext();
		if (bMovedOkay2)
		{
			Debug.Log("had too many?"+it.Current);
		}
		*/
	}
	
	
	//private static SampleEntry readSample(BinaryStream istream)
	private static SampleEntry readSample(BinaryReader istream)
	{
		//Debug.Log("Read Sample");
		SampleEntry ret;
		ret.map = null;
		ret.time = 0.0f;
		if (istream.BaseStream.Position < istream.BaseStream.Length)//check for EOF
		{
			ret.time = istream.ReadSingle();
			ret.map = readNodeTypeVector3Dictionary(istream);
		}
		/*
		//Debug:
		if (ret.map != null)
		{
			Debug.Log("reading sample for time: " + ret.time);
		}
		//end Debug
		*/
		return ret;
	}
	
	private static void writeSample(BinaryWriter ostream, SampleEntry entry)
	{
		//Debug.Log("writing sample for time: " + entry.time);
		ostream.Write(entry.time);
		writeNodeTypeVector3Dictionary(ostream, entry.map);
		//Debug.Log("end write sample.");
	}
	
	////////////////////////////////////////////////////////////
	private static KinectTrackPoints singleton = null; //not publicly visible, but we'll possibly/probably need to play nicely with it to make record/play work...
	
	//private enum RunMode
	public enum RunMode
	{
		ModeNormal = 0, //Live Kinect data
		ModeRecord, //Recording data to a playback file
		ModePlayback, //Using playback data from a file
	};
	
	//private bool bRecording = false;
	//private StreamWriter recorder = null;
	//private StreamReader playback = null;
	
	private RunMode mode = RunMode.ModeNormal;
	private FileStream file = null;
	private BinaryWriter recorder = null;
	private BinaryReader playback = null;
	private float timeElapsed = 0.0f;
	private float lastLiveTelemetry = 0.0f;
	
	private SampleEntry prev;
	private SampleEntry next;
	
	void Start()
	{
//		Debug.Log("Kinect Track Points :: Start");
		if (singleton != null)
		{
			Debug.Log("KinectTrackPoints::start(): singleton was not null!");
		}
		singleton = this;
		/////////////////
		prev.map = null;
		next.map = null;
	}
	
	static float calcU(float a, float b, float t)
	{
		float uStart = a;//.time;
		float uEnd = b;//.time;
		float uRange = uEnd - uStart;
		//if (uRange <//
		//TODO: ensure minimum difference between ranges...?
		if (uRange < 0.001f)
		{
			uRange = 0.001f; //HACK
		}
		
		float u = (t - uStart) / uRange;
		u = Mathf.Clamp(u, 0.0f, 1.0f);
		return u;
	}
	
	SampleEntry lerpEntry(SampleEntry a, SampleEntry b, float t)
	{
		float u = calcU(a.time, b.time, t);
		SampleEntry ret;
		ret.time = Mathf.Lerp(a.time, b.time, u);
		ret.map = new Dictionary<NodeType, Vector3>();
		
		if (a.map.Count > 0)//check map not empty
		{
			Dictionary<NodeType, Vector3>.KeyCollection.Enumerator itA = a.map.Keys.GetEnumerator();
			//Dictionary<NodeType, Vector3>.KeyCollection.Enumerator itB = b.map.Keys.GetEnumerator();
			//Assertion: a,b contain the same keys!
			while (true)
			{
				NodeType keyA, keyB;
				keyA = itA.Current;
				keyB = keyA;//itB.Current; //HACK
				
				//Debug.Log("keyA=" + keyA);
				Vector3 valueA, valueB;
				valueA = a.map[keyA];
				valueB = b.map[keyB];
				
				ret.map[keyA] = Vector3.Lerp(valueA, valueB, u);
				
				if (!itA.MoveNext())
				{
					break;
				}
				/*if (!itB.MoveNext())
				{
					break;
				}*/
			}
		}
		
		return ret;
	}
	
	SampleEntry readNextSample(float thresholdTime)
	{//will return a sample with a null map if we ran out of samples.
		SampleEntry read;
		read.time = 0.0f;
		read.map = null;
		//while (read.map != null)
		while (read.map == null)
		{
			read = readSample(this.playback);
			if (read.map == null) //no more samples -- EOF
			{
				break;
			}
			
			if (thresholdTime > read.time)
			{
				read.map = null; //bad entry, currently in the past.
			}
			
		}
		return read;
	}
	
	void OnDestroy()
	{
	//	Debug.Log("kinect track points closed");
		closeFiles();
	}
	//needed to record data, etc.
	void LateUpdate()
	{
		Debug_Stuff();
		
		if (singleton != this)
		{
			Debug.Log("KinectTrackPoints::Update(): singleton is not this, dying...");
			Destroy(this);//kill us
			return; //abort
		}
		
		float dt = Time.deltaTime;
		//timeElapsed += dt;//update time. 
		
		switch (mode)
		{
		case RunMode.ModeNormal:
		{
			timeElapsed += dt;//update time. 
			//Do nothing; Nodes are updated by other client scripts. 
		}
			break;
			
		case RunMode.ModeRecord:
		{
			//record current sample.
			//timeElapsed += dt;
			if ((nodes != null)&&(nodes.Count > 0)) //don't write empty samples
			{
				bool bNoGo = false;
				if (Debug_EnableStopOnStaleTelemetry)
				{
					if (timeElapsed > lastLiveTelemetry + stale_telemetryWait)
					{
						//stale
						bNoGo = true;
					}
				}
				
				if (!bNoGo)
				{
					timeElapsed += dt;
					
					SampleEntry currentSample;
					currentSample.map = new Dictionary<NodeType, Vector3>(nodes);
					currentSample.time = timeElapsed;
					
					writeSample(this.recorder, currentSample);
				}
			}
		}
			break;
			
		case RunMode.ModePlayback:
		{
			timeElapsed += dt;//update time. 
			//check for stale next
			if (timeElapsed >= next.time)
			{
				next.map = null;
			}
			
			//check for stale prev
			if (timeElapsed >= prev.time)
			{
				prev = next;
				next.map = null;
			}
			
			if (prev.map == null)
			{
				//move next back
				prev = next;
				next.map = null;
				
				//if prev still null, read an entry:
				if (prev.map == null)
				{
					prev = readNextSample(timeElapsed);
				}
			}
			
			if (next.map == null)
			{
				next = readNextSample(timeElapsed);
			}
			
			if ((prev.map == null) || (next.map == null))
			{
				onPlaybackEOF();
				break;//done for this frame at least...
			}
			
			SampleEntry currentSample;
			currentSample = lerpEntry(prev, next, timeElapsed);
			if (currentSample.map != null)
			{
				nodes = new Dictionary<NodeType, Vector3>(currentSample.map);
			}
			else
			{
				Debug.Log("WARNING: currentSample.map was null!");
			}
		}
			break;
			
		default:
		{
			timeElapsed += dt;//update time. 
			//warn about unhandled case, but act like normal mode:
			Debug.Log("KinectTrackPoints: unhandled runMode: " + mode);
		}
			break;
		};//end switch
	}
	
	
	void onPlaybackEOF()
	{
		//called when playback reaches the EOF.
		//TODO: loop playback, probably...?
		Debug.Log("playback complete!");
		//reopen, and reset timeElapsed to zero...??
	}
	
	private void closeFiles()
	{//really more like destroy context
		//try to close the file/etc., nicely
		try
		{
			if (file != null)
			{
				file.Flush();
				file.Close();
				file = null;
			}
		}
		catch (IOException e)
		{
			Debug.Log("Error closing/flushiung file: " + e);
		}
		
		this.file = null;
		this.recorder = null;
		this.playback = null;
		this.mode = RunMode.ModeNormal; //not in another mode!
		
		//paranoia:
		this.timeElapsed = 0.0f; //zero timer!
		this.lastLiveTelemetry = -stale_telemetryWait - 1.0f;
		this.prev.map = null;
		this.next.map = null;
	}
	
	public void _activateKinect()
	{
		Debug.Log("_activateKinect");
		this.mode = RunMode.ModeNormal;
		this.timeElapsed = 0.0f; //zero timer!
		this.lastLiveTelemetry = -stale_telemetryWait - 1.0f;
		closeFiles();
	}
	
	public bool _record(String filename, bool bOverwrite)
	{
		Debug.Log("_record");
		closeFiles();
		this.timeElapsed = 0.0f; //zero timer!
		this.lastLiveTelemetry = -stale_telemetryWait - 1.0f;
		this.mode = RunMode.ModeRecord;
		try
		{
			FileMode mode = bOverwrite ? FileMode.Create : FileMode.CreateNew;
			this.file = File.Open(filename, mode);
			this.recorder = new BinaryWriter(this.file);
		}
		catch(IOException e)
		{
			Debug.Log("_record: Error opening file for recording: " + e);
			this.mode = RunMode.ModeNormal; //failback
		}
		
		if ((file == null) || (recorder == null))
		{
			this.mode = RunMode.ModeNormal; //failed
		}
		
		return (this.mode == RunMode.ModeRecord);
	}
	
	public bool _playback(String filename)
	{
	//	Debug.Log("_playback");
		closeFiles();
		this.timeElapsed = 0.0f; //zero timer!
		this.lastLiveTelemetry = -stale_telemetryWait - 1.0f;
		this.mode = RunMode.ModePlayback;
		prev.map = null;
		next.map = null;
		try
		{
			FileMode mode = FileMode.Open;
			this.file = File.Open(filename, mode);
			this.playback = new BinaryReader(this.file);
		}
		catch (IOException e)
		{
			Debug.Log("_record: Error opening file for recording: " + e);
			this.mode = RunMode.ModeNormal; //failback
		}
		
		if ((file == null) || (playback == null))
		{
			//Debug.Log("playback or file are null!");
			if (file == null)
			{
				Debug.Log("_playback: null file from File.Open!");
			}
			if (playback == null)
			{
				Debug.Log("_playback: null playback object!");
			}
			this.mode = RunMode.ModeNormal; //failed
		}
		
		//FAIL://return (this.mode == RunMode.ModeRecord);
		return (this.mode == RunMode.ModePlayback);
	}
	
	//A quick hack to allow interaction with playback/record for the short term:
	
	public String Debug_Filename = "test.kinectTracker";
	public RunMode Debug_RunMode = RunMode.ModeNormal;
	public bool Debug_bOverwriteFile = true;
	
	//handle stuff from update for quick access to record/playback modes...
	private RunMode Debug_oldRunMode = RunMode.ModeNormal;
	private void Debug_Stuff()
	{
		if (Debug_oldRunMode != Debug_RunMode)
		{
			Debug_oldRunMode = Debug_RunMode;
			
			switch (Debug_RunMode)
			{
			case RunMode.ModeNormal:
				activateKinectMode();
				break;
			case RunMode.ModeRecord:
			{
				bool bOkay;
				bOkay = activateRecorder(Debug_Filename, Debug_bOverwriteFile);
				if (!bOkay)
				{
					Debug.Log("activate record FAILED!");
				}
			}
				break;
			case RunMode.ModePlayback:
			{
				bool bOkay;
				bOkay = activatePlayback(Debug_Filename);
				if (!bOkay)
				{
					Debug.Log("activate playback FAILED!");
				}
			}
				break;
			default:
				activateKinectMode();
				Debug.Log("unhandled debug_runmode mode: " + Debug_RunMode);
				break;
			};//end switch
		}
	}
	
	void onLiveTelemetry()
	{
		lastLiveTelemetry = timeElapsed;
	}
}

