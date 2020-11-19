using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataLayer;
using Microsoft.Extensions.Configuration;

namespace Runner
{
    class Program
    {
        private static IConfigurationRoot _config;

        
        static void Main()
        {
            //Initialize the connection string
            Initialize();

            //1. Get
            Get_all_should_return_6_results();

            //2. Insert
            var id = Insert_should_assign_identity_to_new_entity();

            //3. Find a record
            Find_should_retrieve_existing_entity(id);

            //4. Update the record
            Modify_should_update_existing_entity(id);

            //5. Delete the record
            Delete_should_remove_entity(id);

            //6. Get the multiple addresses for a contact. Complex object
            var repository = CreateRepository();
            var mj = repository.GetFullContact(1);
            mj.Output();

            //7. Usage of WHERE-IN
            List_support_should_produce_correct_results();

            //8. Usage of WHERE-IN with dynamic 
            Dynamic_support_should_produce_correct_results();

            //9. Bulk insert usage
            Bulk_insert_should_insert_4_rows();

            //10. Literal replacements. Only for numeric and boolean types
            GetIllinoisAddresses();

            //11. Multi mapping to eagerly load objects with parent-child relationships in a single query.
            Get_all_should_return_6_results_with_addresses();
        }

        //Rename this method to Main and the Main method to xMain  to test this
        // ReSharper disable once ArrangeTypeMemberModifiers
        // ReSharper disable once UnusedMember.Local
        static async Task XMain()
        {
            Initialize();

            //12. Using Async for any operations in Dapper. Async versions are available for all methods (query and execute).
            await Get_all_should_return_6_results_async();
        }

        //1. Get
        static void Get_all_should_return_6_results()
        {
            // arrange
            var repository = CreateRepository();

            // act
            var contacts = repository.GetAll();

            // assert
            Console.WriteLine($"Count: {contacts.Count}");
            Debug.Assert(contacts.Count == 6);
            contacts.Output();
        }

        //2. Insert
        static int Insert_should_assign_identity_to_new_entity()
        {
            // arrange
            IContactRepository repository = CreateRepository();
            var contact = new Contact
            {
                FirstName = "Joe",
                LastName = "Blow",
                Email = "joe.blow@gmail.com",
                Company = "Microsoft",
                Title = "Developer"
            };
            var address = new Address
            {
                AddressType = "Home",
                StreetAddress = "123 Main Street",
                City = "Baltimore",
                StateId = 1,
                PostalCode = "22222"
            };
            contact.Addresses.Add(address);

            // act
            //repository.Add(contact);

            //Complex object save
            repository.Save(contact);

            // assert
            Debug.Assert(contact.Id != 0);
            Console.WriteLine("*** Contact Inserted ***");
            Console.WriteLine($"New ID: {contact.Id}");
            return contact.Id;
        }

        //3. Find a single record
        static void Find_should_retrieve_existing_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            //var contact = repository.Find(id);
            
            //Complex object retrieval
            var contact = repository.GetFullContact(id);

            // assert
            Console.WriteLine("*** Get Contact ***");
            contact.Output();
            Debug.Assert(contact.FirstName == "Joe");
            Debug.Assert(contact.LastName == "Blow");
            Debug.Assert(contact.Addresses.Count == 1);
            Debug.Assert(contact.Addresses.First().StreetAddress == "123 Main Street");
        }

        //4. Modify a record
        static void Modify_should_update_existing_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            //var contact = repository.Find(id);
            var contact = repository.GetFullContact(id);
            contact.FirstName = "Bob";
            contact.Addresses[0].StreetAddress = "456 Main Street";
            //repository.Update(contact);

            //Modify a complex object
            repository.Save(contact);

            // create a new repository for verification purposes
            IContactRepository repository2 = CreateRepository();
            //var modifiedContact = repository2.Find(id);
            var modifiedContact = repository2.GetFullContact(id);

            // assert
            Console.WriteLine("*** Contact Modified ***");
            modifiedContact.Output();
            Debug.Assert(modifiedContact.FirstName == "Bob");
            Debug.Assert(modifiedContact.Addresses.First().StreetAddress == "456 Main Street");
        }

        //5. Delete a record
        static void Delete_should_remove_entity(int id)
        {
            // arrange
            IContactRepository repository = CreateRepository();

            // act
            repository.Remove(id);

            // create a new repository for verification purposes
            IContactRepository repository2 = CreateRepository();
            var deletedEntity = repository2.Find(id);

            // assert
            Debug.Assert(deletedEntity == null);
            Console.WriteLine("*** Contact Deleted ***");
        }

        //7. Usage of WHERE-IN
        static void List_support_should_produce_correct_results()
        {
            // arrange - Use extras repository
            var repository = CreateRepositoryEx();

            // act
            var contacts = repository.GetContactsById(1, 2, 4);

            // assert
            Debug.Assert(contacts.Count == 3);
            contacts.Output();
        }

        //8. Usage of WHERE-IN with dynamic 
        static void Dynamic_support_should_produce_correct_results()
        {
            // arrange
            var repository = CreateRepositoryEx();

            // act
            var contacts = repository.GetDynamicContactsById(1, 2, 4);

            // assert
            Debug.Assert(contacts.Count == 3);
            Console.WriteLine($"First FirstName is: {contacts.First().FirstName}");
            contacts.Output();
        }

        //9. Bulk insert usage
        static void Bulk_insert_should_insert_4_rows()
        {
            // arrange
            var repository = CreateRepositoryEx();
            var contacts = new List<Contact>
            {
                new Contact { FirstName = "Charles", LastName = "Barkley" },
                new Contact { FirstName = "Scottie", LastName = "Pippen" },
                new Contact { FirstName = "Tim", LastName = "Duncan" },
                new Contact { FirstName = "Patrick", LastName = "Ewing" }
            };

            // act
            var rowsAffected = repository.BulkInsertContacts(contacts);

            // assert
            Console.WriteLine($"Rows inserted: {rowsAffected}");
            Debug.Assert(rowsAffected == 4);
        }

        //10. Literal replacements. Only for numeric and boolean types
        static void GetIllinoisAddresses()
        {
            // arrange
            var repository = CreateRepositoryEx();

            // act
            var addresses = repository.GetAddressesByState(17);

            // assert
            Debug.Assert(addresses.Count == 2);
            addresses.Output();
        }

        //11. Multi mapping to eagerly load objects with parent-child relationships in a single query.
        static void Get_all_should_return_6_results_with_addresses()
        {
            var repository = CreateRepositoryEx();

            // act
            var contacts = repository.GetAllContactsWithAddresses();

            // assert
            Console.WriteLine($"Count: {contacts.Count}");
            contacts.Output();
            Debug.Assert(contacts.Count == 6);
            Debug.Assert(contacts.First().Addresses.Count == 2);
        }

        //12. Using Async for any operations in Dapper. Async versions are available for all methods (query and execute).
        static async Task Get_all_should_return_6_results_async()
        {
            // arrange
            var repository = CreateRepositoryEx();

            // act
            var contacts = await repository.GetAllAsync();

            // assert
            Console.WriteLine($"Count: {contacts.Count}");
            Debug.Assert(contacts.Count == 6);
            contacts.Output();
        }

       //////////////////////////////////////////Setup code///////////////////////////////////////////////////////////////

        private static void Initialize()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            _config = builder.Build();
        }

        private static IContactRepository CreateRepository()
        {
            //Using SQL queries
            //return new ContactRepository(config.GetConnectionString("DefaultConnection"));

            //Using Dapper Contrib
            //return new ContactRepositoryContrib(config.GetConnectionString("DefaultConnection"));

            //Using stored procedures
            return new ContactRepositorySp(_config.GetConnectionString("DefaultConnection"));
        }

        //Extras repository - IN Operator with WHERE, dynamic usage, bulk insert
        private static ContactRepositoryEx CreateRepositoryEx()
        {
            return new ContactRepositoryEx(_config.GetConnectionString("DefaultConnection"));
        }
    }
}
