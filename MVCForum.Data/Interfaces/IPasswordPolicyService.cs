using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnitzCore.Data.Interfaces
{
    public interface IPasswordPolicyService
    {
        List<string> GetPasswordRequirements();

        string GeneratePassword();
    }
}
