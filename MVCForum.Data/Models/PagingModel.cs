﻿namespace SnitzCore.Data.Models
{
    public class PagingModel
    {
        public string? OrderBy { get; set; } = "lpd";
        public string? SortDir { get; set; } = "des";
        public int PageSize { get; set; }
        public int Page { get; set; } = 1;

        public int PageCount { get; set; } = 0;

        public int DefaultDays { get; set; } = 30;
    }
}
