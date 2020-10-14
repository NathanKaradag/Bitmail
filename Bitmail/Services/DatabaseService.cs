using Bitmail.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bitmail.Services
{
    public class DatabaseService
    {
        public readonly ApplicationDbContext DB;

        public DatabaseService(ApplicationDbContext context)
        {
            DB = context;
        }
    }
}