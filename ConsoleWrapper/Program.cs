using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Mono.Options;

namespace ConsoleWrapper {
  /// <summary>
  ///   The container class for the entry point of the application.
  /// </summary>
  internal class Program {
    /// <summary>
    ///   Command line options for the application
    /// </summary>
    private static class CommandLineOptions {
      /// <summary>
      ///   The application that should be started by the console wrapper.
      /// </summary>
      internal static string Subject { get; set; }

      /// <summary>
      ///   The desired width of the console window.
      /// </summary>
      internal static int Width { get; set; }

      /// <summary>
      ///   The desired height of the console window.
      /// </summary>
      internal static int Height { get; set; }

      /// <summary>
      ///   Should the command line help be displayed?
      /// </summary>
      internal static bool ShowHelp { get; set; }
    }

    private static Process SubjectProcess { get; set; }

    /// <summary>
    ///   The entry point for the console wrapper application.
    /// </summary>
    private static void Main( string[] args ) {
      if( ParseCommandLine( args ) ) {
        return;
      }

      if( string.IsNullOrEmpty( CommandLineOptions.Subject ) ) {
        Console.Error.WriteLine( "No subject given!" );
        return;
      }

      if( CommandLineOptions.Width <= 0 ) {
        CommandLineOptions.Width = 80;
      }
      if( CommandLineOptions.Height <= 0 ) {
        CommandLineOptions.Height = 25;
      }

      // Main operations
      Console.WindowWidth = CommandLineOptions.Width;
      Console.WindowHeight = CommandLineOptions.Height;

      string subjectBinary = BinaryFromSubject( CommandLineOptions.Subject );
      string subjectArguments = CommandLineOptions.Subject.Substring( subjectBinary.Length + 1 );

      ProcessStartInfo subjectStartInfo = new ProcessStartInfo();
      subjectStartInfo.FileName = subjectBinary;
      subjectStartInfo.Arguments = subjectArguments;
      subjectStartInfo.RedirectStandardError = true;
      subjectStartInfo.RedirectStandardOutput = true;
      subjectStartInfo.UseShellExecute = false;

      SubjectProcess = new Process();
      SubjectProcess.StartInfo = subjectStartInfo;
      SubjectProcess.EnableRaisingEvents = true;
      SubjectProcess.ErrorDataReceived += ( sender, eventArgs ) => Console.Error.WriteLine( eventArgs.Data );
      SubjectProcess.OutputDataReceived += ( sender, eventArgs ) => Console.WriteLine( eventArgs.Data );

      Console.CancelKeyPress += ( sender, eventArgs ) => {
        // Cancel the Ctrl+C action for our application.
        // The Ctrl+C will still pass through to the hosted application.
        // This will cause it to exit and then our application will exit as well.
        eventArgs.Cancel = true;
      };

      SubjectProcess.Start();

      SubjectProcess.BeginErrorReadLine();
      SubjectProcess.BeginOutputReadLine();

      SubjectProcess.WaitForExit();
    }

    /// <summary>
    ///   Given a string that represents a filename (of an executable) followed by an optional set of parameters,
    ///   the method will return the name of the executable only.
    /// </summary>
    /// <param name="subject">The line containing both the executable and the parameters.</param>
    /// <returns>The full path of the executable only.</returns>
    /// <exception cref="ArgumentException">No valid target executable could be extracted from the subject line.</exception>
    private static string BinaryFromSubject( string subject ) {
      // If the provided subject is already just a file (without parameters), then just return it back.
      if( File.Exists( subject ) ) {
        return subject;
      }

      // Split the subject line at every space, as that is the default token delimiter on the console.
      string[] tokens = subject.Split( new[] {' '} );
      string currentlyChecking = string.Empty;

      for( int tokenIndex = 0; tokenIndex < tokens.Length; ++tokenIndex ) {
        // For every token after the first, we need to add back a space (because we removed those during the splitting).
        if( tokenIndex > 0 ) {
          currentlyChecking += ' ';
        }
        // Add the current token to the string to check.
        currentlyChecking += tokens[ tokenIndex ];

        // See if the current string is a file.
        if( File.Exists( currentlyChecking ) ) {
          return currentlyChecking;
        }
      }

      throw new ArgumentException( "No valid target executable could be extracted from the subject line." );
    }


    /// <summary>
    ///   Parses command line parameters.
    /// </summary>
    /// <param name="args">
    ///   The command line parameters passed to the program.
    /// </param>
    /// <returns>
    ///   <see langword="true" /> if the application should exit.
    /// </returns>
    private static bool ParseCommandLine( IEnumerable<string> args ) {
      OptionSet options = new OptionSet {
        {"subject=", "The application that should be started by the console wrapper.", v => CommandLineOptions.Subject = v},
        {"width=", "The desired width of the console window.", v => CommandLineOptions.Width = int.Parse( v )},
        {"height=", "The desired height of the console window.", v => CommandLineOptions.Height = int.Parse( v )},
        {"h|?|help", "Shows this help message", v => CommandLineOptions.ShowHelp = v != null}
      };

      List<string> extraParameters;
      try {
        extraParameters = options.Parse( args );
      } catch( OptionException ex ) {
        Console.Write( "{0}:", new FileInfo( Assembly.GetExecutingAssembly().Location ).Name );
        Console.WriteLine( ex.Message );
        Console.WriteLine(
          "Try '{0} --help' for more information.", new FileInfo( Assembly.GetExecutingAssembly().Location ).Name );
        return true;
      }

      if( !string.IsNullOrEmpty( CommandLineOptions.Subject ) && extraParameters.Any() ) {
        Console.Error.Write( "Unexpected parameters on command line:" );
        foreach( string extraParameter in extraParameters ) {
          Console.WriteLine( "- {0}", extraParameter );
        }
        return true;
      } else {
        // Construct subject from extra parameters given.
        // This allows the user to just put the subject as the last parameter(s) on the command line.
        CommandLineOptions.Subject = String.Join( " ", extraParameters.ToArray() );
      }

      if( CommandLineOptions.ShowHelp ) {
        Console.WriteLine( "Usage: {0} [OPTIONS]", new FileInfo( Assembly.GetExecutingAssembly().Location ).Name );
        Console.WriteLine();
        Console.WriteLine( "Options:" );
        Console.WriteLine();
        options.WriteOptionDescriptions( Console.Out );
        return true;
      }

      return false;
    }
  }
}