﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CQ.AuthProvider.BusinessLogic.Config.Firebase.Exceptions
{
    internal class FirebaseInitAppException : Exception
    {
        public string ProjectId { get; }

        public FirebaseInitAppException(string projectId, string? message = null) : base(message ?? $"Init of project {projectId} failed") { ProjectId = projectId; }
    }
}