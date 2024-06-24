using SnitzCore.Data.Models;
using System.Collections.Generic;
using SnitzCore.Data.Extensions;

namespace SnitzCore.Data.Interfaces;

public interface IBookmark
{
    int MemberId { get; }
    BookmarkEntry? Get(int id);
    List<BookmarkEntry>? GetAll();

    List<BookmarkEntry> GetPaged(int pagenum, ActiveSince activesince, string lastVisitCookie,
        string memberSince);

    void AddBookMark(int topicId);
    void DeleteBookMark(int id);
    void DeleteBookMarks(IEnumerable<int> bookmarks);

    bool IsBookmarked(int topicid);
}