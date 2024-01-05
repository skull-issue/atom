Console.WriteLine( sr.Atom.Parse( File.ReadAllText( "test.dat" ) ).Serialize() );

var peeps = sr.Atom.Parse( File.ReadAllText( "example.dat" ) ).children.First( c => c.values[0] == "peeps" ).children.Select( c => (Peep)c ).ToArray();
