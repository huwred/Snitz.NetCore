using SnitzCore.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces;

public interface IPrivateMessage
{
    PrivateMessage GetById(int id);
    IEnumerable<PrivateMessage> GetAll();
    IEnumerable<PrivateMessage> GetAll(int memberid);
    IEnumerable<PrivateMessage> GetInbox(int memberid);
    IEnumerable<PrivateMessage> GetOutbox(int memberid);
    Task Send(PrivateMessage pm);
    Task Delete(int pmid, int memberid);
    Task DeleteMany(IEnumerable<int> todelete, int memberid);
    Task Update(PrivateMessage pm);
    Task UpdateContent(int pmid, string content);

    IEnumerable<PrivateMessage> GetLatestPMs(int memberid, int count);
    IEnumerable<PrivateMessageBlocklist>? GetBlocklist(int memberid);
    IEnumerable<int> GetBlocksByName(string membername);
    bool BlockListAdd(int memberid, string toblock);
    void Create(PrivateMessage postmodel);
    IEnumerable<PrivateMessageListingModel> Find(int? curruser, string term, SearchIn searchIn, SearchFor phraseType, SearchDate searchByDays, int? memberId);
}