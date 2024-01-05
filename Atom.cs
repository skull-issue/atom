using System.Text;
using System.Linq;
using System.Collections.Generic;


namespace sr
{
	// initial suboptimal implementation; there's plenty of room to squeeze more performance out.
	public partial class Atom
	{
		public List<string>? values;
		public List<Atom>? children;

		public string Key => (values?.Count ?? 0) <= 0 ? string.Empty : values![0];


		public static Atom Parse( IEnumerable<char> text )
			=> Parser.Parse( text );


		public override string ToString()
			=> $"[{children?.Count ?? 0}]  {string.Join( ", ", values ?? new() )}";


		public string Serialize()
			=> Parser.Serialize( this );


		static string Indents( int amt )
			=> amt < 0 ? string.Empty : new string( Enumerable.Repeat( '\t', amt ).ToArray() );


		class Parser
		{
			const char LTRL = '◘'; // the string-literal toggle character; type it with Alt-Num8


			public static string Serialize( Atom atom, int depth = -1 )
			{
				var sb = new StringBuilder();

				if( atom.values != null )
					sb.AppendLine( Indents( depth ) + string.Join( " ", Literalize( atom.values ) ) );

				if( atom.children != null )
					foreach( var c in atom.children )
						sb.Append( Serialize( c, depth + 1 ) );

				return sb.ToString();

				IEnumerable<string> Literalize( IEnumerable<string> values )
				{
					foreach( var value in values )
						yield return value.Contains( '\r' ) || value.Contains( '\r' ) || value.Contains( ' ' ) || value.Contains( "//" ) ? $"{LTRL}{value}{LTRL}" : value;
				}
			}


			public static Atom Parse( IEnumerable<char> text )
				=> Atomize( Tokenize( Sanitize( text ) ) );


			static IEnumerable<string> Sanitize( IEnumerable<char> text )
			{
				bool crlf = false;
				bool slash = false;
				bool prevSlash = false;

				bool literal = false;
				bool newline = false;
				bool comment = false;

				var sb = new StringBuilder(4096);

				foreach( char c in text )
				{
					prevSlash = slash;
					slash = !literal && c == '/';
					crlf = c == '\r' || c == '\n';

					if( !literal )
						// start newline
						if( crlf )
						{
							newline = true;
							continue;
						}
						// resolve newline
						else if( newline && !crlf )
						{
							yield return sb.ToString();
							sb.Clear();
							newline = comment = slash = false;
						}
						// detect comment
						else if( slash && prevSlash )
							comment = true;

					// ignore until next line
					if( comment )
						continue;

					// toggle literal
					if( c == LTRL )
						literal = !literal;

					// append character
					if( literal )
						sb.Append( c );
					else if( !slash )
					{
						if( prevSlash )
							sb.Append( '/' );
						sb.Append( c );
					}
				}

				// end of stream
				yield return sb.ToString();
				sb.Clear();
			}


			static IEnumerable<Token> Tokenize( IEnumerable<string> lines )
			{
				foreach( var line in lines )
				{
					var token = Token.Parse( line );
					if( token.values.Count > 0 )
						yield return token;
				}
			}


			static Atom Atomize( IEnumerable<Token> tokens )
			{
				var stack = new Stack<Atom>();
				var prevDepth = -1;

				var root = new Atom();
				var head = root;
				Atom prev = head;

				foreach( var current in tokens )
				{
					var depth = current.depth;

					// if you see tabs on the first line, no you didn't
					if( prevDepth < 0 )
						depth = prevDepth = 0;

					while( depth > prevDepth )
					{
						stack.Push( head );
						head = prev;
						prevDepth++;
					}
					while( depth < prevDepth )
					{
						head = stack.Pop();
						prevDepth--;
					}

					(head.children ?? (head.children = new())).Add( prev = new Atom { values = current.values } );
					prevDepth = depth;
				}

				return root;
			}


			class Token
			{
				public int depth;
				public string text = string.Empty;
				public List<string> values = new();

				// pretty much just so it's legible in studio's inspector
				public override string ToString()
					=> $"{depth}{Indents(depth)}{string.Join( ", ", values )}";

				public static Token Parse( string line )
				{
					int d = 0;
					foreach( var c in line )
						if( c == '\t' )
							d++;
						else
						{
							var raw = line.Substring( d );
							var text = raw.Replace( $"{LTRL}", "" );
							var values = SplitToValues( raw );
							return new Token { depth = d, text = text, values = values };
						}

					return new Token { depth = d };
				}


				static List<string> SplitToValues( string line )
				{
					var sb = new StringBuilder(64);
					var values = new List<string>();
					bool literal = false;

					foreach( var c in line )
					{
						if( c == LTRL )
							literal = !literal;
						else if( !literal && c == ' ' )
							BuildValue();
						else
							sb.Append( c );
					}

					BuildValue();
					return values;


					void BuildValue()
					{
						if( sb.Length <= 0 )
							return;
						values.Add( sb.ToString() );
						sb.Clear();
					}
				}
			}
		}
	}
}
