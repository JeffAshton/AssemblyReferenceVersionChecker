
using System;
using System.Collections.Generic;
using System.IO;
using NDesk.Options;

namespace ReferenceVersionChecker {

	internal sealed class Program {

		internal static int Main( string[] arguments ) {

			Arguments args;

			try {
				List<string> extras;
				args = Arguments.Parser.Parse( arguments, out extras );

				if( extras.Count > 0 ) {

					Console.Error.WriteLine(
							"Invalid arguments: {0}",
							String.Join( " ", extras )
						);
					Console.Error.WriteLine();

					Arguments.Parser.WriteOptionDescriptions( Console.Out );
					return -1;
				}

			} catch( OptionException err ) {

				Console.Error.WriteLine( "Invalid arguments: " );
				Console.Error.WriteLine( err.Message );
				Console.Error.WriteLine();

				Arguments.Parser.WriteOptionDescriptions( Console.Out );
				return -1;
			}

			if( args.Assemblies.Count == 0 || args.Reference == null ) {

				Arguments.Parser.WriteOptionDescriptions( Console.Out );
				return -1;
			}

			Run( args );
			return 0;
		}


		private static void Run( Arguments args ) {

			Type checkerType = typeof( AssemblyReferenceChecker );
			string checkerAssemblyName = checkerType.Assembly.GetName().Name;
			string checkerTypeName = checkerType.FullName;
			string checkerLocation = Path.GetDirectoryName( checkerType.Assembly.Location );

			foreach( FileInfo file in args.Assemblies ) {

				AppDomainSetup setup = new AppDomainSetup();
				setup.ApplicationBase = checkerLocation;

				AppDomain appDomain = AppDomain.CreateDomain( file.FullName, null, setup );

				try {
					AssemblyReferenceChecker checker = (AssemblyReferenceChecker)appDomain
						.CreateInstanceAndUnwrap(
							assemblyName: checkerAssemblyName,
							typeName: checkerTypeName
						);

					checker.Check( file.FullName, args );

				} finally {
					AppDomain.Unload( appDomain );
				}
			}
		}
	}
}
