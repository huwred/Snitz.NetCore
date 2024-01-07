using Microsoft.Extensions.Configuration;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Reflection;

namespace SnitzCore.Data.Extensions
{
    public class SnitzTableAttribute : TableAttribute
    {

        public SnitzTableAttribute(string name, string prefix)
            : base(ModifyName(name, prefix))
        {

        }
        public static string ModifyName(string name, string prefix)
        {
            string path = "";
            string? forumTablePrefix = null;
            string? memberTablePrefix = null;
            try
            {
                var assemblypath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
                if (assemblypath != null)
                {
                    path = assemblypath.Replace("bin\\Debug\\net7.0", "");
                }
                if (!string.IsNullOrWhiteSpace(path))
                {
                    var builder = new ConfigurationBuilder()
                        .SetBasePath(path)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                    var config = builder.Build();

                    forumTablePrefix = config.GetSection("SnitzForums").GetSection("forumTablePrefix").Value;
                    memberTablePrefix = config.GetSection("SnitzForums").GetSection("memberTablePrefix").Value;
                }
            }
            catch (Exception)
            {
                //supress any errors as we have a default;
            }


            //return prefix + name;
            if (prefix == "FORUM")
                prefix = forumTablePrefix ?? "FORUM_";
            if (prefix == "MEMBER")
                prefix = memberTablePrefix ?? "FORUM_";

            return prefix + name;
        }
    }
}
