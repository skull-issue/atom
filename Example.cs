

using sr;

public class Peep
{
	public string fistname;
	public string lastname;
	public float3 position;
	public decimal dollars;
	public List<string> directions;
	public List<string> hairContents;


	public static implicit operator Peep( Atom atom )
	{
		var peep = new Peep {
			fistname = atom.values![0],
			lastname = atom.values![1]
		};

		foreach( var c in atom.children! )
		{
			switch( c.values![0] )
			{
				case "position":
					peep.position = (float3)c;
					break;

				case "dollars":
					peep.dollars = decimal.Parse( c.values![1] );
					break;

				case "directions":
					peep.directions = new();
					for( int i = 1; i < c.values.Count; i++ )
						peep.directions.Add( c.values![i] );
					break;

				case "hair contents":
					peep.hairContents = new();
					foreach( var cc in c.children! )
						peep.hairContents.Add( cc.values![0]);
					break;
			}
		}

		return peep;
	}
}


public struct float3
{
	public float x;
	public float y;
	public float z;


	public static implicit operator float3( Atom atom )
		=> new float3() {
			x = float.Parse( atom.values![1] ),
			y = float.Parse( atom.values![2] ),
			z = float.Parse( atom.values![3] )
		};
}