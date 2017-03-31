using System;
using System.IO;
using System.Reflection;

namespace ReferenceVersionChecker {

	internal sealed class AssemblyReferenceChecker : MarshalByRefObject {

		internal void Check(
				string assemblyPath,
				Arguments arguments
			) {

			Func<Version, bool> versionChecker = CreateVersionChecker( arguments );

			try {
				Assembly assembly = Assembly.ReflectionOnlyLoadFrom( assemblyPath );
				AssemblyName[] references = assembly.GetReferencedAssemblies();

				foreach( AssemblyName reference in references ) {

					if( reference.Name.Equals( arguments.Reference, StringComparison.Ordinal ) ) {

						bool validVersion = versionChecker( reference.Version );
						if( !validVersion ) {

							Console.Out.WriteLine( assemblyPath );
							Console.Out.WriteLine( "  > {0}", reference.FullName );
							Console.Out.WriteLine();
						}
					}
				}

			} catch( BadImageFormatException ) {

				if( !arguments.IgnoreBadImages ) {

					Console.Error.WriteLine( assemblyPath );
					Console.Error.WriteLine( "  > Bad image format" );
					Console.Error.WriteLine();
				}

			} catch( FileLoadException ) {

				Console.Error.WriteLine( assemblyPath );
				Console.Error.WriteLine( "  > File load exception" );
				Console.Error.WriteLine();
			}
		}

		private static Func<Version, bool> CreateVersionChecker( Arguments arguments ) {

			if( arguments.Version != null ) {
				return ( version ) => version.Equals( arguments.Version );
			} else {
				return ( version ) => true;
			}
		}
	}
}
