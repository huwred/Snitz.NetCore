using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SnitzCore.Data;
using SnitzCore.Data.Extensions;
using SnitzCore.Data.Interfaces;
using SnitzCore.Data.Models;

namespace SnitzCore.Service
{
    public class BookmarkService : IBookmark
    {
        private readonly SnitzDbContext _dbContext;

        public int MemberId { get; set; }

        public BookmarkService(SnitzDbContext dbContext,IMember memberService,IHttpContextAccessor httpContextAccessor)
        {
            _dbContext = dbContext;
            var userId = httpContextAccessor.HttpContext?.User.Identity?.Name;
            if (userId != null)
            {
                MemberId = memberService.GetByUsername(userId)!.Id;
            }
            

        }
        public BookmarkEntry? GetEntryById(int id)
        {
            return _dbContext.Bookmarks.Include(b=>b.Member).Include(b=>b.Topic).AsNoTrackingWithIdentityResolution().First(b=>b.Id == id && b.MemberId == MemberId);
        }

        public List<BookmarkEntry>? GetAllEntries()
        {
            return _dbContext.Bookmarks.Include(b=>b.Member).Include(b=>b.Topic).ThenInclude(t=>t.Forum).AsNoTrackingWithIdentityResolution().Where(b=>b.MemberId == MemberId).ToList();
        }

        public List<BookmarkEntry> GetPaged(int pagenum, ActiveSince activesince, string lastVisitCookie, string memberSince)
        {
            throw new NotImplementedException();
        }

        public void AddBookMark(int topicId)
        {
            var newbookmark = new BookmarkEntry
            {
                TopicId = topicId,
                MemberId = MemberId
            };
            _dbContext.Bookmarks.Add(newbookmark);
            _dbContext.SaveChanges();

        }

        public void DeleteBookMark(int id)
        {
            var bookmark = _dbContext.Bookmarks.FirstOrDefault(b=>b.TopicId==id && b.MemberId == MemberId);
            if (bookmark != null)
            {
                _dbContext.Bookmarks.Remove(bookmark);
                _dbContext.SaveChanges();
            }

        }

        public void DeleteBookMarks(IEnumerable<int> bookmarks)
        {
            foreach (int bookmarkid in bookmarks)
            {
                var bookmark = _dbContext.Bookmarks.Find(bookmarkid);
                if (bookmark != null)
                {
                    _dbContext.Bookmarks.Remove(bookmark);

                }
            }
            _dbContext.SaveChanges();
        }

        public bool IsBookmarked(int topicid)
        {
            var res = _dbContext.Bookmarks.FirstOrDefault(b=>b.TopicId == topicid && b.MemberId == MemberId);
            return res != null;
        }
    }
}
