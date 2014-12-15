using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigSharp
{
    public interface ILoader
    {
        string Load(string fileName);
    }
}
