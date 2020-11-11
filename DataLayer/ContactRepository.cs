using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;

namespace DataLayer
{
    public class ContactRepository : IContactRepository
    {
        private IDbConnection db;

        public ContactRepository(string connString)
        {
            this.db = new SqlConnection(connString);
        }

        public Contact Find(int id)
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts WHERE Id = @Id", new { id }).SingleOrDefault();
        }

        public List<Contact> GetAll()
        {
            // Query method that maps to strongly typed objects.
            return this.db.Query<Contact>("Select * from Contacts").ToList();
        }

        public Contact Add(Contact contact)
        {
            var sql =
                "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES(@FirstName, @LastName, @Email, @Company, @Title); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            //When only insert is needed then Command in Dapper can be used. In this case Query gets the result too.
            var id = this.db.Query<int>(sql, contact).Single();
            contact.Id = id;
            return contact;
        }

        public Contact Update(Contact contact)
        {
            var sql =
                "UPDATE Contacts " +
                "SET FirstName = @FirstName, " +
                "    LastName  = @LastName, " +
                "    Email     = @Email, " +
                "    Company   = @Company, " +
                "    Title     = @Title " +
                "WHERE Id = @Id";
            this.db.Execute(sql, contact);
            return contact;
        }

        public void Remove(int id)
        {
            this.db.Execute("DELETE FROM Contacts WHERE Id = @Id", new { id });
        }

        public Contact GetFullContact(int id)
        {
            throw new System.NotImplementedException();
        }

        public void Save(Contact contact)
        {
            throw new System.NotImplementedException();
        }
    }
}
