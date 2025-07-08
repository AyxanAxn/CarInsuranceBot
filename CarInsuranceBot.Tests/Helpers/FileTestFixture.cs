namespace CarInsuranceBot.Tests.Helpers;

public class FileTestFixture : IDisposable
{
    private readonly List<string> _createdFiles = new();
    private readonly List<string> _createdDirectories = new();

    public string CreateTestFile(string content = "test content", string extension = ".pdf")
    {
        var fileName = $"test_{Guid.NewGuid()}{extension}";
        var path = Path.Combine(Path.GetTempPath(), fileName);
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _createdDirectories.Add(directory);
        }
        
        File.WriteAllText(path, content);
        _createdFiles.Add(path);
        return path;
    }

    public string CreateTestDirectory(string name = null)
    {
        var directoryName = name ?? $"test_dir_{Guid.NewGuid()}";
        var path = Path.Combine(Path.GetTempPath(), directoryName);
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            _createdDirectories.Add(path);
        }
        
        return path;
    }

    public string CreateTestFileInDirectory(string directory, string fileName, string content = "test content")
    {
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
            _createdDirectories.Add(directory);
        }
        
        var path = Path.Combine(directory, fileName);
        File.WriteAllText(path, content);
        _createdFiles.Add(path);
        return path;
    }

    public void Dispose()
    {
        // Clean up files
        foreach (var file in _createdFiles)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }

        // Clean up directories (in reverse order to handle nested directories)
        foreach (var directory in _createdDirectories.OrderByDescending(d => d.Length))
        {
            try
            {
                if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
                    Directory.Delete(directory);
            }
            catch (Exception)
            {
                // Ignore cleanup errors
            }
        }
    }
} 