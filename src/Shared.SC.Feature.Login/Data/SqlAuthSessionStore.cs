using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataHandler;

using Shared.SC.Feature.Login.Models;

namespace Shared.SC.Feature.Login.Data
{
    /// <summary>
    /// Session store to be used with the OWIN cookie authentication middleware
    /// </summary>
    public class SqlAuthSessionStore : IAuthenticationSessionStore
    {
        private readonly string _connectionString;

        private readonly TicketDataFormat _formatter;

        public SqlAuthSessionStore(TicketDataFormat tdf)
            : this(tdf, "AuthSessionStoreContext")
        {
        }

        public SqlAuthSessionStore(TicketDataFormat tdf, string cns)
        {
            _connectionString = cns;
            _formatter = tdf;
        }

        public Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            string key = Guid.NewGuid().ToString();
            using (SqlAuthSessionStoreContext store = new SqlAuthSessionStoreContext(_connectionString))
            {
                store.Entries.Add(new AuthSessionEntry { Key = key, TicketString = _formatter.Protect(ticket), ValidUntil = ticket.Properties.ExpiresUtc });
                store.SaveChanges();
            }

            return Task.FromResult(key);
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            if (ticket == null)
            {
                throw new ArgumentNullException(nameof(ticket));
            }

            using (SqlAuthSessionStoreContext store = new SqlAuthSessionStoreContext(_connectionString))
            {
                AuthSessionEntry entry = store.Entries.FirstOrDefault(a => a.Key == key);
                if (entry != null)
                {
                    entry.TicketString = _formatter.Protect(ticket);
                    entry.ValidUntil = ticket.Properties.ExpiresUtc;
                }
                else
                {
                    store.Entries.Add(new AuthSessionEntry { Key = key, TicketString = _formatter.Protect(ticket), ValidUntil = ticket.Properties.ExpiresUtc });
                }

                store.SaveChanges();
            }

            return Task.FromResult(0);
        }

        public Task<AuthenticationTicket> RetrieveAsync(string key)
        {
            AuthenticationTicket ticket = null;
            using (SqlAuthSessionStoreContext store = new SqlAuthSessionStoreContext(_connectionString))
            {
                AuthSessionEntry entry = store.Entries.FirstOrDefault(a => a.Key == key);
                if (entry != null)
                {
                    ticket = _formatter.Unprotect(entry.TicketString);
                }

                return Task.FromResult(ticket);
            }
        }

        public Task RemoveAsync(string key)
        {
            using (SqlAuthSessionStoreContext store = new SqlAuthSessionStoreContext(_connectionString))
            {
                AuthSessionEntry entry = store.Entries.FirstOrDefault(a => a.Key == key);
                if (entry != null)
                {
                    store.Entries.Remove(entry);
                    store.SaveChanges();
                }
            }

            return Task.FromResult(0);
        }
    }
}