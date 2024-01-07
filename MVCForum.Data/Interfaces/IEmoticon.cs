using SnitzCore.Data.Models;
using System.Collections.Generic;

namespace SnitzCore.Data.Interfaces;

public interface IEmoticon
{
    Emoticon? GetByName(string name);
    IEnumerable<Emoticon> GetAll();
}