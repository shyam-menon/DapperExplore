using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DataLayer
{
    public class ContactRepositoryEx
    {
        private IDbConnection db;

        public ContactRepositoryEx(string connString)
        {
            this.db = new SqlConnection(connString);
        }

        //Usage of IN clause for WHERE
        public List<Contact> GetContactsById(params int[] ids)
        {
            return this.db.Query<Contact>("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids }).ToList();
        }

        //Usage of IN clause for WHERE with the generic return type not specified (Contact) and use dynamic type instead
        public List<dynamic> GetDynamicContactsById(params int[] ids)
        {
            return this.db.Query("SELECT * FROM Contacts WHERE ID IN @Ids", new { Ids = ids }).ToList();
        }

        //Usage of bulk insert
        //Note that this is a syntax optimization and not a performance optimization as this makes round trips to DB
        public int BulkInsertContacts(List<Contact> contacts)
        {
            var sql =
                "INSERT INTO Contacts (FirstName, LastName, Email, Company, Title) VALUES(@FirstName, @LastName, @Email, @Company, @Title); " +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            //Execute method understand that this is an array
            return this.db.Execute(sql, contacts);
        }

        //Literal replacements (used with numeric and Boolean types to increase performance)
        public List<Address> GetAddressesByState(int stateId)
        {
            return this.db.Query<Address>("SELECT * FROM Addresses WHERE StateId = {=stateId}", new { stateId }).ToList();
        }

        //Multi mapping to eagerly load objects with parent-child relationships in a single query.
        public List<Contact> GetAllContactsWithAddresses()
        {
            var sql = "SELECT * FROM Contacts AS C INNER JOIN Addresses AS A ON A.ContactId = C.Id";

            var contactDict = new Dictionary<int, Contact>();

            //Query has the first parameter as the parent, second as the child and the third as the return type
            //As there is a one to many relationship between contacts and addresses, the check is needed below
            //before adding items to dictionary.
            var contacts = this.db.Query<Contact, Address, Contact>(sql, (contact, address) =>
            {
                //C# 7 feature that allows to get value from dictionary with an out variable
                if (!contactDict.TryGetValue(contact.Id, out var currentContact))
                {
                    currentContact = contact;
                    contactDict.Add(currentContact.Id, currentContact);
                }

                currentContact.Addresses.Add(address);
                return currentContact;
            });
            return contacts.Distinct().ToList();
        }

        //Async call to Query. Async methods for execute are also available
        public async Task<List<Contact>> GetAllAsync()
        {
            var contacts = await this.db.QueryAsync<Contact>("SELECT * FROM Contacts");
            return contacts.ToList();
        }
    }
}
