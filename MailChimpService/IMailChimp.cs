using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MailChimp;
using MailChimp.Types;

namespace MailChimpService.MailChimp
{
    internal class MCBatchResult
    {
            public int success_count;
            public int error_count;
            public MCEmailResult[] errors;
    }

    internal class MCEmailResult
    {
        public int code;
        public string message;
        public MCMemberInfo row;

    }

    internal class MCMemberInfo
    {
        public string email;
        public string email_type;
        public List.MergeVar[] merges;
        public string status;
        public string timestamp;
    }

    internal class MCMergeVar
    {
        public string tag;
        public string name;
        public bool req;
        public string val;
    }



}
