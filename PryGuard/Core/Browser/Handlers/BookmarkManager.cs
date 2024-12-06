using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.IO;
using System.Linq;

public class BookmarkManager
{
    private readonly string _bookmarkFilePath;

    /// <summary>
    /// Collection of bookmarks managed by this class.
    /// </summary>
    public ObservableCollection<Bookmark> Bookmarks { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BookmarkManager"/> class.
    /// </summary>
    /// <param name="cachePath">The directory path to store the bookmarks file.</param>
    public BookmarkManager(string cachePath)
    {
        if (string.IsNullOrWhiteSpace(cachePath))
            throw new ArgumentException("Cache path cannot be null or empty.", nameof(cachePath));

        _bookmarkFilePath = Path.Combine(cachePath, "bookmarks.json");
        Bookmarks = new ObservableCollection<Bookmark>();
        LoadBookmarks();
    }

    /// <summary>
    /// Adds a new bookmark to the collection.
    /// </summary>
    /// <param name="title">The title of the bookmark.</param>
    /// <param name="url">The URL of the bookmark.</param>
    public void AddBookmark(string title, string url)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Title and URL cannot be null or empty.");

        if (Bookmarks.Any(b => b.Title.Equals(title, StringComparison.OrdinalIgnoreCase) && b.URL.Equals(url, StringComparison.OrdinalIgnoreCase)))
        {
            // Optionally log or notify the user that the bookmark already exists
            return;
        }

        var newBookmark = new Bookmark { Title = title, URL = url };
        Bookmarks.Add(newBookmark);
        SaveBookmarks();
    }

    /// <summary>
    /// Removes a bookmark from the collection.
    /// </summary>
    /// <param name="bookmark">The bookmark to remove.</param>
    public void RemoveBookmark(Bookmark bookmark)
    {
        if (bookmark == null)
            throw new ArgumentNullException(nameof(bookmark), "Bookmark cannot be null.");

        if (Bookmarks.Remove(bookmark))
        {
            SaveBookmarks();
        }
    }

    /// <summary>
    /// Saves the current collection of bookmarks to the bookmarks file.
    /// </summary>
    private void SaveBookmarks()
    {
        try
        {
            var json = JsonSerializer.Serialize(Bookmarks, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_bookmarkFilePath, json);
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.WriteLine($"Failed to save bookmarks: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads bookmarks from the bookmarks file.
    /// </summary>
    private void LoadBookmarks()
    {
        try
        {
            if (File.Exists(_bookmarkFilePath))
            {
                var json = File.ReadAllText(_bookmarkFilePath);
                Bookmarks = JsonSerializer.Deserialize<ObservableCollection<Bookmark>>(json)
                            ?? new ObservableCollection<Bookmark>();
            }
        }
        catch (Exception ex)
        {
            // Handle or log the exception as needed
            Console.WriteLine($"Failed to load bookmarks: {ex.Message}");
        }
    }
}
