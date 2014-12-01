using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperSimple.Data
{
    public interface IConnectionStringManager
    {
        string ConnectionString { get; }
    }
}
