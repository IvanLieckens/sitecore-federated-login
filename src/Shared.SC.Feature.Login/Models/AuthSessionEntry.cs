using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.SC.Feature.Login.Models
{
    /// <summary>
    /// Model for use with the session authentication store. data will be stored into database
    /// </summary>
    public class AuthSessionEntry
    {
        [Key]
        public string Key { get; set; }

        public DateTimeOffset? ValidUntil { get; set; }

        public string TicketString { get; set; }
    }
}