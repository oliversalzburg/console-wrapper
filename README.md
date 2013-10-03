console-wrapper
===============

A small tool to open another console application in a window of a given size.

### Usage

    Usage: console-wrapper.exe [OPTIONS]
    
    Options:
    
          --subject=VALUE        The application that should be started by the
                                   console wrapper.
          --width=VALUE          The desired width of the console window.
          --height=VALUE         The desired height of the console window.
      -h, -?, --help             Shows this help message

Providing a subject through the `--subject` parameter is optional. The parameter can also be omitted. In that case all remaining parameters on the command line are treated as the subject.

The path to the given subject executable has to be explicit. No relative paths (or `PATH` lookup) is supported at this time.

### Example

#### Explicit
    console-wrapper.exe --width=180 --height=40 --subject="C:\Program Files\nodejs\node.exe P:\GitHub\fairy\app.js"

#### Simple
    console-wrapper.exe --width=180 --height=40 C:\Program Files\nodejs\node.exe P:\GitHub\fairy\app.js