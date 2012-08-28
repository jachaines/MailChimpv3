using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MailChimp;
using MailChimp.Types;


namespace MailChimpService.MailChimp
{
    public class MailChimp
    {
        #region Constants
        private const string IdField = "USERID";
        #endregion

        #region Private Enum
        private enum Status
        {
            subscribed,
            unsubscribed
        }
        #endregion

        #region Member Variables
        private string _apiKey;
        private string _configApiKey;
        private string _listId;
        private string _listName;
        private int _retryCount;
        private static List<MCMergeVar> _listColumns;
        private static MCApi _wrapper;
        private StringBuilder _reportBuilder;
        #endregion

        #region Properties
        public string Report { get { return _reportBuilder.ToString(); } }
        private static List<MCMergeVar> FixedColumns
        {
            get
            {
                return new List<MCMergeVar>
                           {
                              new MCMergeVar
                                       {
                                               name = "User Id",
                                               req = true,
                                               tag = "USERID"
                                       },
                                new MCMergeVar
                                       {
                                               name = "Email Address",
                                               req = true, 
                                               tag = "EMAIL"
                                     
                                       },
                            };
            }
        }

        private string apiKey
        {
            get
            {
                if (string.IsNullOrEmpty(_apiKey))
                {
                    var retryCount = 0;
                    while (retryCount < _retryCount)
                    {
                        try
                        {
                            _wrapper.ApiKey = _configApiKey;
                            _apiKey = _configApiKey;

                            if (string.IsNullOrEmpty(_apiKey))
                            {
                                throw new ConfigurationErrorsException(
                                        "The provided username and/or password is incorrect, or the provided API key is incorrect.");
                            }
                            retryCount = _retryCount;
                        }
                        catch
                        {
                            retryCount++;
                            if (retryCount >= _retryCount)
                            {
                                throw;
                            }
                        }
                    }
                }
                return _apiKey;
            }
        }

        private bool isLoggedIn
        {
            get { return !string.IsNullOrEmpty(apiKey); }
        }

        private string listId
        {
            get
            {
                if (string.IsNullOrEmpty(_listId))
                {
                    if (isLoggedIn)
                    {
                        var lists = _wrapper.Lists();
                        foreach (var list in lists.Data)
                        {
                            if (list.Name == _listName)
                            {
                                _listId = list.ListID;
                                //UpdateListColumns(_listId);
                                break;
                            }
                        }
                        if (string.IsNullOrEmpty(_listId))
                        {
                            throw new ConfigurationErrorsException("List name is not set correctly in the configuration file.");
                        }
                    }
                }
                return _listId;
            }
        }
        #endregion

        #region Constructor
        public MailChimp()
        {
            var config = MailChimpConfiguration.Load();
            _configApiKey = config.APIKey;
            _listName = config.ListName;
            _retryCount = config.RetryCount;
            _listColumns = config.ListColumns;
            _wrapper = new MCApi(_configApiKey, true);
        }

        public MailChimp(
            string apiKey,
            string listName,
            int retryCount,
            IEnumerable<MailChimpColumn> listColumns)
        {
           
            _configApiKey = apiKey;
            _wrapper = new MCApi(_configApiKey, true);
            _listName = listName;
            _retryCount = retryCount;
            _listColumns = new List<MCMergeVar>();

            foreach (var column in listColumns)
            {
                _listColumns.Add(new MCMergeVar
                {
                    name = column.Name,
                    req = column.Required,
                    tag = column.Tag
                });
            }

            GetMailChimpListMembers();
        }
        #endregion

        #region Public Methods

        #endregion

   
        #region Private Methods
        private List<List.MemberInfo> GetMailChimpListMembers()
        {
            var chimpMembersUnsubscribed = _wrapper.ListMembers(listId,List.MemberStatus.Unsubscribed);
            var chimpMembersSubscribed = _wrapper.ListMembers(listId, List.MemberStatus.Subscribed);
            var chimpMembers = chimpMembersSubscribed.Data.Union(chimpMembersUnsubscribed.Data);

            var chimpMemberInfos = new List<List.MemberInfo>();
            foreach (var member in chimpMembers)
            {
                var memberInfo = _wrapper.ListMemberInfo(listId, member.Email);
                chimpMemberInfos.Add(memberInfo);
            }
            return chimpMemberInfos;
        }
        #endregion
    }

}