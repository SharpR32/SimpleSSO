using HeimdallMini.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HeimdallMini.Domain.Entities
{
    public class User: AuditableEntity
    {
        public string UserName { get; init; } = string.Empty;
        public byte[] PasswordHash { get; protected set; } = Array.Empty<byte>();

        public ICollection<Login> Logins { get; protected set; }


        private const int SALT_LENGTH = 32;

        internal User() { }
        public User(string userName, string password)
        {
            UserName = userName;

            var salt = ComputeSalt(true);
            this.PasswordHash = salt.Concat(ComputePasswordHash(password, salt)).ToArray();
        }
        /// <summary>
        /// Adds new Login data to database
        /// </summary>
        /// <param name="ip"></param>
        /// <returns><see cref="DateTime"/> of login.</returns>
        public DateTime Login(string ip)
        {
            Logins ??= new List<Login>();
            var login = new Login { IpAddress = ip };
            Logins.Add(login);
            return login.Created;
        }

        public bool ChangePassword(string oldPassword, string newPassword)
        {
            if (!Validate(oldPassword))
                return false;

            var salt = ComputeSalt(true);
            this.PasswordHash = salt.Concat(ComputePasswordHash(newPassword, salt)).ToArray();

            return true;
        }

        public bool Validate(string password)
        {
            var salt = ComputeSalt();
            var challenger = ComputePasswordHash(password, salt);

            return this.PasswordHash[32..].SequenceEqual(challenger);
        }

        protected virtual IEnumerable<byte> ComputeSalt(bool newSalt = false)
        {
            using var sha256 = SHA256.Create();
            if (newSalt)
            {
                var salt = DateTime.UtcNow.ToString();
                var saltBytes = Encoding.UTF8.GetBytes(salt);
                return sha256.ComputeHash(saltBytes);
            }

            return this.PasswordHash[..SALT_LENGTH];
        }

        protected virtual IEnumerable<byte> ComputePasswordHash(string password, IEnumerable<byte> salt)
        {
            using var sha256 = SHA256.Create();
            IEnumerable<byte> passwordSpan = Encoding.UTF8.GetBytes(password);
            var passwordHash = sha256.ComputeHash(passwordSpan.Concat(salt).ToArray());

            return passwordHash;
        }
    }
}
