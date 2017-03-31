using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NDesk.Options;

namespace ReferenceVersionChecker {

	internal sealed class Arguments : MarshalByRefObject {

		public ISet<FileInfo> Assemblies = new HashSet<FileInfo>();
		public string Reference = null;
		public Version Version = null;
		public bool IgnoreBadImages = false;

		internal static readonly OptionSet<Arguments> Parser = new OptionSet<Arguments>()
			.Add(
				"assembly=",
				"The path to an assembly or a directory containing assemblies",
				( a, v ) => {

					if( File.Exists( v ) ) {
						a.Assemblies.Add( new FileInfo( v ) );

					} else if( Directory.Exists( v ) ) {

						IEnumerable<FileInfo> assemblies = new DirectoryInfo( v )
							.EnumerateFiles( "*.*", SearchOption.AllDirectories )
							.Where( IsAssembly );

						a.Assemblies.UnionWith( assemblies );

					} else {

						string msg = "Assembly file not found: " + v;
						throw new OptionException( msg, "assembly" );
					}
				}
			)
			.Add(
				"reference=",
				( a, v ) => {

					if( a.Reference != null ) {
						throw new OptionException( "Only a single reference can be checked", "reference" );
					}

					if( String.IsNullOrEmpty( v ) ) {
						throw new OptionException( "Reference cannot be empty", "reference" );
					}

					a.Reference = v;
				}
			)
			.Add(
				"version=",
				( a, v ) => {

					if( a.Version != null ) {
						throw new OptionException( "Only a single version can be checked", "version" );
					}

					if( String.IsNullOrEmpty( v ) ) {
						throw new OptionException( "Version cannot be empty", "version" );
					}

					a.Version = Version.Parse( v );
				}
			)
			.Add(
				"ignoreBadImages",
				( a, v ) => {
					a.IgnoreBadImages = true;
				}
			);

		private static bool IsAssembly( FileInfo file ) {

			switch( file.Extension.ToLowerInvariant() ) {

				case ".dll":
				case ".exe":
					return true;

				default:
					return false;
			}
		}
	}
}
