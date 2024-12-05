using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using System.Linq;

public class BookmarkManager
{
    private string _bookmarkPath;
    public ObservableCollection<Bookmark> Bookmarks { get; private set; }

    public BookmarkManager(string cachePath)
    {
        // Set the bookmark path from the provided cache path
        _bookmarkPath = Path.Combine(cachePath, "bookmarks.json");

        // Initialize Bookmarks to avoid null reference issues
        Bookmarks = new ObservableCollection<Bookmark>();
        LoadBookmarks();
    }

    public void AddBookmark(string title, string url)
    {
        // Check if the bookmark already exists
        if (Bookmarks.Any(b => b.Title == title && b.URL == url))
        {
            // Optionally, you can log or notify the user that the bookmark already exists
            return;
        }

        var bookmark = new Bookmark { Title = title, URL = url };
        Bookmarks.Add(bookmark);
        SaveBookmarks();
    }

    public void RemoveBookmark(Bookmark bookmark)
    {
        Bookmarks.Remove(bookmark);
        SaveBookmarks();
    }
   
    private void SaveBookmarks()
    {
        var json = JsonSerializer.Serialize(Bookmarks);
        File.WriteAllText(_bookmarkPath, json);
    }

    private void LoadBookmarks()
    {
        if (File.Exists(_bookmarkPath))
        {
            var json = File.ReadAllText(_bookmarkPath);
            Bookmarks = JsonSerializer.Deserialize<ObservableCollection<Bookmark>>(json)
                        ?? new ObservableCollection<Bookmark>(); // Ensure Bookmarks is not null
        }
    }
}
