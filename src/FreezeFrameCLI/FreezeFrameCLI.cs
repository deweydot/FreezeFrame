using System;
using System.IO;
using System.IO.Pipes;

using var pipe = new NamedPipeClientStream(".", "FreezeFrameTAS", PipeDirection.InOut);
Console.WriteLine("Connecting to ULTRAKILL...");
pipe.Connect();
using var reader = new StreamReader(pipe);
using var writer = new StreamWriter(pipe) { AutoFlush = true };

Console.WriteLine("Connected to FreezeFrame plugin.");
while (true) {
    string input = Console.ReadLine();
    if (string.IsNullOrEmpty(input)) continue;
    writer.WriteLine(input);
    string response = reader.ReadLine();
    Console.WriteLine(response);
}