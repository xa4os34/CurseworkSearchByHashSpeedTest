using System.Security.Cryptography;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Exporters;

BenchmarkRunner.Run<SearchBenchmark>();

[RPlotExporter]
public class SearchBenchmark
{
    private readonly SHA256 _sha256 = SHA256.Create();
    private readonly LinkedList<Entry> _entries = new();
    private readonly string[] _words = 
        File.ReadAllText("words.txt")
        .Split(' ', 
            StringSplitOptions.RemoveEmptyEntries | 
            StringSplitOptions.TrimEntries);

    public IEnumerable<string> Words => 
        _words.Skip(100000)
        .Take(1)
        .ToArray();

    [GlobalSetup]
    public void Setup()
    {
        for (var i = 0; i < _words.Length; i++)
        {
            string word = _words[i];
            byte[] wordBytes = Encoding.UTF8.GetBytes(word);
            
            var entry = new Entry(
                hashSha256: _sha256.ComputeHash(wordBytes),
                customHash: word.GetHashCode(),
                text: word,
                index: i);
            
            _entries.AddLast(entry);
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Words))]
    public Entry[] SearchAllBySha256(string word)
    {
        byte[] wordBytes = Encoding.UTF8.GetBytes(word);
        byte[] hash = _sha256.ComputeHash(wordBytes);

        return _entries
            .Where(x => 
                x.HashSha256.SequenceEqual(hash) && 
                x.Text == word)
            .ToArray();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Words))]
    public Entry[] SearchByCustomHash(string word)
    {
        int hash = word.GetHashCode();
        
        return _entries
            .Where(x => 
                hash.Equals(x.CustomHash) && 
                word.Equals(x.Text))
            .ToArray();
    }
    
    [Benchmark]
    [ArgumentsSource(nameof(Words))]
    public Entry[] SearchByText(string word)
    {
        return _entries
            .Where(x => 
                word.Equals(x.Text))
            .ToArray();
    }
    
}

public struct Entry
{
    public readonly byte[] HashSha256; 
    public readonly int CustomHash; 
    public readonly string Text;
    public readonly long Index;

    public Entry(byte[] hashSha256, int customHash, string text, long index)
    {
        HashSha256 = hashSha256;
        CustomHash = customHash;
        Text = text;
    }
}