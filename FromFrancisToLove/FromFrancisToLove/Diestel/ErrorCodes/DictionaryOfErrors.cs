using FromFrancisToLove.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FromFrancisToLove.Diestel.ErrorCodes
{
    public class DictionaryOfErrors
    {
        private readonly HouseOfCards_Context _context;

        public DictionaryOfErrors(HouseOfCards_Context context)
        {
            _context = context;
        }

        public static string ResponseCode()
        {
            return "";
        }
    }
}
