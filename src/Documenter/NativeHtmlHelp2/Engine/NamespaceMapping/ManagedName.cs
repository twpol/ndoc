using System;

namespace NDoc.Documenter.NativeHtmlHelp2.Engine.NamespaceMapping
{
	internal class ManagedName
	{
		string _ns;

		public ManagedName( string ns )
		{
			// strip off any NDoc type prefix
			int colonPos = ns.IndexOf( ':' );
			if ( colonPos > -1 )
				_ns = ns.Substring( colonPos + 1 );
			else
				_ns = ns;
		}

		public string RootNamespace
		{
			get
			{
				int firstDot = _ns.IndexOf( '.' );
				if ( firstDot > -1 )
					return _ns.Substring( 0, firstDot );

				return _ns;
			}
		}

		public override string ToString()
		{
			return _ns;
		}
	}
}
