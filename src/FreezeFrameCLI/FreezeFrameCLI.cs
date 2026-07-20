// Minimal named-pipe client GUI (PoC only — no error hardening).
//
// Build/run:
//   dotnet run
//
// Edit PipeName below to match your server's pipe name.

using System.IO.Pipes;

ApplicationConfiguration.Initialize();
Application.Run(new MainForm());

public class MainForm : Form
{
    private readonly TextBox _output;
    private readonly TextBox _input;
    private NamedPipeClientStream? _pipe;
    private StreamWriter? _writer;

    public MainForm()
    {
        Text = "Named Pipe Client";
        Width = 600;
        Height = 500;

        _output = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            Dock = DockStyle.Fill,
        };

        _input = new TextBox
        {
            Dock = DockStyle.Bottom,
        };
        _input.KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SendLine();
            }
        };

        Controls.Add(_output);
        Controls.Add(_input);

        Shown += async (s, e) => await ConnectAsync();
    }

    private async Task ConnectAsync()
    {
        try
        {
            _pipe = new NamedPipeClientStream(".", "FreezeFrameTAS", PipeDirection.InOut, PipeOptions.Asynchronous);
            Log("[connecting to FreezeFrameTAS...]");
            await _pipe.ConnectAsync();

            _writer = new StreamWriter(_pipe) { AutoFlush = true };
            Log("[connected]");

            _ = Task.Run(ReadLoop);
        }
        catch (Exception ex)
        {
            Log($"[connection failed: {ex.Message}]");
        }
    }

    private void ReadLoop()
    {
        if (_pipe is null) return;
        using var reader = new StreamReader(_pipe);
        try
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                Log(line);
            }
        }
        catch (Exception ex)
        {
            Log($"[read error: {ex.Message}]");
        }
        Log("[disconnected]");
    }

    private void SendLine()
    {
        if (_writer is null) return;
        string line = _input.Text;
        try
        {
            _writer.WriteLineAsync(line);
            _input.Clear();
        }
        catch (Exception ex)
        {
            Log($"[send failed: {ex.Message}]");
        }
    }

    private void Log(string text)
    {
        if (_output.InvokeRequired)
        {
            _output.Invoke(() => Log(text));
            return;
        }
        _output.AppendText(text + Environment.NewLine);
    }
}