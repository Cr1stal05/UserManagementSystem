using System;
using System.Collections.Generic;

namespace UserManagementSystem.DTOs
{
    public class BulkActionRequest
    {
        public List<Guid> UserIds { get; set; } = new List<Guid>();
        public string Action { get; set; }  // "block", "unblock", "delete", "deleteUnverified"
    }
}