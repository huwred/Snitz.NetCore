using System;
using Microsoft.EntityFrameworkCore;

namespace SnitzCore.Data.Extensions;

public static class DbContextExtensions
{

    public static object? Set(this DbContext _context, Type t)
    {
        return _context.GetType().GetMethod("Set")?.MakeGenericMethod(t).Invoke(_context, null);
    }
}
    public class Pair<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }
    }