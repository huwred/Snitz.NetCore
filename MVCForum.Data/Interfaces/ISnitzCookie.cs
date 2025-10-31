using System;

namespace SnitzCore.Data.Interfaces
{
    public interface ISnitzCookie
    {
        void LogOut();
        void Clear(string value);
        void ClearAll();
        string? GetLastVisitDate();
        void SetLastVisitCookie(string value);
        string? GetActiveRefresh();
        void SetActiveRefresh(string value);
        string? GetTopicSince();
        void SetTopicSince(string value);
        string? GetCookieValue(string cookieKey);
        void SetCookie(string name, string value, DateTime? expires = null);
        void PollVote(int pollid);
        bool HasVoted(int pollid,int? memberid);
        void UpdateTopicTrack(string topicId);
        string? Tracked(string topicId);
        void ClearTracking(string topicId);

        string? CookieUser();
    }
}