using UnityEngine;
using System.Collections;

class WorldCircle: MonoBehaviour
{
	//private members
	private int _segments;
	private float _xradius;
	private float _yradius;
	private LineRenderer _renderer;

	#region Constructors
	// This one does all the work
	public WorldCircle(ref LineRenderer renderer, int segments, float xradius, float yradius)
	{
		_renderer = renderer;
		_segments = segments;
		_xradius = xradius;
		_yradius = yradius;
		Draw(segments, _xradius, _yradius);
	}

	// these are 'convenience' constructors
	public WorldCircle(ref LineRenderer renderer): this(ref renderer, 256, 5.0f, 5.0f) { }

	public WorldCircle(ref LineRenderer renderer, int segments) : this(ref renderer, segments, 5.0f, 5.0f) { }

	public WorldCircle(ref LineRenderer renderer, int segments, float [] radii) : this(ref renderer, segments, radii[0], radii[1]) { }
	#endregion

	public void Draw(int segments, float[] radii)
	{
		_xradius = radii[0];
		_yradius = radii[1];
		Draw(segments, _xradius, _yradius);
	}

	public void Draw(int segments, float xradius, float yradius)
	{
		_xradius = xradius;
		_yradius = yradius;
		_renderer.SetVertexCount(segments + 1);
		_renderer.useWorldSpace = false;
		CreatePoints();
	}

	public float[] radii
	{
		get {
			float [] values = new float[2];
			values[0] = _xradius;
			values[1] = _yradius;
			return values;
		}
	}

	private void CreatePoints ()
	{
		float x;
		float y;
		float z;
		float angle = 20f;

		for (int i = 0; i < (_segments + 1); i++)
		{
			x = Mathf.Sin (Mathf.Deg2Rad * angle) * _xradius;
			z = Mathf.Cos (Mathf.Deg2Rad * angle) * _yradius;

			_renderer.SetPosition (i,new Vector3(x,0,z) );

			angle += (360f / _segments);
		}
	}
}