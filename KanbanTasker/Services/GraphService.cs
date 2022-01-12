﻿using KanbanTasker.Helpers.Microsoft_Graph;
using KanbanTasker.Helpers.Microsoft_Graph.Authentication;
using KanbanTasker.Helpers.Microsoft_Graph.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KanbanTasker.Services
{
    public class GraphService
    {
        private string[] scopes = new string[] { "files.readwrite", "user.read" };
        private string appId = "422b281b-be2b-4d8a-9410-7605c92e4ff1";
        private static AuthenticationProvider _authProvider;
        private GraphServiceHelper _graphServiceHelper;

        public GraphService()
        {
            _authProvider = new AuthenticationProvider(appId, scopes);
            _graphServiceHelper = new GraphServiceHelper(_authProvider);
        }

        public AuthenticationProvider AuthenticationProvider
        {
            get => _authProvider;
        }

        public OneDriveRequests OneDrive
        {
            get => _graphServiceHelper.OneDrive;
        }

        public UserRequests User
        {
            get => _graphServiceHelper.User;
        }
    }
}